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
using GMReloaded.UI.Final;

namespace GMReloaded
{
	public class GameStateController : MonoSingleton<GameStateController> 
	{
		public string serverName;

		private HUD hud { get { return HUD.Instance; } }

		private PlayerStatsTable statsTable { get { return hud == null ? null : hud.playerStats; } }

		private ArenaEventDispatcher arenaEventDispatcher { get { return ArenaEventDispatcher.Instance; } }

		private KBMenuRenderer menuRenderer { get { return KBMenuRenderer.IsNull ? null : KBMenuRenderer.Instance; } }

		protected Achievements.MissionsController missions { get { return Achievements.MissionsController.Instance; } }

		//

		private double endTime = 0f;

		private string arenaId;

		//

		public void SetArena(string arenaId)
		{
			this.arenaId = arenaId;

			if(statsTable != null)
				statsTable.SetArena(arenaId);
		}

		public void SetServerName(object serverName, bool setHint = true)
		{
			this.serverName = serverName.ToString();

			if(menuRenderer != null)
			{
				if(setHint)
				{
					menuRenderer.SetLoadingHintText(Config.hints.GetRandom());
				}
			}

			if(statsTable != null)
				statsTable.SetServerName(this.serverName);
		}

		public void SetLevelEndTime(object obj)
		{
			double.TryParse(obj.ToString(), out endTime);
			   
			Debug.Log("level start time " + endTime);
		}

		public void LeaveRoom()
		{
			PhotonNetwork.LeaveRoom();
		}

		private int lastTime = -1;
		private bool endOfRoundStarted = false;
		private bool showScoreTableStarted = false;
		private bool arenaReloadStarted = false;

		private void Update()
		{
			if(endTime > 0f && PhotonNetwork.room != null)
			{
				int time = (int)(endTime - PhotonNetwork.time);

				if(lastTime != time)
				{
					arenaEventDispatcher.madnessMode.UpdateGameTime(time);

					if(time >= 0)
					{
						if(hud != null)
							hud.SetArenaTime(time);

						//

						float playedTime = LocalClientRobotEmil.user.playedTime;

						if(playedTime >= 3000 * 60)
						{
							if(!missions.IsMissionActivated(Config.Missions.MissionIDs.Mission_67))
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_67, 1);
						}
						else if(playedTime >= 1600 * 60)
						{
							if(!missions.IsMissionActivated(Config.Missions.MissionIDs.Mission_66))
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_66, 1);
						}
						else if(playedTime >= 800 * 60)
						{
							if(!missions.IsMissionActivated(Config.Missions.MissionIDs.Mission_65))
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_65, 1);
						}
						else if(playedTime >= 400 * 60)
						{
							if(!missions.IsMissionActivated(Config.Missions.MissionIDs.Mission_64))
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_64, 1);
						}
						else if(playedTime >= 200 * 60)
						{
							if(!missions.IsMissionActivated(Config.Missions.MissionIDs.Mission_63))
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_63, 1);
						}
						else if(playedTime >= 100 * 60)
						{
							if(!missions.IsMissionActivated(Config.Missions.MissionIDs.Mission_62))
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_62, 1);
						}
						else if(playedTime >= 50 * 60)
						{
							if(!missions.IsMissionActivated(Config.Missions.MissionIDs.Mission_61))
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_61, 1);
						}
						else if(playedTime >= 10 * 60)
						{
							if(!missions.IsMissionActivated(Config.Missions.MissionIDs.Mission_60))
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_60, 1);
						}


						//

						if(PhotonNetwork.isMasterClient)
						{
							if(time <= Config.gameReloadShowScoretableTime + 3 && !endOfRoundStarted)
							{
								// konec kola, dojde k reloadu
								arenaEventDispatcher.OnStartEndOfRoundMaster();
								endOfRoundStarted = true;
							}
							else if(time <= Config.gameReloadShowScoretableTime && !showScoreTableStarted)
							{
								arenaEventDispatcher.OnShowScoreTableMaster();
								showScoreTableStarted = true;
							}
							else if(time < 2 && !arenaReloadStarted)
							{
								missions.stats.SetCompletedArena(arenaId);

								arenaEventDispatcher.OnStartArenaReloadMaster();
								arenaReloadStarted = true;
							}
						}
					}
					else if(time <= -10)
					{
						Debug.LogError("time <= -10 - forced leave room");
						LeaveRoom();
					}

					lastTime = time;
				}
			}
		}
	}
}
