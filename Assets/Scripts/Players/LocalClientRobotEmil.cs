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
using CodeStage.AntiCheat.ObscuredTypes;

namespace GMReloaded
{
	//Singleton debile
	public class LocalClientRobotEmil : MonoBehaviourTO
	{
		public class User
		{
			#region Settings

			private const string playerSkinKey = "LCRP_Skin";
			public RobotEmil.Skin skin
			{
				get
				{ 
					if(!Cloud.CloudSyncedPlayerPrefs.HasKey(playerSkinKey))
						return RobotEmil.Skin.Red;

					return (RobotEmil.Skin)Cloud.CloudSyncedPlayerPrefs.GetInt(playerSkinKey);
				}

				set
				{ 
					Cloud.CloudSyncedPlayerPrefs.SetInt(playerSkinKey, (int)value);			
				}
			}

			private const string playerNickKey = "LCRP_Nick";
			public string nick
			{
				get 
				{ 
					string nick = PlayerPrefs.GetString(playerNickKey); 
					return ValidateNick(nick) ? nick : null; 
				}

				set 
				{ 
					var savedNick = value;

					bool validNick = ValidateNick(savedNick);

					if(validNick)
					{
						PlayerPrefs.SetString(playerNickKey, savedNick); 
						UpdateNick();
					}
					else
					{
						Debug.LogError("Failed to save nick " + savedNick);
					}
				}
			}

			private const string playerColorKey = "LCRP_Color";
			public Color color
			{
				get
				{ 
					int colorId = Cloud.CloudSyncedPlayerPrefs.GetInt(playerColorKey, -1);
					var colors = Config.Player.colors;

					if(colorId < 0 || colorId >= colors.Length)
					{
						colorId = Random.Range(0, colors.Length);
						Cloud.CloudSyncedPlayerPrefs.SetInt(playerColorKey, colorId);
					}

					return colors[colorId];
				}
			}

			public void UpdateNick()
			{
				PhotonNetwork.playerName = nick; 

				if(!HUD.IsNull)
				{
					var hud = HUD.Instance;

					if(hud != null && hud.playerStats != null)
					{
						hud.playerStats.UpdateChangedPlayerNick(PhotonNetwork.player);
					}
				}
			}

			public bool ValidateNick(string value) { return value != null && value.Length >= 3; }

			private const string playedTimeKey = "LCRP_PlayedTimeKey";
			public float playedTime
			{
				get { return ObscuredPrefs.GetFloat(playedTimeKey, 0f); }
				set { ObscuredPrefs.SetFloat(playedTimeKey, value); }
			}

			#endregion
		}

		public static readonly User user = new User();

		//

		public static LocalClientRobotEmil _instance;

		public static LocalClientRobotEmil Instance
		{
			get
			{
				if(_instance == null)
					_instance = FindObjectOfType<LocalClientRobotEmil>();

				return _instance;
			}
		}

		protected ArenaEventDispatcher arenaEventDispatcher { get { return ArenaEventDispatcher.Instance; } }

		protected Achievements.MissionsController missions { get { return Achievements.MissionsController.Instance; } }

		private HUD hud { get { return HUD.Instance; } }


		#region Login

		public static Dictionary<string, string> loginCredentials
		{
			get
			{ 
				var _nick = user.nick;

				return new Dictionary<string, string> 
				{
					{ "login", WWW.EscapeURL(_nick)  },
					{ "pass", Utils.MD5(_nick + SystemInfo.deviceUniqueIdentifier) }
				};
			}
		}

		#endregion

		#region Exps

		private const int maxLevel = 9999;

		private const string levelKey = "LCRELevelKey_R0.1";
		private static int _level = -1;
		public static int level
		{
			get
			{
				if(_level == -1)
				{
					if(!Cloud.CloudSyncedPlayerPrefs.HasKey(levelKey))
						level = 0;
					
					_level = Cloud.CloudSyncedPlayerPrefs.GetInt(levelKey);

					if(_level < 0)
						level = 0;
				}

				return _level;
			}

			set
			{ 
				if(value > maxLevel)
					value = maxLevel;

				if(value < 0)
					value = 0;

				if(_level == value)
					return;

				_level = value;

				Cloud.CloudSyncedPlayerPrefs.SetInt(levelKey, _level);
			}
		}

		private const string levelPointsKey = "LCRELevelPointsKey_R0.1";
		public static int _levelPoints = -1;
		public static int levelPoints 
		{
			get
			{ 
				if(_levelPoints < 0)
				{
					_levelPoints = Cloud.CloudSyncedPlayerPrefs.GetInt(levelPointsKey);

					if(_levelPoints <= 0)
						_levelPoints = Config.Exp.GetLevelDesiredPoints(0);
				}
			
				return _levelPoints;
			}

			set
			{ 
				if(value == _levelPoints)
					return;
			
				_levelPoints = value;

				if(_levelPoints < 0)
					_levelPoints = 0;

				Debug.Log("_levelPoints " + _levelPoints);

				Cloud.CloudSyncedPlayerPrefs.SetInt(levelPointsKey, _levelPoints);
			}
		}

		public static int nextLevelPoints
		{
			get
			{
				return Config.Exp.GetLevelDesiredPoints(level + 1);
			}
		}

