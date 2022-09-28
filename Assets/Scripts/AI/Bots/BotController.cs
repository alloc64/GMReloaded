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
using System.Linq;

namespace GMReloaded.AI.Bots
{
	public class BotController
	{
		private PlayerInitialSpawn _playerInitialSpawn;

		private HUD hud { get { return HUD.Instance; } }

		private HashSet<RobotEmilBotClient> botInstances = new HashSet<RobotEmilBotClient>();

		public BotController(PlayerInitialSpawn playerInitialSpawn)
		{
			this._playerInitialSpawn = playerInitialSpawn;
		}

		public List<RobotEmilBotClient> SpawnBots(PhotonPlayer botOwner, int botCount)
		{
			var room = PhotonNetwork.room;

			if(room == null)
				return null;

			botCount = Mathf.Clamp(botCount, 0, room.maxPlayers);

			int maxBotCount = (botCount - room.playerCount);

			if(maxBotCount < 0)
				maxBotCount = 0;

			Debug.Log("Spawning " + maxBotCount + " bots");

			List<RobotEmilBotClient> list = new List<RobotEmilBotClient>();

			for(int i = 0; i < maxBotCount; i++)
			{
				var bot = SpawnBot(botOwner);

				if(bot != null)
					list.Add(bot);
			}

			return list;
		}

		public RobotEmilBotClient SpawnBot(PhotonPlayer botOwner)
		{
			if(_playerInitialSpawn == null)
			{
				Debug.LogError("SpawnBot _playerInitialSpawn == null");
				return null;
			}

			var robot = RobotEmilNetworked.LoadPlayerRobot(_playerInitialSpawn);

			if(robot == null)
			{
				Debug.LogError("Failed to spawn bot robot == null");
				return null;
			}

			int botId = botInstances.Count;

			var botInstance = robot.SetBotClient(this, botOwner, Config.Bots.defaultPhotonId+botId, botId);

			botInstances.Add(botInstance);

			OnBotConnected(botInstance);

			return botInstance;
		}

		public void RemoveBotOnDestroy(RobotEmilBotClient bot)
		{
			OnBotDisconnected(bot);

			botInstances.Remove(bot);
		}

		//

		private void OnBotConnected(RobotEmilBotClient bot)
		{
			if(hud != null)
				hud.OnPlayerConnectedAED(bot.photonPlayer);
		}

		private void OnBotDisconnected(RobotEmilBotClient bot)
		{
			OnBotDisconnected(bot.photonPlayer);
		}

		private void OnBotDisconnected(PhotonPlayer photonPlayer)
		{
			if(hud != null)
				hud.OnPlayerDisconnectedAED(photonPlayer);
		}

		//

		private List<PhotonPlayer> remoteBotPhotonPlayers = new List<PhotonPlayer>();

		public void OnRemoteBotConnected(PhotonPlayer player)
		{
			if(!remoteBotPhotonPlayers.Contains(player))
				remoteBotPhotonPlayers.Add(player);
		}

		//

		public void OnPhotonPlayerConnected(PhotonPlayer player)
		{
			var room = PhotonNetwork.room;

			if(room == null)
				return;

			Debug.Log("Check bot to disconnects " + room.playerCount + " + " + botInstances.Count);
			
			if(room.playerCount + botInstances.Count >= room.maxPlayers)
			{
				var lastBot = botInstances.Last();

				if(lastBot == null)
				{
					Debug.LogError("Failed to remove bot - lastBot == null");
				}
				else
				{
					lastBot.Destroy();
				}
			}
		}

		public void OnPhotonPlayerDisconnected(PhotonPlayer player)
		{
			for(int i = 0; i < remoteBotPhotonPlayers.Count; i++)
			{
				var rbpp = remoteBotPhotonPlayers[i];

				if(rbpp != null)
				{
					var robotInstance = PlayersController.Instance.Get(rbpp.ID);
					if(robotInstance != null)
					{
						if(robotInstance.botOwner == player)
						{
							Debug.Log("Found bot owner disconnect");
							OnBotDisconnected(rbpp);

							remoteBotPhotonPlayers.RemoveAt(i);
						}
					}
				}
			}
		}
	}

}