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
using GMReloaded.UI;
using System;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;
using System.Collections.Generic;

using GMReloaded.Entities;
using GMReloaded.UI.Final;

namespace GMReloaded
{
	public class ArenaEventDispatcher : Photon.PunBehaviour
	{
		public static class PropsConstants
		{
			public const string LevelEndTimeKey = "LevelEndTime";
			public const string ServerKey = "Server";
			public const string MadnessKey = "Madness";
			public const string LevelKey = "Level";
			public const string RoundTimeKey = "RoundTime";
		}

		//

		[SerializeField]
		private PlayerInitialSpawn playerInitialSpawn;

		[SerializeField]
		private GameObject arenaModel;

		[SerializeField]
		private Bounds arenaBounds;

		//

		private string arenaId;

		private GMReloaded.Config.Arenas.ArenaConfig arenaConfig;

		//

		private float trapsUpdateTimer = 0f;

		private float trapsUpdateTime = 30f;

		private List<Traps.TrapConfig> trapConfigs;

		//

		private GlobalStateController stateController { get { return GlobalStateController.Instance; }}

		private GameStateController gameStateController { get { return GameStateController.Instance; }}

		private KBMenuRenderer menuRenderer { get { return KBMenuRenderer.IsNull ? null : KBMenuRenderer.Instance; } }

		//

		private PlayerStatsTable playerStatsTable { get { return HUD.IsNull ? null : HUD.Instance.playerStats; } }

		private GameInfoStack gameInfoStack { get { return HUD.IsNull ? null : HUD.Instance.gameInfoStack; } }

		private RoomPropertiesController rpc { get { return RoomPropertiesController.Instance; } }

		private PlayersController playerController { get { return PlayersController.Instance; } }

		private SpawnManager spawnManager { get { return SpawnManager.Instance; } }

		protected Bonuses.BonusController bonusController { get { return Bonuses.BonusController.Instance; } }

		protected GMReloaded.Tutorial.TutorialManager tutorial { get { return GMReloaded.Tutorial.TutorialManager.Instance; } }

		private HUD hud { get { return HUD.Instance; } }

		private static ArenaEventDispatcher _instance = null;

		public static ArenaEventDispatcher Instance 
		{
			get
			{ 
				if(_instance == null) 
					_instance = FindObjectOfType<ArenaEventDispatcher>();

				return _instance;
			}
		}

		#region Madness Mode

		public Madness.MadnessModeController madnessMode = new Madness.MadnessModeController();

		#endregion

		#region Bots

		public AI.Bots.BotController _botController = null;
		public AI.Bots.BotController botController
		{
			get
			{ 
				if(_botController == null)
					_botController = new GMReloaded.AI.Bots.BotController(playerInitialSpawn);

				return _botController;
			}
		}

		#endregion

		#region Unity
		
		protected virtual void Awake()
		{
		}

		protected virtual void Update()
		{
			if(PhotonNetwork.isMasterClient)
				UpdateTrapsDispatch();
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(arenaBounds.center, arenaBounds.extents * 2f);
		}

		#endregion

		#region Setup

		private void SetArenaId(object value)
		{
			this.arenaId = value.ToString();

			gameStateController.SetArena(arenaId);

			Config.Arenas.arenaConfig.TryGetValue(arenaId, out arenaConfig);

			if(arenaConfig == null)
			{
				Debug.LogError("SetArenaId - arenaConfig not found " + arenaId);
				return;
			}

			trapConfigs = arenaConfig.traps.ToList();
		}

		#endregion

		#region Bots

		public void OnRemoteBotConnected(PhotonPlayer photonPlayer)
		{
			Debug.Log("ArenaEventDispatcher.OnRemoteBotConnected" + photonPlayer);

			if(!HUD.IsNull)
				hud.OnPlayerConnectedAED(photonPlayer);

			if(botController != null)
				botController.OnRemoteBotConnected(photonPlayer);
		}

		#endregion

		#region Chat

		public void SubmitChatMessage(string message)
		{
			var robot = LocalClientRobotEmil.Instance.parentRobot;

			photonView.RPC("OnChatMessageReceived", PhotonTargets.All, (robot.color.ToTK2DColor() + robot.nick + Color.white.ToTK2DColor() + ": " + message));
		}

		[PunRPC]
		protected virtual void OnChatMessageReceived(string text)
		{
			if(hud != null && hud.chatConsole != null)
				hud.chatConsole.OnChatMessageReceived(text);
		}

