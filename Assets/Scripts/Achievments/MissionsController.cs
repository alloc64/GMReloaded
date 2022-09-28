/************************************************************************
 * Copyright (c) 2014 Milan Jaitner                                     *
 * This program is free software: you can redistribute it and/or modify *
 * it under the terms of the GNU General Public License as published by *
 * the Free Software Foundation, either version 3 of the License, or    * 
 * any later version.													*
																		*
 * This program is distributed in the hope that it will be useful,      *
 * but WITHOUT ANY WARRANTY; without even the implied warranty of       *
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the         *
 * GNU General Public License for more details.							*
																		*
 * You should have received a copy of the GNU General Public License	*
 * along with this program.  If not, see http://www.gnu.org/licenses/	*
 ***********************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_XBOXONE
using GMReloaded.Xbox.Linq;
#else
using System.Linq;
#endif

namespace GMReloaded.Achievements
{
	public class MissionsController : MonoSingletonPersistent<MissionsController>, IAchievableItemCallback
	{
		//

		private Stats.PersistentStats _stats;
		public Stats.PersistentStats stats
		{
			get
			{ 
				if(_stats == null)
					_stats = new Stats.PersistentStats();
					
				return _stats;
			}
		}

		//

		private Dictionary<string, Mission> _missions = new Dictionary<string, Mission>();
		public Dictionary<string, Mission> missions 
		{ 
			get 
			{ 
				if(_missions == null) 
					_missions = new Dictionary<string, Mission>(); 
				return _missions;
			}
		}

		#region Missions

		private int missionsCounter = 0;

		private Mission _nextMission = null;
		public Mission nextMission
		{
			get
			{ 
				if(_nextMission == null)
					_nextMission = SelectNextMission();
			
				return _nextMission;
			}
		}

		#endregion

		//

		private GameMessage gameMessage { get { return HUD.IsNull ? null : HUD.Instance.gameMessage; } }

		//

		#region Unity

		protected override void Awake()
		{
			base.Awake();

			foreach(var mission in Config.missions.missionList)
			{
				RegisterItem(mission.key, mission);
			}
		}

		#endregion

		private void RegisterItem(string key, Mission mission)
		{
			mission.SetCallback(this);
			mission.SetId(missionsCounter++);

			missions.Add(key, mission);
		}

		private Mission GetMission(string key)
		{
			if(key == null)
				return null;

			Mission m = null;

			missions.TryGetValue(key, out m);

			return m;
		}


		public void IncrementMission(string key, int incr = 1)
		{
			if(tutorial.isActive && key != Config.Missions.MissionIDs.Mission_123)
				return;

			var mission = GetMission(key);

			if(mission == null)
			{
				Debug.LogWarning("Mission not found ! " + key);
				return;
			}

			mission.progress += incr;
		}

		public void IncrementMissionWithCallbackOnCompletion(string key, int incr, System.Action callback)
		{
			var mission = GetMission(key);

			bool wasInactive = mission != null && !mission.activated;

			IncrementMission(key, incr);

			if(callback != null && wasInactive && mission != null && mission.activated)
			{
				callback();
			}
		}

		public virtual bool IsMissionActivated(string key)
		{
			var mission = GetMission(key);

			if(mission == null)
			{
				Debug.LogWarning("Mission not found ! " + key);
				return false;
			}

			return mission.activated;
		}

		#region IAchievableItemCallback

		public bool IndicateItemProgress(string key, int progress, int maxProgress)
		{
			//Vola se callbackem pouze pokud se progress pricetl == je validni

			if(progress <= maxProgress) 
			{
				#if !UNITY_METRO && STEAM_ENABLED
				Steam.Steamworks.Instance.IndicateAchievementProgress(key, progress, maxProgress);
				#endif

				return true;
			}

			return false;
		}

		public void ActivateItem(AchievableItemBase item)
		{
			var mission = item as Mission;
			var key = mission.key;

			#if !UNITY_METRO && STEAM_ENABLED
			Steam.Steamworks.Instance.SetAchievement(key);
			#endif

			if(!HUD.IsNull)
			{
				if(gameMessage != null)
				{
					gameMessage.SetMissionInfoBox(mission);
				}

				var chatConsole = HUD.Instance.chatConsole;

				if(chatConsole != null)
					chatConsole.SubmitMessage(localization.GetValue(mission.isAchievement ? "Achievement_Unlocked" : "Mission_Unlocked") + localization.GetValue(key));
			}

			GMReloaded.Analytics.GAI.Instance.LogEvent("Game", "Mission", key, 1);

			if(LocalClientRobotEmil.Instance != null)
			{
				int exp = mission.experience;

				if(gameMessage != null)
					gameMessage.SetXPMessage(exp);

				LocalClientRobotEmil.Instance.OnLocalScoreChanged(mission.experience);
			}

			Timer.DelayAsync(1f, () =>
			{
				_nextMission = null; // refresh

				SetMissionIndicator(nextMission);
			});
		}

		#endregion

		private Mission SelectNextMission()
		{
			#if UNITY_XBOXONE
			foreach(var kvp in missions.Missions_OrderBy_AOT((firstPair, nextPair) => firstPair.Value.experience.CompareTo(nextPair.Value.experience)))
			#else
			foreach(var kvp in missions.OrderBy(mKvp => mKvp.Value.experience))
			#endif
			{
				var m = kvp.Value;

				if(m == null || m.activated || m.experience <= 0 || m.isIgnoredInProgressBar)
					continue;

				return m;
			}

			return null;
		}

		private void SetMissionIndicator(Mission mission)
		{
			var hud = HUD.Instance;

			if(hud != null)
			{
				hud.missionIndicator.SetCurrRoundMission(mission);
			}
		}
	}
}