		public static float levelProgress
		{
			get
			{
				if(level >= maxLevel)
					return 0f;

				float currLevelPoints = (float)Config.Exp.GetLevelDesiredPoints(level);

				float progress = Mathf.Abs(currLevelPoints - ((float)levelPoints)) / (((float)nextLevelPoints) - currLevelPoints);

				if(float.IsNaN(progress))
					return 0f;

				return Mathf.Clamp01(progress);
			}
		}

		public static int madnessPoints 
		{ 
			get 
			{ 
				int pointsFromLevels = Config.MadnessMode.GetMadnessPointsInLevel(level);
				int pointsFromMissions = Config.missions.GetMadnessPointsFromCompletedMissions();

				return pointsFromLevels + pointsFromMissions; 
			} 
		}
			
		#endregion

		[SerializeField]
		public RobotEmilNetworked parentRobot;

		private RobotEmilDeathCamEffect deathCamEffect;

		#region Load & Spawning

		public void OnSetClientTypeLocalClient()
		{
		}

		public virtual void Spawn(RobotEmilNetworked parentRobot)
		{
			this.parentRobot = parentRobot;

			OnLocalScoreChanged(0);
		}

		public void OnFirstSpawn()
		{
			arenaEventDispatcher.OnArenaPlayerReady();

			UpdateScreenEffectsAvailability();
		}

		#endregion

		#region Exps

		public void OnLocalScoreChanged(int diff)
		{
			GMReloaded.API.Score.Instance.LogRelativeExperience(diff);

			levelPoints += diff;

			if(levelPoints < Config.Exp.GetLevelDesiredPoints(level))
			{
				OnExpLevelChanged(-1);
			}

			//tady je stara hodnota
			if(levelPoints >= Config.Exp.GetLevelDesiredPoints(level + 1))
			{
				OnExpLevelChanged(1);
			}

			parentRobot.OnLocalScoreChanged(level, levelPoints, Config.Exp.GetLevelDesiredPoints(level + 1), levelProgress);
		}

		public void OnLocalKillsChanged(int diff)
		{
			GMReloaded.API.Score.Instance.LogRelativeKills(diff);
		}

		public void OnLocalDeathsChanged(int diff)
		{
			GMReloaded.API.Score.Instance.LogRelativeDeaths(diff);
		}

		private void OnExpLevelChanged(int diff)
		{
			level += diff;

			GMReloaded.Analytics.GAI.Instance.LogEvent("Game", "Level", ((diff < 0) ? "Down" : "Up"), level);
				                                           
			parentRobot.OnLevelChanged(diff);

			if(diff > 0)
			{
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_112, 1);
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_113, 1);
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_114, 1);
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_115, 1);
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_116, 1);
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_117, 1);
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_118, 1);
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_119, 1);
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_121, 1);
			}
		}

		#endregion

		public void OnStartEndOfRound(int numAlive, RobotEmil bestBoxDestroyer)
		{
			//TODO: handling rozdělení lokálních bodů

			if(parentRobot.state != RobotEmil.State.Dead && parentRobot.state != RobotEmil.State.Unspawned)
			{
				switch(numAlive)
				{
					case 1:
						parentRobot.OnIsLastSurvivor();
					break;

					case 2:		
						parentRobot.OnIsSecondSurvivor();
					break;
						
					case 3:		
						parentRobot.OnIsThirdSurvivor();
					break;
				}
			}

			if(parentRobot == bestBoxDestroyer)
			{
				parentRobot.OnIsBestBoxDestroyer();
			}
		}

		public void ReequipWeapons()
		{
			if(parentRobot != null)
				parentRobot.ReequipWeapons();
			else
				Debug.LogError("Failed to reequip weapons - parentRobot == null");
		}

		public void ForceWeaponAtSlotIdx(int idx, WeaponType weaponType)
		{
			Config.Weapons.localClientEquipedWeapons.SetWeaponAtSlot(idx, weaponType);

			ReequipWeapons();
		}

		public void VerifyEquips()
		{
			Config.Weapons.localClientEquipedWeapons.VerifySlots();
		}

		#region Settings

		public enum ScreenEffects
		{
			DOF,
			HDR,
			AntiAliasing
		}

		private void UpdateScreenEffectsAvailability()
		{
			foreach(ScreenEffects effect in System.Enum.GetValues(typeof(ScreenEffects)))
			{
				UpdateScreenEffectAvailability(effect);
			}
		}

		public void UpdateScreenEffectAvailability(ScreenEffects effects)
		{
			switch(effects)
			{
				case ScreenEffects.DOF:

					var dof = RobotEmilImageEffects.Instance.dof;

					if(dof != null)
					{
						var amount = Config.ClientPersistentSettings.DOF;
						
						dof.enabled = amount != Config.ClientPersistentSettings.DOFAmount.None;
						dof.aperture = 12f + (0.5f * ((int)(amount-1)));
					}
					
				break;

				case ScreenEffects.HDR:

					var ie = RobotEmilImageEffects.Instance;

					if(ie != null)
						ie.SetHDREnabled(Config.ClientPersistentSettings.HDR);
					
				break;
					
				case ScreenEffects.AntiAliasing:
					
					var aa = RobotEmilImageEffects.Instance.antialiasing;

					if(aa != null)
						aa.enabled = Config.ClientPersistentSettings.AntiAliasing;
					
				break;

			}
		}

		#endregion

		protected override void OnDestroy()
		{
			base.OnDestroy();

			_instance = null;
		}
	}
}