		#endregion

		#region Robot Emil Bonuses

		public void GlobalSlowdownUsed(bool used, RobotEmilNetworked robotParent)
		{
			robotParent.GlobalSlowdownUsed(true, false);
			photonView.RPC("OnRemoteGlobalSlowdownUsed", PhotonTargets.Others, used, robotParent.photonPlayerId);
		}

		[PunRPC]
		protected virtual void OnRemoteGlobalSlowdownUsed(bool used, int ownerPhotonPlayerId)
		{
			//Debug.Log("OnRemoteGlobalSlowdownUsed used:" + used);

			foreach(var kvp in playerController.Objects)
			{
				var p = kvp.Value;
				if(p != null && p.photonPlayerId != ownerPhotonPlayerId)
				{
					p.GlobalSlowdownUsed(used, true);
				}
			}
		}

		public void TeleportAcquiredPositions(byte[] acquiredByteData)
		{
			photonView.RPC("OnRemoteTeleportUsed", PhotonTargets.All, acquiredByteData);
		}

		[PunRPC]
		protected virtual bool OnRemoteTeleportUsed(byte [] acquiredRandomPositionsBytes)
		{
			var props = StructSerializer.Deserialize<GMReloaded.TeleportBonusImpl.TeleportProperties>(acquiredRandomPositionsBytes);

			if(props.numActivePlayers < 1 || props.teleportData == null)
			{
				Debug.LogError("Received invalid teleport data");
				return false;
			}

			foreach(var kvp in playerController.Objects)
			{
				var p = kvp.Value;
				if(p != null)// && p.isLocalClient)
				{
					for(int i = 0; i < props.numActivePlayers; i++)
					{
						var td = props.teleportData[i];

						Debug.Log(td.photonPlayerID + " == " + p.photonPlayerId);

						if(td.photonPlayerID == p.photonPlayerId)
						{
							p.OnTeleported(td.position);
							//return true;
							break;
						}
					}

					//Debug.LogWarning("Unable teleport player " + p.name);
				}
			}

			return false;
		}

		#endregion

		#region Arena Reloads

		private void UpdateArenaLevelEndTime()
		{
			if(!PhotonNetwork.isMasterClient)
				return;
			
			Hashtable properties = PhotonNetwork.room.customProperties;

			object o = null;

			properties.TryGetValue(PropsConstants.RoundTimeKey, out o);

			if(o != null)
			{
				int roomTime = -1;

				int.TryParse(o.ToString(), out roomTime);

				if(roomTime > 0)
				{
					int t = roomTime + Config.Arenas.roomTimeMargin;

					properties[PropsConstants.LevelEndTimeKey] = PhotonNetwork.time + t; 

					PhotonNetwork.room.SetCustomProperties(properties);
				}
			}
		}

		public static void OnArenaChange(string arenaId, int roundTime)
		{
			Debug.Log("OnArenaChange " + arenaId + " - " + roundTime);

			Hashtable properties = PhotonNetwork.room.customProperties;

			var propertiesKeys = properties.Keys.ToList();

			foreach(var key in propertiesKeys)
			{
				if(key == null || key.ToString() == PropsConstants.ServerKey || key.ToString() == PropsConstants.MadnessKey || key.ToString() == PropsConstants.RoundTimeKey)
					continue;

				//Debug.Log("removing room property " + key);
				properties[key] = null;
			}

			if(roundTime < Config.Arenas.minRoundTime)
				roundTime = Config.Arenas.roundTimeDefault;

			if(PhotonNetwork.isMasterClient)
			{
				properties[PropsConstants.RoundTimeKey] = roundTime;
				//tady zalezi na poradi
				properties[PropsConstants.LevelEndTimeKey] = PhotonNetwork.time + roundTime + Config.Arenas.roomTimeMargin; 
				properties[PropsConstants.LevelKey] = arenaId;
			}

			PhotonNetwork.room.SetCustomProperties(properties);
		}

		public void ReloadArena()
		{
			if(PhotonNetwork.isMasterClient)
			{
				string newArena = null;
				int n = 0;

				do
				{
					newArena = Config.Arenas.availableArenaSceneIds.GetRandom();

					if(!Application.CanStreamedLevelBeLoaded(newArena))
						newArena = null;
				}
				while(newArena == null && n++ < 10);

				int roundTime = Config.Arenas.roundTimeDefault;

				var props = PhotonNetwork.room.customProperties;

				if(props != null)
				{
					object obj = null;
					props.TryGetValue(PropsConstants.RoundTimeKey, out obj);

					if(obj != null)
					{
						int t = 0;
						int.TryParse(obj.ToString(), out t);

						if(t >= Config.Arenas.minRoundTime)
							roundTime = t;
					}
				}
					
				photonView.RPC("OnRemoteReloadArena", PhotonTargets.AllViaServer, newArena, roundTime);
			}
		}

		[PunRPC]
		private void OnRemoteReloadArena(string arenaId, int roomTime)
		{
			madnessMode.RestoreAllMadnessSteps();

			if(bonusController != null)
				bonusController.ResetAllBonuses();
			
			API.Score.Instance.LogRelativeRounds(1);

			OnArenaChange(arenaId, roomTime);

			/*foreach(var kvp in PhotonNetwork.room.customProperties)
				Debug.Log("debug room property " + kvp.Key + " / " + kvp.Value);*/
			
			if(menuRenderer != null)
			{
				menuRenderer.SetLoadingHintText(Config.hints.GetRandom());
				menuRenderer.GoToLoading(() =>
				{
					LevelLoader.Instance.LoadArena(arenaId, 1f);
				});
			}
		}

		public void OnArenaLoaded()
		{
			if(!HUD.IsNull)
			{
				Debug.Log("OnArenaLoaded - Hud not null, WTF?");
				return;
			}

			HUD.LoadScene(() => 
			{
				OnPhotonCustomRoomPropertiesChanged(PhotonNetwork.room.customProperties);

				UpdateArenaLevelEndTime();

				foreach(var player in PhotonNetwork.playerList)
					if(player != null && player != PhotonNetwork.player && player.customProperties != null)
						OnPhotonPlayerPropertiesChanged(new object[] { player, player.customProperties });

				RobotEmilNetworked.LoadLocalClientPlayerRobot(PhotonNetwork.player, playerInitialSpawn);

				GMReloaded.UI.Final.CreateGame.KBGameRoom gameRoom = menuRenderer == null ? null : menuRenderer.gameRoom;

				if(gameRoom != null)
				{
					Debug.Log("Trying to spawn bots - botCount: " + gameRoom.botCount);

					if(gameRoom.botCount > 0 && PhotonNetwork.isMasterClient)
					{
						botController.SpawnBots(PhotonNetwork.player, gameRoom.botCount);
					}
				}

				tutorial.HandleEvent(TutorialEvent.OnGameStarted);

				hud.OnLocalPlayerJoined(PhotonNetwork.player);
				hud.Show();
			});
		}

		public void OnArenaPlayerReady()
		{
			if(hud != null)
				hud.Show();

			UI.Final.Scene.UISceneLoader.DestroyScene();

			if(menuRenderer != null)
				menuRenderer.SetState(null, KBMenuRenderer.State.InGame);
		}

		public void OnStartEndOfRoundMaster()
		{
			photonView.RPC("OnStartEndOfRound", PhotonTargets.All);
		}

		public void OnShowScoreTableMaster()
		{
			photonView.RPC("OnShowScoreTable", PhotonTargets.All);
		}

		public void OnStartArenaReloadMaster()
		{
			photonView.RPC("OnStartArenaReload", PhotonTargets.All);
			ReloadArena();
		}

		[PunRPC]
		private void OnStartEndOfRound()
		{
			LocalClientRobotEmil.Instance.OnStartEndOfRound(playerController.numAlive, playerController.bestBoxDestroyer);
		}

		[PunRPC]
		private void OnShowScoreTable()
		{
			if(playerStatsTable != null)
				playerStatsTable.SetPermanentlyVisible(true);
		}

		[PunRPC]
		private void OnStartArenaReload()
		{
			if(playerStatsTable != null)
				playerStatsTable.SetPermanentlyVisible(false);
		}

		#endregion

		#region Remote Synced Projectiles

		private Dictionary<int, IProjectileObjectWithExplosion> projectiles = new Dictionary<int, IProjectileObjectWithExplosion>();

		public void RegisterSyncedProjectile(IProjectileObjectWithExplosion projectile)
		{
			if(projectile == null)
				return;

			int projHashId = projectile.GetAssignedHashId();

			if(projHashId <= 0)
			{
				Debug.LogWarning("Unable register synced projectile with id <= 0" + projectile);
				return;
			}

			Debug.Log("RegisterSyncedProjectile " + projHashId + " / " + projectile);

			projectiles[projHashId] = projectile;
		}

		public void ExplodeSyncedProjectile(IProjectileObjectWithExplosion projectile)
		{
			int projHashId = projectile.GetAssignedHashId();

			Debug.Log("ExplodeSyncedProjectile" + projHashId);

			photonView.RPC("ExplodeSyncedProjectileRemote", PhotonTargets.Others, projHashId, StructSerializer.Serialize<IProjectileObjectWithExplosion>(projectile));

			UnregisterSyncedProjectile(projectile);
		}

		[PunRPC]
		private void ExplodeSyncedProjectileRemote(int projectileHashId, byte[] projectileProperties)
		{
			IProjectileObjectWithExplosion projectile = null;

			projectiles.TryGetValue(projectileHashId, out projectile);

			Debug.Log("Exploding remote projectile " + projectileHashId);

			if(projectile != null)
			{
				StructSerializer.Deserialize<IProjectileObjectWithExplosion>(projectile, projectileProperties);

				projectile.Explode();
			}

			UnregisterSyncedProjectile(projectile);
		}

		private void UnregisterSyncedProjectile(IProjectileObjectWithExplosion projectile)
		{
			if(projectile == null)
				return;

			int projHashId = projectile.GetAssignedHashId();

			if(projHashId <= 0)
			{
				Debug.LogWarning("Unable unregister synced projectile with id <= 0" + projectile);
				return;
			}

			projectiles.Remove(projHashId);
		}

		#endregion

		#region Madness Mode

		public void DispatchMadnessStepOnRemotes(Config.MadnessMode.MadnessStep step)
		{
			photonView.RPC("RemoteDispatchMadnessStep", PhotonTargets.OthersBuffered, step.Serialize(), PhotonNetwork.time);
		}

		[PunRPC]
		private void RemoteDispatchMadnessStep(byte [] stepBytes, double timestamp)
		{
			madnessMode.RemoteProcessMadnessStep(Config.MadnessMode.MadnessStep.Deserialize(stepBytes), timestamp);
		}

		#endregion

		#region Traps

		private void UpdateTrapsDispatch()
		{
			if(arenaConfig == null || trapConfigs == null)
				return;

			if(trapsUpdateTimer < trapsUpdateTime)
			{
				trapsUpdateTimer += Time.deltaTime;
			}

			if(trapsUpdateTimer >= trapsUpdateTime)
			{
				for(int i = 0; i < trapConfigs.Count; i++)
				{
					var trapConfig = trapConfigs[i];

					if(!trapConfig.valid)
						continue;

					if(UnityEngine.Random.Range(0f, 100f) < trapConfig.probability * 100f)
					{
						if(trapConfig.repeats < trapConfig.repeatsMaxCount) // TODO: durationBetween
						{
							bool dispatching = Traps.TrapDispatcher.Instance.Dispatch(trapConfig);

							if(dispatching)
							{
								photonView.RPC("OnTrapRemoteDispatch", PhotonTargets.Others, arenaId, (byte)trapConfig.trapType);
								trapConfig.repeats++;
							}
						}

						break;
					}
				}

				trapsUpdateTimer = 0;
			}
			
		}

		[PunRPC]
		private void OnTrapRemoteDispatch(string arenaId, byte trapType)
		{
			if(this.arenaId != arenaId)
			{
				Debug.LogWarning("received OnTrapRemoteDispatch with invalid arena Id " + arenaId + " / " + trapType);
				return;
			}

			if(trapConfigs == null)
			{
				Debug.LogError("trapConfigs not initialized before OnTrapRemoteDispatch call");
				return;
			}


			foreach(var trapConfig in trapConfigs)
			{
				if(trapConfig.valid && trapConfig.trapType == (TrapType)trapType)
				{
					Traps.TrapDispatcher.Instance.Dispatch(trapConfig);
					return;
				}
			}

			Debug.LogError("failed to dispatch remote trap -- not found in trapConfig " + trapType);
		}

		#endregion

		#region Photon Events

		public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
		{
			base.OnPhotonPlayerPropertiesChanged(playerAndUpdatedProps);

			PhotonPlayer p = playerAndUpdatedProps[0] as PhotonPlayer;

			var player = playerController.Get(p.ID);

			if(player != null)
				player.OnPlayerPropertiesChanged(playerAndUpdatedProps[1] as ExitGames.Client.Photon.Hashtable);
		}

		public override void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
		{
			base.OnPhotonCustomRoomPropertiesChanged(propertiesThatChanged);

			lock(propertiesThatChanged)
			{
				object[] keys = propertiesThatChanged.Keys.ToArray();

				for(int i = 0; i < keys.Length; i++)
				{
					var ko = keys[i];

					if(ko == null)
						continue;
					
					var key = ko.ToString();
					object value = null;

					propertiesThatChanged.TryGetValue(key, out value);

					if(value == null)
					{
						Debug.Log("Skipping null property " + key);
					}

					if(key.StartsWith(EntityContainer.prefix))
					{
						EntityController.Instance.HandleNetworkData(value);
					}
					else
					{
						switch(key)
						{
							case PropsConstants.LevelKey:
								SetArenaId(value);
							break;

							case PropsConstants.MadnessKey:
								madnessMode.SetMadnessMode(value);
							break;

							case Madness.MadnessModeController.networkPropsId:
								madnessMode.HandleMadnessModeNetworkData(value);
							break;
								
							case PropsConstants.ServerKey:
								gameStateController.SetServerName(value, false);
							break;
								
							case PropsConstants.LevelEndTimeKey:
								gameStateController.SetLevelEndTime(value);
							break;
						}
					}
				}
			}
		}

		public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{

		}

		protected virtual void ForceLeaveRoom()
		{
			if(bonusController != null)
				bonusController.ResetAllBonuses();

			Tutorial.TutorialManager.Destroy();
			
			LevelLoader.Instance.LoadServerListMenu();
		}


		public override void OnMasterClientSwitched(PhotonPlayer player)
		{
			Debug.Log("OnMasterClientSwitched: " + player);
		}

		public override void OnLeftRoom()
		{
			Debug.Log("OnLeftRoom isInGame " + stateController.isInGame);

			if(stateController.isInGame)
				ForceLeaveRoom();
		}

		public override void OnDisconnectedFromPhoton()
		{
			Debug.Log("OnDisconnectedFromPhoton");
			ForceLeaveRoom();
		}

		public override void OnPhotonInstantiate(PhotonMessageInfo info)
		{
			Debug.Log("OnPhotonInstantiate " + info.sender);    // you could use this info to store this or react
		}

		public override void OnPhotonPlayerConnected(PhotonPlayer player)
		{
			hud.OnPlayerConnectedAED(player);

			Debug.Log("OnPhotonPlayerConnected: " + player);

			if(PhotonNetwork.isMasterClient && botController != null)
				botController.OnPhotonPlayerConnected(player);
		}

		public override void OnPhotonPlayerDisconnected(PhotonPlayer player)
		{
			hud.OnPlayerDisconnectedAED(player);

			Debug.Log("OnPlayerDisconneced: " + player);

			if(botController != null)
				botController.OnPhotonPlayerDisconnected(player);

			if(player.isLocal)
			{
				Debug.LogError("OnPhotonPlayerDisconnected called on local client - bug?");
				ForceLeaveRoom();
			}
		}

		public override void OnFailedToConnectToPhoton(DisconnectCause cause)
		{
			Debug.Log("OnFailedToConnectToPhoton " + cause);
			ForceLeaveRoom();
		}

		#endregion

		protected virtual void OnDestroy()
		{
			_instance = null;
		}

		#region Arena Bounds

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("TouchOrchestra/Grenade Madness/Calculate Arena AABB")]
		private static void GenerateDestroyableBoxes()
		{
			ArenaEventDispatcher aed = FindObjectOfType<ArenaEventDispatcher>();

			if(aed == null)
			{
				Debug.LogWarning("ArenaEventDispatcher not found in scene");
				return;
			}

			aed.CalculateSelfBounds();
		}
		#endif

		public void CalculateSelfBounds()
		{
			if(arenaModel == null)
			{
				Debug.LogWarning("arenaModel not set");
				return;
			}

			var renderers = arenaModel.GetComponentsInChildren<MeshRenderer>();

			Bounds bounds = new Bounds();

			foreach(var r in renderers)
			{
				if(r != null)
					bounds.Encapsulate(r.bounds);
			}

			arenaBounds = bounds;
		}

		public bool IsRobotInBounds(RobotEmilNetworked robot)
		{
			return robot != null && arenaBounds.Contains(robot.position);
		}
		
		#endregion
	}
	
}
