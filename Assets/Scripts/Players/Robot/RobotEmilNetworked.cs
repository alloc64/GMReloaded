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
using System;

using GMReloaded.Entities;
using GMReloaded.Bonuses;

namespace GMReloaded
{
	public class RobotEmilNetworked : RobotEmil, IInterpolationCallback<RobotEmilInterpolatorState>
	{
		#region implemented abstract members of RobotEmil

		public override string nick
		{
			get { return botClient == null ? (photonPlayer == null ? "Unknown" : photonPlayer.name) : botClient.nick; }
		}

		#endregion

		#region Network serialization / deserialization + interpolation

		public struct NetworkProperties : ISerializableStruct
		{
			public Vector3 position;
			public float angleY;
			public State state;
			public float speedMultiplier;
			public Vector3 controlDirection;
			public RobotEmilViewObserver.Direction directionState;
			public bool running;

			public WeaponType currWeaponType;

			public short score;
			public short boxes;
			public sbyte kills;
			public sbyte deaths;
			public short ping;

			public byte numBonusGrenades;

			#region ISerializableStruct implementation

			public void OnSerializeStruct(System.IO.BinaryWriter bw)
			{
				bw.Write(position);
				bw.Write(angleY);
				bw.Write((byte)state);
				bw.Write(speedMultiplier);
				bw.Write(controlDirection);
				bw.Write((byte)directionState);
				bw.Write(running);

				bw.Write((byte)currWeaponType);

				bw.Write(score);
				bw.Write(boxes);
				bw.Write(kills);
				bw.Write(deaths);
				bw.Write(ping);

				bw.Write(numBonusGrenades);
			}

			public bool OnDeserializeStruct(System.IO.BinaryReader br)
			{
				position = br.ReadVector3();
				angleY = br.ReadSingle();
				state = (State)br.ReadByte();
				speedMultiplier = br.ReadSingle();
				controlDirection = br.ReadVector3();
				directionState = (RobotEmilViewObserver.Direction)br.ReadByte();
				running = br.ReadBoolean();

				currWeaponType = (WeaponType)br.ReadByte();

				score = br.ReadInt16();
				boxes = br.ReadInt16();
				kills = br.ReadSByte();
				deaths = br.ReadSByte();
				ping = br.ReadInt16();

				numBonusGrenades = br.ReadByte();

				return true;
			}

			#endregion
		}

		public byte[] networkProperties
		{
			get
			{
				var np = new NetworkProperties();

				np.position = position;
				np.angleY = angleY;
				np.state = state;
				np.speedMultiplier = speedMultiplier;
				np.controlDirection = controlDirection;
				np.running = running;
				np.currWeaponType = currWeaponType;
				np.directionState = directionState;

				np.score  = Convert.ToInt16(Mathf.Clamp(score, short.MinValue, short.MaxValue));
				np.boxes  = Convert.ToInt16(Mathf.Clamp(boxes, short.MinValue, short.MaxValue));
				np.kills  = Convert.ToSByte(Mathf.Clamp(kills, sbyte.MinValue, sbyte.MaxValue));
				np.deaths = Convert.ToSByte(Mathf.Clamp(deaths, sbyte.MinValue, sbyte.MaxValue));
				np.ping   = Convert.ToInt16(Mathf.Clamp(PhotonNetwork.GetPing(), 0, short.MaxValue));

				np.numBonusGrenades = Convert.ToByte(Mathf.Clamp(numBonusGrenades, Byte.MinValue, Byte.MaxValue));

				return StructSerializer.Serialize<NetworkProperties>(np);
			}
		}

		/*private void DebugState(RobotEmilInterpolatorState lhs, RobotEmilInterpolatorState rhs)
		{
			Debug.Log(" State: " + lhs.position + " - " + lhs.position);

			for(int i = 0; i < lhs.activeGrenades.Length; i++)
			{
				var a0 = lhs.activeGrenades[i];
				var a1 = rhs.activeGrenades[i];

				if(a0.state == GrenadeBase.State.Uninitialized)
					continue;


				Debug.Log(a0.state + " - " + a0.position + " - " + a1.position + " - " + a1.state);
			}
		}*/

		public void OnUpdateInterpolated(RobotEmilInterpolatorState lhs, RobotEmilInterpolatorState rhs, float dt)
		{
			position = Vector3.Lerp(lhs.position, rhs.position, dt);
			angleY = Mathf.LerpAngle(lhs.angleY, rhs.angleY, dt);
			controlDirection = Vector3.Lerp(lhs.controlDirection, rhs.controlDirection, dt);
			directionState = lhs.directionState;
			running = lhs.running;

			//DebugState(lhs, rhs);

			numBonusGrenades = lhs.numBonusGrenades;
			//grenadesNetworkProcessor.OnUpdateInterpolated(lhs.activeGrenades, rhs.activeGrenades, dt);

			SetState(lhs.state, true);

			SetWeapon(lhs.currWeaponType);
			SetSpeedMultiplier(lhs.speedMultiplier);
		}

		public void OnUpdateLerped(RobotEmilInterpolatorState latest, float dt)
		{
			position = Vector3.Lerp(position, latest.position, dt);
			angleY = Mathf.LerpAngle(angleY, latest.angleY, dt);
			controlDirection = Vector3.Lerp(controlDirection, latest.controlDirection, dt);
			directionState = latest.directionState;
			running = latest.running;

			numBonusGrenades = latest.numBonusGrenades;
			//grenadesNetworkProcessor.OnUpdateLerped(latest.activeGrenades, dt);

			SetState(latest.state, true);

			SetWeapon(latest.currWeaponType);
			SetSpeedMultiplier(latest.speedMultiplier);
		}

		public void OnNetworkPropertiesReceived(NetworkProperties np)
		{
			this.score = np.score;
			this.boxes = np.boxes;
			this.kills = np.kills;
			this.deaths = np.deaths;
			this.ping = np.ping;

			if(playerStatsTable != null)
				playerStatsTable.UpdatePlayer(this);
		}

		/*
		private RobotEmilNetworkGrenadesProcessor _grenadesNetworkProcessor;
		private RobotEmilNetworkGrenadesProcessor grenadesNetworkProcessor
		{
			get
			{
				if(_grenadesNetworkProcessor == null)
					_grenadesNetworkProcessor = new RobotEmilNetworkGrenadesProcessor(this);

				return _grenadesNetworkProcessor;
			}
		}
		*/

		#endregion

		private int _score = 0;
		public int score
		{
			get { return _score; }

			protected set
			{ 
				if(localClient != null)
					localClient.OnLocalScoreChanged(value - _score);
				
				_score = value;
			}
		}

		private int _boxes = 0;
		public int boxes 
		{ 
			get { return _boxes; }
			protected set
			{ 
				_boxes = value;
			}
		}

		private int _kills = 0;
		public int kills 
		{ 
			get { return _kills; }
			protected set
			{ 
				if(localClient != null)
					localClient.OnLocalKillsChanged(value - _kills);
				
				_kills = value;
			}
		}

		private int _deaths = 0;
		public int deaths 
		{ 
			get { return _deaths; } 
			protected set
			{ 
				if(localClient != null)
					localClient.OnLocalDeathsChanged(value - _deaths);

				_deaths = value;
			}
		}

		private int _ping = 0;
		public int ping 
		{ 
			get
			{ 
				return clientType != ClientType.RemoteClient ? PhotonNetwork.GetPing() : _ping;
			}

			protected set
			{ 
				_ping = value;
			}
		}

		private int pickedUpBonusCount = 0;

		[SerializeField]
		protected PhotonView photonView;

		private LocalClientRobotEmil localClient;

		private RobotEmilDeathCamEffect deathEffect;

		private PhotonPlayer _photonPlayer;
		public PhotonPlayer photonPlayer
		{
			get
			{
				return botClient != null ? botClient.photonPlayer : _photonPlayer;
			}

			set
			{ 
				//boti maji vlastni photonPlayer

				_photonPlayer = value;
			}
		}

		// toto pouzivaji jenom boti
		public PhotonPlayer botOwner { get; private set; }

		public RobotEmilRemoteClientPhotonObserver remoteClientObserver;

		public int photonPlayerId
		{
			get
			{
				if(photonPlayer == null)
					return -1;

				return photonPlayer.ID;
			}
		}

		public bool isMasterClient { get { return photonPlayer != null && photonPlayer.isMasterClient; } }


		//

		public override Vector3 positionRaw 
		{ 
			get { return position; } 
			set 
			{ 
				if(remoteClientObserver != null)
					remoteClientObserver.interpolator.ForcePosition(value);
				
				position = value; 
			} 
		}

		private HUDPasiveBonusStackIcon boxBonusPassieStackIndicator;

		/*
		public override int freeBoxes
		{
			get { return base.freeBoxes; }

			set
			{
				if(value < 0)
					value = 0;

				base.freeBoxes = value;

				if(boxBonusPassieStackIndicator != null)
					boxBonusPassieStackIndicator.value = value;
			}
		}
		*/

		#region Singleton getters

		private GameInfoStack gameInfoStack { get { return HUD.IsNull ? null : HUD.Instance.gameInfoStack; } }

		protected WeaponIndicator weaponIndicator { get { return hud == null || hud.weaponIndicator == null ? null : hud.weaponIndicator; } }

		private PlayerStatsTable playerStatsTable { get { return HUD.IsNull ? null : HUD.Instance.playerStats; } }

		private IExplosiveObjectsController eoc { get { return IExplosiveObjectsController.Instance; } }

		#endregion

		#region Bonus Implementations

		private TeleportBonusImpl teleportBonusImpl = new TeleportBonusImpl();

		#endregion

		#region Dabing

		private KillCounter killCounter;

		#endregion


		#region Skin

		private Color _color = Color.white;
		public override Color color
		{
			get
			{ 
				switch(clientType)
				{
					default:
					case ClientType.BotClient:
					case ClientType.RemoteClient:
						return _color;
						
					case ClientType.LocalClient:
					return LocalClientRobotEmil.user.color;
				}
			}

			protected set { _color = value; }
		}

		#endregion

		#region Self Loader


		public virtual void PairPhotonPlayer(PhotonPlayer player)
		{
			if(player == null)
			{
				Debug.LogError("PairPhotonPlayer failed");
				return;
			}

			this.photonPlayer = player;

			photonView.RPC("OnRemorePairPhotonPlayer", PhotonTargets.OthersBuffered, player.ID, color.ToByteArray(), (byte)skin);
		}


		[PunRPC]
		protected virtual void OnRemorePairPhotonPlayer(int playerId, byte[] color, byte skin)
		{
			PhotonPlayer remotePhotonPlayer = PhotonPlayer.Find(playerId);

			if(remotePhotonPlayer == null)
				Debug.LogError("Failed to pair remote photon player " + playerId);

			this.photonPlayer = remotePhotonPlayer;

			this.SetPlayerColor(color.ColorFromByteArray());
			this.SetSkin((Skin)skin);

			Debug.Log("OnRemorePairPhotonPlayer " + playerId + " - " + nick + " - " + color.ColorFromByteArray() + " - " + skin);
		}

		// Boti

		public void PairBotPhotonPlayer(PhotonPlayer player, PhotonPlayer botOwner)
		{
			if(player == null)
			{
				Debug.LogError("PairBotPhotonPlayer failed");
				return;
			}

			this.photonPlayer = player;

			photonView.RPC("OnRemorePairBotPhotonPlayer", PhotonTargets.OthersBuffered, player.ID, botOwner.ID, nick, color.ToByteArray(), (byte)skin);
		}

		private bool onRemoteBotConnectedCall = false;
		private bool onRemoteBotConnectedCalled = false;
		[PunRPC]
		protected virtual void OnRemorePairBotPhotonPlayer(int playerId, int botOwnerId, string nick, byte[] color, byte skin)
		{
			PhotonPlayer remotePhotonPlayer = new PhotonPlayer(false, playerId, nick);

			if(remotePhotonPlayer == null)
				Debug.LogError("Failed to pair remote bot photon player " + playerId);

			Debug.Log("OnRemorePairBotPhotonPlayer " + playerId + " - " + nick + " - " + color.ColorFromByteArray() + " - " + skin);

			this.photonPlayer = remotePhotonPlayer;
			this.botOwner = PhotonPlayer.Find(botOwnerId);
			this.SetPlayerColor(color.ColorFromByteArray());
			this.SetSkin((Skin)skin);

			if(clientType == ClientType.RemoteClient)
			{
				onRemoteBotConnectedCall = true;
			}
		}

		private void UpdateOnRemoteBotClientConnected()
		{
			if(onRemoteBotConnectedCall && !onRemoteBotConnectedCalled && arenaEventDispatcher != null)
			{
				arenaEventDispatcher.OnRemoteBotConnected(photonPlayer);
				onRemoteBotConnectedCall = false;
				onRemoteBotConnectedCalled = true;
			}
		}

		public static RobotEmilNetworked LoadPlayerRobot(PlayerInitialSpawn playerInitialSpawn)
		{
			var robot = PhotonNetworkExtensions.LoadPrefab<RobotEmilNetworked>("Prefabs/Players/RobotEmil", playerInitialSpawn.position, Quaternion.identity);

			if(robot != null)
			{
				robot.OnInstantiated();
			}

			return robot;
		}

		public static void LoadLocalClientPlayerRobot(PhotonPlayer player, PlayerInitialSpawn playerInitialSpawn)
		{
			if(playerInitialSpawn == null)
			{
				Debug.LogError("Arena playerInitialSpawn not assigned!");
				return;
			}

			RobotEmilNetworked robot = LoadPlayerRobot(playerInitialSpawn);

			if(robot != null)
			{
				robot.SetLocalClient(player);
			}
		}

		#endregion

		#region Unity

		protected override void Awake()
		{
			base.Awake();

			Assert.IsAssigned(photonView);
			SetupObservers(photonView.isMine);
		}

		#endregion

		#region Photon Networking setup

		protected override void OnInstantiated()
		{
			base.OnInstantiated();

			//STUB
		}

		protected override void OnLoaded()
		{
			playersController.Register(this);
			base.OnLoaded();
		}

		#region Observers

		private void SetupObservers(bool localPlayer)
		{
			if(localPlayer)
			{
				AddPhotonObservedComponent<RobotEmilLocalClientPhotonObserver>();
			}
			else
			{
				remoteClientObserver = AddPhotonObservedComponent<RobotEmilRemoteClientPhotonObserver>();
				SetRemoteClient();
			}
		}

		protected virtual T AddPhotonObservedComponent<T>() where T : IRobotEmilPhotonObserver
		{
			if(photonView == null)
				return null;

			T c = gameObject.AddComponent<T>();

			if(c == null)
				return null;

			c.SetParentRobot(this);

			photonView.observed = c;

			return c;
		}

		#endregion

		#endregion

		#region Network

		//TODO: test only klientsky granaty

		protected override void OnDrawGizmos()
		{
			base.OnDrawGizmos();

			//grenadesNetworkProcessor.OnDrawGizmos();
		}

		//private Timer testTimer = new Timer();
		private float scoreTableDataUpdateTimer = 0f;

		#if TRAILER_VERSION

		private bool robotVisible = true;

		#endif

		protected override void Update()
		{
			#if TRAILER_VERSION

			if(clientType == ClientType.LocalClient)
			{
				if(Input.GetKeyUp(KeyCode.G))
				{
					robotVisible = !robotVisible;

					ragdoll.gameObject.SetActive(robotVisible);


					if(robotVisible)
					{
						ResetAnimatorLayerWeights();

						SetWeapon(currWeaponType, true);
					}
				}

				if(Input.GetKeyUp(KeyCode.J))
				{
					if(GMReloaded.LocalClientRobotEmil.level < 50)
					{
						GMReloaded.LocalClientRobotEmil.levelPoints = GMReloaded.Config.Exp.GetLevelDesiredPoints(50);
						GMReloaded.LocalClientRobotEmil.level = 50;
					}
				}
			}

			#endif

			base.Update();

			if(clientType != ClientType.RemoteClient)
			{
				if(scoreTableDataUpdateTimer < 0.4f)
				{
					scoreTableDataUpdateTimer += Time.deltaTime;
				}
				else
				{
					if(playerStatsTable != null)
						playerStatsTable.UpdatePlayer(this);
				
					scoreTableDataUpdateTimer = 0f;
				}

				OnUpdateWeaponProjectilesCount();

				if(state != State.Dead && state != State.Unspawned)
					CheckArenaBounds();
			}
			else
			{
				UpdateOnRemoteBotClientConnected();
			}

			/*
			testTimer.Delay(0.05f, () =>
			{
				var bytes = networkProperties;

				var np = StructSerializer.Deserialize<NetworkProperties>(bytes);

				grenadesNetworkProcessor.Process(np.activeGrenades, np.activeGrenades, 1f, true);
			});*/
		}

		public override void OnTriggerEnter(Collider c)
		{
			base.OnTriggerEnter(c);

			if(clientType != RobotEmil.ClientType.RemoteClient)
			{
				if(c.gameObject.layer == Layer.Bonus)
				{
					var bonusPickableBase = c.GetComponent<Entities.BonusPickable>();

					if(bonusPickableBase != null)
						OnTryPickUpBonus(bonusPickableBase.entityContainer);
				}
			}
		}
		#endregion

		#region Spawning

		protected override void Spawn()
		{
			base.Spawn();

			killsFromLastSpawn = 0;

			if(clientType == ClientType.LocalClient && hud != null)
				hud.OnLocalPlayerSpawned();
		}

		public virtual void SpawnOnFreeSpawn()
		{
			if(clientType != ClientType.RemoteClient)
			{
				photonView.RPC("AcquireFreeSpawnPosition", PhotonTargets.MasterClient, photonPlayerId);
				PhotonNetwork.SendOutgoingCommands();
			}
			else
			{
				//TODO: Toto musi prijit doprdele a musi se to zavolat RPCckem kdyz se hrac respawne
				Spawn();
			}
		}

		[PunRPC]
		private void AcquireFreeSpawnPosition(int photonPlayerId)
		{
			Debug.LogWarning("AcquireFreeSpawnPosition " + photonPlayerId);

			//TODO: tady by měl bejt delay, pokud se hrac nepripoji do 60s tak ten spawn vratime jako not used
			SpawnPoint spawn = null;

			if(spawnManager != null)
				spawn = spawnManager.GetFreeSpawn();

			if(spawn == null)
			{
				Debug.LogWarning("Free spawn not found for player " + photonPlayerId);
				photonView.RPC("OnRemoteFreeSpawnNotAcquired", PhotonTargets.All, photonPlayerId);
			}
			else
			{
				photonView.RPC("OnRemoteSpawnPositionAcquired", PhotonTargets.All, photonPlayerId, spawn.position, spawn.localEulerAngles.y);
			}
		}

		[PunRPC]
		private void OnRemoteSpawnPositionAcquired(int photonPlayerId, Vector3 spawnPosition, float spawnPositionAngleY)
		{
			if(spawnManager != null)
			{
				spawnManager.ValidateSpawn(ref spawnPosition, ref spawnPositionAngleY);
			}

			if(botClient != null)
				botClient.OnRemoteSpawnPositionAcquired(photonPlayerId, spawnPosition, spawnPositionAngleY);

			Spawn(spawnPosition, spawnPositionAngleY);

			//Debug.Log("OnRemoteSpawnPositionAcquired()");
		}

		[PunRPC]
		private void OnRemoteFreeSpawnNotAcquired(int photonPlayerId)
		{
			if(this.photonPlayerId == photonPlayerId)
			{
				OnFreeSpawnNotFound();
			}
		}

		private void OnFreeSpawnNotFound()
		{
			Debug.LogError("OnFreeSpawnNotFound respawning in 15s");
			Timer.DelayAsyncIndependent(0.5f, () => Respawn(15));
		}

		public override void Spawn(Vector3 spawnPosition, float angleY)
		{
			if(deathEffect != null)
				deathEffect.Hide();
			
			base.Spawn(spawnPosition, angleY);
		}

		protected override void OnSpawned()
		{
			base.OnSpawned();
		}

		protected override void OnFirstSpawn()
		{
			base.OnFirstSpawn();

			if(localClient != null)
				localClient.OnFirstSpawn();
		}

		protected virtual void Respawn(float delay = 3f)
		{
			Debug.Log("Respawn " + delay);

			int lastTime = -1;

			RespawnIndicator respawnIndicator = null;

			if(clientType == ClientType.LocalClient)
				respawnIndicator = hud.respawnIndicator;

			if(respawnIndicator != null)
				respawnIndicator.Show();

			// acquirnu si spawn

			Timer.DelayAsync(delay, () =>
			{
				// respawnu ho

				SpawnOnFreeSpawn();

				if(respawnIndicator)
					respawnIndicator.Hide();
				
			}, (t) => 
			{
				if(clientType == ClientType.LocalClient)
				{
					int time = Mathf.RoundToInt(delay - t * delay);

					if(lastTime != time)
					{
						if(respawnIndicator != null)
							respawnIndicator.SetTime(time);
						
						lastTime = time;
					}
				}
			});
		}

		#endregion

		#region Player Color

		public void SetPlayerColor(Color color)
		{
			this.color = color;
		}

		#endregion

		#region Player Properties

		public void SetPlayerProperty(string key, object value)
		{
			SetCustomPlayerProperties(new ExitGames.Client.Photon.Hashtable() { { key, value } });
		}

		private void SetCustomPlayerProperties(ExitGames.Client.Photon.Hashtable properties)
		{
			if(photonPlayer == null)
			{
				Debug.Log("Failed to SetCustomPlayerProperties photonPlayer == null");
				return;
			}

			photonPlayer.SetCustomProperties(properties);
		}

		public void OnPlayerPropertiesChanged(ExitGames.Client.Photon.Hashtable hashtable)
		{
			/*
			foreach(var kvp in hashtable)
			{
				switch(kvp.Key.ToString())
				{
					case playerPropertyColor:
					break;
				}
			}
			*/
		}

		#endregion

		#region Client settings

		public void SetRemoteClient()
		{
			clientType = ClientType.RemoteClient;
		}

		public void SetLocalClient(PhotonPlayer player)
		{
			clientType = ClientType.LocalClient;

			if(hud != null)
			{
				hud.hitIndicator.SetRobotParent(this);
			}

			localClient = gameObject.AddComponent<LocalClientRobotEmil>();

			SetSkin(LocalClientRobotEmil.user.skin);
			PairPhotonPlayer(player);

			localClient.Spawn(this);

			SetListener();
			SpawnOnFreeSpawn();

			killCounter = new KillCounter();

			deathEffect = gameObject.AddComponent<RobotEmilDeathCamEffect>();
		}

		public AI.Bots.RobotEmilBotClient SetBotClient(AI.Bots.BotController botController, PhotonPlayer botOwner, int photonBotId, int botId)
		{
			botClient = gameObject.AddComponent<AI.Bots.RobotEmilBotClient>();
			botClient.SetBotParent(this, botOwner, botController, photonBotId, botId);

			clientType = ClientType.BotClient;

			botClient.FirstSpawn();

			return botClient;
		}

		#endregion

		#region Robot Damage + Hit management

		public override void HitByMeleeWeapon(IAttackerObject attacker, float percentualDamage, float damage)
		{
			base.HitByMeleeWeapon(attacker, percentualDamage, damage);

			// dostal jsem zasah 

			var attackerRobot = attacker.ParentRobot as RobotEmilNetworked;

			if(attacker is IRagdollDirectionalInfluencer)
			{
				IRagdollDirectionalInfluencer dirInfluencer = attacker as IRagdollDirectionalInfluencer;

				var attackerWeaponType = (byte)attacker.WeaponType;

				photonView.RPC
				(
					"OnRemotePlayerHitByMeleeWeapon", PhotonTargets.All, 
					attackerWeaponType, 
					attackerRobot.photonPlayerId, 
					photonPlayerId, 

					attacker.position,
					dirInfluencer.hitForce,

					percentualDamage, 
					damage
				);

				PhotonNetwork.SendOutgoingCommands();
			}
			else
			{
				Debug.LogWarning(attacker + " not implementing HitRemotePlayerByMeleeWeapon");
			}
		}

		[PunRPC]
		private void OnRemotePlayerHitByMeleeWeapon(byte wt, int attackerPhotonPlayerId, int victimPhotonPlayerId, Vector3 hitPosition, Vector3 hitForce, float percentualDamage, float damage)
		{
			if(this != null && clientType != ClientType.RemoteClient)
			{
				var attackerWeaponType = (WeaponType)wt; 
				RobotEmilNetworked attackerRobot = null;

				if(playersController != null)
					attackerRobot = playersController.Get(attackerPhotonPlayerId);

				if(attackerRobot == null)
				{
					Debug.LogWarning("OnRemotePlayerHit - attackerRobot == null");
					return;
				}

				Debug.Log("OnRemotePlayerHitByMeleeWeapon " + attackerRobot.name + " Hit " + name);

				// Ja jsem dostal hit, pošlu notifikaci ostatnim

				if(!Hit(attackerRobot, attackerWeaponType, -1, percentualDamage, damage))
					return;
			
				OnHitEffect(hitPosition);

				photonView.RPC
				(
					"OnRemotePlayerHit", PhotonTargets.Others, 
					(byte)attackerWeaponType, 
					-1,

					attackerRobot.photonPlayerId, 
					photonPlayerId, 

					hitPosition,

					percentualDamage, 
					damage
				);

				PhotonNetwork.SendOutgoingCommands();
			}
			else
			{
				Debug.Log("OnRemotePlayerHitByMeleeWeapon received on remote client");
			}
		}

		protected override void OnLocalPlayerHit(IAttackerObject attacker, float percentualDamage, float damage)
		{
			base.OnLocalPlayerHit(attacker, percentualDamage, damage);

			var attackerRobot = attacker.ParentRobot as RobotEmilNetworked;
			var attackerWeaponType = attacker.WeaponType;
			var attackerWeaponProjectileType = attacker.ProjectileType;

			Debug.Log("OnLocalPlayerHit " + attacker + " / " + attackerWeaponType + " / " + attackerWeaponProjectileType);


			FlashGrenade flashGrenade = attacker as FlashGrenade;

			// pokud jsem si dal sam pecku tak si vemu HP a pošlu to ostatnim

			if(flashGrenade != null)
			{
				OnFlashGrenadeHit(flashGrenade, percentualDamage, damage);
				return;
			}
			else
			{
				if(!Hit(attackerRobot, attackerWeaponType, attackerWeaponProjectileType, percentualDamage, damage))
					return;
			}

			OnHitEffect(attacker.position);

			if(ragdoll != null)
				ragdoll.HitByObject(attacker);

			// poslu zpravu ostatnim

			if(attacker is IRagdollExplosiveInfluencer)
			{
				IRagdollExplosiveInfluencer explosiveInfluencer = attacker as IRagdollExplosiveInfluencer;

				photonView.RPC
				(
					"OnRemotePlayerHitByExplosiveInfluencer", PhotonTargets.Others, 
					(byte)attackerWeaponType, 
					attackerWeaponProjectileType,

					(attackerRobot == null ? -1 : attackerRobot.photonPlayerId), 
					photonPlayerId, 

					attacker.position,
					explosiveInfluencer.explosionForce,
					explosiveInfluencer.explosionForceRadius,

					percentualDamage, 
					damage
				);
			}
			else
			{
				photonView.RPC
				(
					"OnRemotePlayerHit", PhotonTargets.Others, 
					(byte)attackerWeaponType, 
					attackerWeaponProjectileType,

					(attackerRobot == null ? -1 : attackerRobot.photonPlayerId), 
					photonPlayerId, 

					attacker.position,

					percentualDamage, 
					damage
				);
			}

			PhotonNetwork.SendOutgoingCommands();
		}

		[PunRPC]
		private void OnRemotePlayerHitByExplosiveInfluencer(byte weaponType, int attackerWeaponProjectileType, int attackerPhotonPlayerId, int victimPhotonPlayerId, Vector3 hitPosition, float explosionForce, float explosionForceRadius, float percentualDamage, float damage)
		{
			OnRemotePlayerHit(weaponType, attackerWeaponProjectileType, attackerPhotonPlayerId, victimPhotonPlayerId, hitPosition, percentualDamage, damage);

			if(ragdoll != null)
				ragdoll.HitByIRagdollExplosiveInfluencer(explosionForce, hitPosition, explosionForceRadius);
		}

		[PunRPC]
		private void OnRemotePlayerHitByRagdollDirectionalInfluencer(byte weaponType, int attackerWeaponProjectileType, int attackerPhotonPlayerId, int victimPhotonPlayerId, Vector3 hitPosition, Vector3 hitForce, float percentualDamage, float damage)
		{
			OnRemotePlayerHit(weaponType, attackerWeaponProjectileType, attackerPhotonPlayerId, victimPhotonPlayerId, hitPosition, percentualDamage, damage);

			if(ragdoll != null)
				ragdoll.HitByIRagdollDirectionalInfluencer(hitForce);
		}

		[PunRPC]
		private void OnRemotePlayerHit(byte weaponType, int attackerWeaponProjectileType, int attackerPhotonPlayerId, int victimPhotonPlayerId, Vector3 hitPosition, float percentualDamage, float damage)
		{
			var attackerWeaponType = (WeaponType)weaponType; 
			var attackerRobot = attackerPhotonPlayerId == -1 ? null : playersController.Get(attackerPhotonPlayerId);
			var victimRobot = playersController.Get(victimPhotonPlayerId);

			if(victimRobot == null)
			{
				Debug.LogWarning("OnRemotePlayerHit - victimRobot == null");
				return;
			}

			victimRobot.Hit(attackerRobot, attackerWeaponType, attackerWeaponProjectileType, percentualDamage, damage);
		}

		private void OnFlashGrenadeHit(FlashGrenade grenade, float percentualDamage, float damage)
		{
			if(clientType != ClientType.RemoteClient)
			{
				float visibility = percentualDamage;

				bool isInFrustum = grenade.IsVisible(viewObserver.camera);

				if(!isInFrustum)
					visibility *= 0.5f;
				else
				{
					RaycastHit hit;
					if(Physics.Linecast(position, grenade.position, out hit, ((1 << Layer.DestroyableEntity) | (1 << Layer.Default))))
					{
						visibility = 0f;
					}
				}

				OnFlashPlayer(1f - visibility);
			}
		}

		private void OnFlashPlayer(float visibility)
		{
			if(clientType == ClientType.LocalClient)
			{
				if(hud != null && hud.flashbang != null)
					hud.flashbang.GrenadeFlash(visibility, this);
			}
			else // BOT, jinak se to nevola
			{
				if(botClient != null)
					botClient.OnFlashPlayer(visibility);
			}
		}

		protected void OnHitEffect(Vector3 hitPosition)
		{
			if(clientType == ClientType.LocalClient && normalizedLives < 0.99f && state != State.Dead)
			{
				viewObserver.Shake();

				if(hud != null)
				{
					hud.hitIndicator.SetHitPosition(hitPosition);
					hud.OnPlayerHPChanged(normalizedLives, -1);
				}
			}
		}

		#endregion

		protected override void ReloadWeapon(WeaponType weaponType)
		{
			base.ReloadWeapon(weaponType);

			if(clientType == ClientType.LocalClient && weaponIndicator != null)
			{
				weaponIndicator.SetWeapon(weaponType);
			}
		}

		public override int Attack(AttackType attackType)
		{
			int hashId = base.Attack(attackType);

			//Debug.Log("Sync ID " + hashId);

			// -1 chyba
			// 0 - projektil kterej se nesyncuje
			// > 0 syncnutej projektil

			if(hashId >= 0)
			{
				photonView.RPC("OnRemoteAttack", PhotonTargets.Others, attackType, PhotonNetwork.time, hashId);
				PhotonNetwork.SendOutgoingCommands();
			}

			return hashId;
		}

		public override void OnUpdateWeaponDepeletedProgress(float p)
		{
			base.OnUpdateWeaponDepeletedProgress(p);

			if(clientType == ClientType.LocalClient && hud != null)
			{
				var crosshair = hud.crosshair;

				if(crosshair != null)
					crosshair.SetDepeletedProgress(p);
			}
		}

		// updatovano jenom na lokal klientovi
		public virtual void OnUpdateWeaponProjectilesCount()
		{
			if(currWeapon != null && clientType == ClientType.LocalClient)
			{
				var c = currWeapon.weaponConfig;

				if(c != null && c.use != Config.Weapons.WeaponConfig.Use.Forever && hud != null)
				{
					int max = Mathf.Clamp(c.initProjectileCount + numBonusGrenades, 0, c.maxProjectileCount);

					//TODO: pokud ma secondary attack tak posefisovat

					hud.weaponIndicator.SetWeaponProjectilesCount(AttackType.Primary, max - c.usedCount, max);
				}
			}
		}

		#region RPCs

		[PunRPC]
		public void OnRemoteAttack(byte attackType, double timestamp, int projectileHashId)
		{
			OnStartAttack((RobotEmil.AttackType)attackType, timestamp, projectileHashId);
		}

		#endregion

		#region Bonuses

		public void OnTryPickUpBonus(Entities.EntityContainer entityContainer)
		{
			if(entityContainer == null)
				return;

			var bonus = entityContainer.bonusImpl.bonus;

			if(bonus == null)
				return;

			photonView.RPC("OnTryPickUpBonusMaster", PhotonTargets.MasterClient, entityContainer.entityId, (int)bonus.behaviour);
			PhotonNetwork.SendOutgoingCommands();
		}

		[PunRPC]
		private void OnTryPickUpBonusMaster(short entityId, int bonusBehaviourInt)
		{
			Bonus.Behaviour bonusBehaviour = (Bonus.Behaviour)bonusBehaviourInt;

			var entityContainer = EntityController.Instance.GetEntityContainer(entityId);

			if(entityContainer != null && entityContainer.bonusImpl.canBonusBePickedUp)
			{
				photonView.RPC("OnBonusPickUpAllowed", PhotonTargets.All, entityId);
			}
			else
			{
				photonView.RPC("OnBonusPickUpRefused", PhotonTargets.All, entityId);

				Debug.LogWarning("OnTryPickUpBonus bonus boxId" + entityId + " not pickable / " + bonusBehaviour);
			}

			PhotonNetwork.SendOutgoingCommands();
		}

		[PunRPC]
		private void OnBonusPickUpAllowed(short entityId)
		{
			var entityContainer = EntityController.Instance.GetEntityContainer(entityId);

			if(entityContainer == null)
			{
				Debug.LogWarning("Error, unable find entityContainer in OnBonusPickUpAllowed / " + entityId);
				return;
			}

			OnBonusPickUpLocal(entityContainer);
		}

		[PunRPC]
		private void OnBonusPickUpRefused(short entityId)
		{
			var entityContainer = EntityController.Instance.GetEntityContainer(entityId);

			if(entityContainer == null)
			{
				Debug.LogWarning("Error, unable find boxDestroyable in OnBonusPickUpRefused / " + entityId);
				return;
			}

			OnBonusPickUpLocal(entityContainer, true);
		}

		private void OnBonusPickUpLocal(EntityContainer entityContainer, bool refusedByMaster = false)
		{
			if(entityContainer == null)
				return;

			var bonusImpl = entityContainer.bonusImpl;
			var bonus = bonusImpl == null ? null : bonusImpl.bonus;

			bool pickedUp = (bonus != null && PickUpBonus(bonus, clientType != RobotEmil.ClientType.RemoteClient));

			//TODO: refusedByMaster se neověřuje

			var pickupType = BonusPickable.State.NormalPickUp;//refusedByMaster ? BonusPickable.PickupType.Silent : BonusPickable.PickupType.Normal;

			if(!pickedUp)
			{
				pickupType = BonusPickable.State.Denied;

				if(clientType == ClientType.LocalClient && bonus != null)
				{
					switch(bonus.behaviour)
					{
						case Bonus.Behaviour.HPPack:
						break;
							
						default:
							gameMessage.SetMessage(localization.GetValue("Bonus_StackFull", bonus.name), GameMessage.MsgType.BottomSmallNotice);
						break;
					}
				}
			}
			else
			{
				if(clientType == ClientType.LocalClient && activeBonusStack.isFull)
				{
					tutorial.HandleEvent(TutorialEvent.OnActiveBonusStackFull);
				}
			}

			entityContainer.PickUpBonus(pickupType);
		}

		//

		protected override void OnBonusPickedUp(Bonus bonus)
		{
			base.OnBonusPickedUp(bonus);

			if(clientType == ClientType.LocalClient)
			{
				missions.stats.SetBonusCollected(bonus.behaviour);

				upgradeTree.IncrementItem(Config.UpgradeTree.ItemIDs.Item_7, 1);

				tutorial.HandleEvent(TutorialEvent.OnBonusPickedUp, bonus.behaviour);

				//

				pickedUpBonusCount++;

				if(pickedUpBonusCount >= 100)
				{
					missions.IncrementMission(Config.Missions.MissionIDs.Mission_86, 1);
				}

				gameMessage.SetBonusPickedUp(bonus);
			}
				
		}

		public override bool UseActiveBonus(int idx, bool usedRemote, double timestamp = -1f)
		{
			bool ret = base.UseActiveBonus(idx, usedRemote, timestamp);

			photonView.RPC("RemoteUseActiveBonus", PhotonTargets.Others, idx, PhotonNetwork.time);
			PhotonNetwork.SendOutgoingCommands();

			return ret;
		}

		public override void OnBonusUsageStarted(Bonus bonus)
		{
			base.OnBonusUsageStarted(bonus);

			if(!bonus.usedRemote)
			{
				photonView.RPC("RemoteUseBonus", PhotonTargets.Others, (int)bonus.behaviour);
				PhotonNetwork.SendOutgoingCommands();
			}

			if(clientType == ClientType.LocalClient)
			{
				gameMessage.SetBonusUseStarted(bonus);

				tutorial.HandleEvent(TutorialEvent.OnBonusUsageStarted, bonus.behaviour);

				if(bonus.type == Bonus.Type.Active)
					missions.stats.SetActiveBonusUsed(bonus.behaviour);
			}
		}

		public override void OnBonusUsageComplete(Bonus bonus)
		{
			base.OnBonusUsageComplete(bonus);


			if(clientType == ClientType.LocalClient)
			{
				tutorial.HandleEvent(TutorialEvent.OnBonusUsageComplete, bonus.behaviour);
			}
		}

		[PunRPC]
		public void RemoteUseActiveBonus(int idx, double timestamp)
		{
			base.UseActiveBonus(idx, true, timestamp);
		}

		private void DetonateAllExplosiveObjects()
		{
			eoc.ExplodeAll();
		}

		[PunRPC]
		protected virtual void RemoteUseBonus(int b)
		{
			Bonus.Behaviour bonusBehaviour = (Bonus.Behaviour)b;
			switch(bonusBehaviour)
			{
				case Bonus.Behaviour.GlobalSlowdown:
				//case Bonus.Behaviour.TimeSpeedUp:
				case Bonus.Behaviour.Teleport:
				case Bonus.Behaviour.Detonation:
					gameMessage.SetBonusUsageEnded(bonusBehaviour);
				break;
			}
		}

		protected virtual void TeleportUsed()
		{
			photonView.RPC("TeleportAcquireRandomPositions", PhotonTargets.MasterClient);

			missions.IncrementMission(Config.Missions.MissionIDs.Mission_99, 1);
			missions.IncrementMission(Config.Missions.MissionIDs.Mission_100, 1);
			missions.IncrementMission(Config.Missions.MissionIDs.Mission_101, 1);
			missions.IncrementMission(Config.Missions.MissionIDs.Mission_102, 1);
			missions.IncrementMission(Config.Missions.MissionIDs.Mission_103, 1);
			missions.IncrementMission(Config.Missions.MissionIDs.Mission_104, 1);

			upgradeTree.IncrementItem(Config.UpgradeTree.ItemIDs.Item_14, 1);
		}


		//Tato funkce se vola jenom na Master clientovi a vola pote RPC OnRemoteTeleportUsed
		[PunRPC]
		protected virtual void TeleportAcquireRandomPositions()
		{
			byte[] acquiredByteData = teleportBonusImpl.AcquireRandomPositions(playersController.Objects);

			if(acquiredByteData != null && arenaEventDispatcher != null)
			{
				arenaEventDispatcher.TeleportAcquiredPositions(acquiredByteData);
			}
		}

		public virtual void OnTeleported(Vector3 teleportedPosition)
		{
			if(clientType == ClientType.LocalClient)
				hud.flashbang.TeleportFlash();
			
			//TODO: tady se musi vypnout interpolace + vyresi problem pri respawnu
			Debug.Log(name + " - Teleporting player " + position + " / " + teleportedPosition);

			positionRaw = teleportedPosition;
		}

		public override bool UseBonus(Bonus bonus, bool usedRemote)
		{
			base.UseBonus(bonus, usedRemote);

			if(bonus == null || bonus.behaviour == Bonus.Behaviour.None)
				return false;

			bonus.usedRemote = usedRemote;

			switch(bonus.behaviour)
			{
				// bonusy s casovou limitaci
				case Bonus.Behaviour.GrenadeMasking:
				case Bonus.Behaviour.MoreGrenadeDamage:
				case Bonus.Behaviour.Shield:
				//case Bonus.Behaviour.TimeSpeedUp:
				case Bonus.Behaviour.GlobalSlowdown:
				case Bonus.Behaviour.Speed:
				case Bonus.Behaviour.Magnet:
				case Bonus.Behaviour.Ghost:
				case Bonus.Behaviour.GrenadePlus:
				case Bonus.Behaviour.HolyExplosions:
				case Bonus.Behaviour.GunUpgrade:
				case Bonus.Behaviour.QuickExplodes:
					return bonusController.UseActiveOrPassiveBonus(bonus, this);

				// one time use bonusy

				case Bonus.Behaviour.Teleport:

					// nechceme aby ostatni klienti dotazovali na mastera - pouze ten co jej pouzil
					if(clientType == ClientType.LocalClient)
						TeleportUsed();

					bonusController.UseOnetimeBonus(bonus);
				break;
					
				case Bonus.Behaviour.Detonation:
					DetonateAllExplosiveObjects();
					bonusController.UseOnetimeBonus(bonus);
				break;

				case Bonus.Behaviour.HPPack:

					if(lives < initLives)
					{
						float lastNormalizedLives = normalizedLives;

						lives += initLives * Config.Bonuses.HPPack_Value;

						if(clientType == ClientType.LocalClient && hud != null)
						{
							if(lastNormalizedLives < 0.1f)
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_111, 1);
							
							hud.OnPlayerHPChanged(normalizedLives, 1f);
						}
					}
					else
						return false;
					
				break;

				case Bonus.Behaviour.PointsPack:
					
					if(clientType == ClientType.LocalClient)
					{
						int level = LocalClientRobotEmil.level;
						float pts = Config.Exp.GetLevelDesiredPoints(level < 2 ? 2 : level) * (level < 10 ? 0.1f : 0.05f);

						if(pts > Config.Bonuses.maxExpPowerupValue)
							pts = Config.Bonuses.maxExpPowerupValue;

						gameMessage.SetXPMessage((int)pts);

						score += (int)pts;
					}
				break;

				case Bonus.Behaviour.TopSecret:

					if(clientType == ClientType.LocalClient && !missions.IsMissionActivated(Config.Missions.MissionIDs.Mission_122))
					{
						missions.IncrementMission(Config.Missions.MissionIDs.Mission_122, 1);
						bonusController.UseOnetimeBonus(bonus);
					}
					else
					{
						return false;
					}
				break;
					
				/*
				case Bonus.Behaviour.BoxPack:
					//bonusController.OneTimeBonusUsed(bonus);
					if(isLocalClient)
						gameMessage.SetMessage(bonus);
					
					freeBoxes += Config.Bonuses.BoxPack_BoxesCount;
				break;
				*/
					
				default:
					Debug.Log("Unknown implementation of bonus " + bonus.behaviour);
				break;
			}

			return true;
		}

		#endregion

		public override void OnEnemyDied(WeaponType attackerWeaponType, int attackerWeaponProjectileType, RobotEmil enemy, bool suicide)
		{
			base.OnEnemyDied(attackerWeaponType, attackerWeaponProjectileType, enemy, suicide);

			if(!tutorial.isActive)
			{
				int pts = 0;

				if(!suicide)
				{
					killsFromLastSpawn++;

					pts = Config.Score.playerEnemyKilledPoints;

					if(clientType == ClientType.LocalClient)
						gameMessage.SetXPMessage(pts);
				}

				score += pts;
				kills += suicide ? -1 : 1;
			}

			if(suicide)
			{
				gameInfoStack.PlayerSuicide(color, nick);

				if(clientType == ClientType.LocalClient)
				{
					if(freezed)
					{
						upgradeTree.IncrementItem(Config.UpgradeTree.ItemIDs.Item_12, 1);
					}
				}
			}
			else
			{
				if(clientType == ClientType.LocalClient)
				{
					killCounter.OnKilledEnemy(this);

					missions.IncrementMissionWithCallbackOnCompletion(Config.Missions.MissionIDs.Mission_1, 1, () => upgradeTree.IncrementItem(Config.UpgradeTree.ItemIDs.Item_17, 1));
					missions.IncrementMission(Config.Missions.MissionIDs.Mission_2, 1);
					missions.IncrementMission(Config.Missions.MissionIDs.Mission_3, 1);
					missions.IncrementMission(Config.Missions.MissionIDs.Mission_4, 1);
					missions.IncrementMission(Config.Missions.MissionIDs.Mission_5, 1);
					missions.IncrementMission(Config.Missions.MissionIDs.Mission_6, 1);
					missions.IncrementMission(Config.Missions.MissionIDs.Mission_7, 1);
					missions.IncrementMission(Config.Missions.MissionIDs.Mission_8, 1);
					missions.IncrementMission(Config.Missions.MissionIDs.Mission_9, 1);
					missions.IncrementMission(Config.Missions.MissionIDs.Mission_10, 1);
					missions.IncrementMission(Config.Missions.MissionIDs.Mission_11, 1);

					upgradeTree.IncrementItem(Config.UpgradeTree.ItemIDs.Item_1, 1);

					if(state == State.Dead)
					{
						missions.IncrementMission(Config.Missions.MissionIDs.Mission_48, 1);
						missions.IncrementMission(Config.Missions.MissionIDs.Mission_49, 1);
						missions.IncrementMission(Config.Missions.MissionIDs.Mission_50, 1);
						missions.IncrementMission(Config.Missions.MissionIDs.Mission_51, 1);
						missions.IncrementMission(Config.Missions.MissionIDs.Mission_52, 1);

						upgradeTree.IncrementItem(Config.UpgradeTree.ItemIDs.Item_2, 1);
					}

					if(normalizedLives < 0.25f)
					{
						upgradeTree.IncrementItem(Config.UpgradeTree.ItemIDs.Item_15, 1);
					}

					if(killsFromLastSpawn >= 5 && killsFromLastSpawn <= 10)
					{
						//5 - 5 + 1 = 1
						//6 - 5 + 1 = 2
						//7 - 5 + 1 = 3
						//8 - 5 + 1 = 4
						//9 - 5 + 1 = 5
						//10 - 5 + 1 = 6

						int cnt = (killsFromLastSpawn - 5);
						upgradeTree.IncrementItem(Config.UpgradeTree.ItemIDs.Item_18, (int)Mathf.Pow(2, cnt));
					}

					if(ghostBonusUsed)
					{
						missions.IncrementMission(Config.Missions.MissionIDs.Mission_87, 1);
						missions.IncrementMission(Config.Missions.MissionIDs.Mission_88, 1);
						missions.IncrementMission(Config.Missions.MissionIDs.Mission_89, 1);
						missions.IncrementMission(Config.Missions.MissionIDs.Mission_90, 1);
						missions.IncrementMission(Config.Missions.MissionIDs.Mission_91, 1);
						missions.IncrementMission(Config.Missions.MissionIDs.Mission_92, 1);
					}

					if(enemy.isFlashed)
					{
						upgradeTree.IncrementItem(Config.UpgradeTree.ItemIDs.Item_5, 1);
					}

					if(attackerWeaponProjectileType >= 0 && attackerWeaponProjectileType != EnumsExtensions.UnknownEnum())
					{
						var projectileType = (ProjectileType)attackerWeaponProjectileType;

						switch(projectileType)
						{
							case ProjectileType.StickyGrenade:
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_12, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_13, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_14, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_15, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_16, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_17, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_18, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_19, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_20, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_21, 1);
							break;

							case ProjectileType.BouncyGrenade:
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_22, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_23, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_24, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_25, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_26, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_27, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_28, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_29, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_30, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_31, 1);
							break;

							case ProjectileType.HolyGrenade:
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_76, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_77, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_78, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_79, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_80, 1);
								missions.IncrementMission(Config.Missions.MissionIDs.Mission_81, 1);

								upgradeTree.IncrementItem(Config.UpgradeTree.ItemIDs.Item_6, 1);
							break;
						}
					}

					switch(attackerWeaponType)
					{
						case WeaponType.BaseballBatMetal:
						case WeaponType.BaseballBatWooden:
						case WeaponType.Crowbar:
						case WeaponType.Knife:
						case WeaponType.Taser:
							missions.IncrementMission(Config.Missions.MissionIDs.Mission_43, 1);
							missions.IncrementMission(Config.Missions.MissionIDs.Mission_44, 1);
							missions.IncrementMission(Config.Missions.MissionIDs.Mission_45, 1);
							missions.IncrementMission(Config.Missions.MissionIDs.Mission_46, 1);
							missions.IncrementMission(Config.Missions.MissionIDs.Mission_47, 1);
						break;
					}
				}

				gameInfoStack.PlayerKilled(color, nick, enemy.color, enemy.nick);
			}

		}

		public void OnMultikillHappened(int killCount)
		{
			switch(killCount)
			{
				case 2:
					snd.speech.DoubleKill();
				break;

				case 3:
					snd.speech.TripleKill();
				break;

				case 4:
					//default:
					//	if(killCount > 3)
					snd.speech.MultiKill();
				break;
			}

			if(killCount > 2 && clientType == ClientType.LocalClient)
				upgradeTree.IncrementItem(Config.UpgradeTree.ItemIDs.Item_3, killCount);
		}

		protected override void OnLivesChanged(float val)
		{
			if(clientType == ClientType.LocalClient && hud != null)
				hud.SetHP(normalizedLives);
		}

		protected override void OnDeath(bool suicide)
		{
			if(deathEffect != null)
				deathEffect.Show();

			deaths++;
			
			base.OnDeath(suicide);

			switch(clientType)
			{
				case ClientType.RemoteClient:
				break;
				
				case ClientType.LocalClient:
					
					if(hud != null)
						hud.OnLocalPlayerDied();

					killCounter.Reset();

					if(suicide)
						snd.speech.Suicide();
					else
						snd.speech.Killed();
					
				break;
				
				case ClientType.BotClient:
					
					if(!suicide)
						tutorial.HandleEvent(TutorialEvent.OnBotKilled);

				break;
			}

			bonusController.StopBonusesDispatchAfterDeath(this);
		}

		protected override void OnDeathEnd()
		{
			if(this == null)
				return;

			base.OnDeathEnd();
			Respawn();
		}

		#region Death trigger

		private void CheckArenaBounds()
		{
			bool isInBounds = arenaEventDispatcher.IsRobotInBounds(this);

			if(!isInBounds)
			{
				Debug.LogWarning(name + " detected robot out of bounds, killing himself");

				// zabiju se
				Hit(this, WeaponType.Knife, -1, 1f, Mathf.Infinity);
			}
		}


		#endregion


		#region Boxes

		public override void OnBoxDestroyed(Entities.BoxDestroyable box, IProjectileObjectWithExplosion projectile)
		{
			base.OnBoxDestroyed(box, projectile);

			boxes++;

			if(clientType == ClientType.LocalClient)
				upgradeTree.IncrementItem(Config.UpgradeTree.ItemIDs.Item_4, 1);
		}

		public void OnIsBestBoxDestroyer()
		{
			if(clientType == ClientType.LocalClient)
			{
				gameMessage.SetMessageLocalized("SC_IsBestBoxDestroyer", GameMessage.MsgType.MiddleBigNotice, Config.Score.bestBoxDestroyerPoints);
				score += Config.Score.bestBoxDestroyerPoints;
			}
		}

		#endregion

		#region Exps

		public void OnLevelChanged(int diff)
		{
			//Debug.Log("OnLevelChanged " + diff);

			if(diff < 0)
				gameMessage.SetMessageLocalized("EX_LevelDown", GameMessage.MsgType.MiddleBigNotice);
			else
			{
				gameMessage.SetMessageLocalized("EX_LevelUp", GameMessage.MsgType.MiddleBigNotice);
				snd.Play(Config.Sounds.levelUp);
			}
		}

		public void OnLocalScoreChanged(int level, int currPts, int nextPts, float progress)
		{
			if(clientType == ClientType.LocalClient && hud != null && hud.playerLevelIndicator != null)
				hud.playerLevelIndicator.SetXP(level, currPts, nextPts, progress);
		}

		#endregion

		#region Score

		public void OnIsLastSurvivor()
		{
			if(clientType == ClientType.LocalClient)
			{
				gameMessage.SetMessageLocalized("SC_LastSurvivor", GameMessage.MsgType.MiddleBigNotice, Config.Score.playerLastSurvivorPoints);
				score += Config.Score.playerLastSurvivorPoints;

				missions.IncrementMission(Config.Missions.MissionIDs.Mission_68, 1);
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_69, 1);
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_70, 1);
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_71, 1);
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_72, 1);
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_73, 1);
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_74, 1);
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_75, 1);

				upgradeTree.IncrementItem(Config.UpgradeTree.ItemIDs.Item_9, 1);
			}
		}

		public void OnIsSecondSurvivor()
		{
			if(clientType == ClientType.LocalClient)
			{
				gameMessage.SetMessageLocalized("SC_SecondSurvivor", GameMessage.MsgType.MiddleBigNotice, Config.Score.playerSecondSurvivorPoints);
				score += Config.Score.playerSecondSurvivorPoints;

				upgradeTree.IncrementItem(Config.UpgradeTree.ItemIDs.Item_10, 1);
			}
		}

		public void OnIsThirdSurvivor()
		{
			if(clientType == ClientType.LocalClient)
			{
				upgradeTree.IncrementItem(Config.UpgradeTree.ItemIDs.Item_11, 1);
			}
		}

		#endregion

		#region GlobalSlowdown

		public override void GlobalSlowdownUsed(bool used, bool usedRemote)
		{
			base.GlobalSlowdownUsed(used, usedRemote);

			if(botClient != null)
				botClient.GlobalSlowdownUsed(used, usedRemote);
		}

		#endregion

		#region Ghost

		public override void GhostUsed(bool used, float ghostSpeedMultiplier)
		{
			base.GhostUsed(used, ghostSpeedMultiplier);

			if(botClient != null)
				botClient.GhostUsed(used);
			
		}

		#endregion

		#region Magnet

		public override void MagnetUsed(bool used)
		{
			base.MagnetUsed(used);

			if(botClient != null)
				botClient.MagnetUsed(used);
		}

		#endregion

		#region Shield

		public override void ShieldUsed(bool used)
		{
			base.ShieldUsed(used);

			if(botClient != null)
				botClient.ShieldUsed(used);
		}

		#endregion

		#region Speed
	
		public override void SetSpeedMultiplier(float multiplier)
		{
			//#if UNITY_EDITOR
			//Debug.Log("SetSpeedMultiplier " + multiplier);
			//#endif

			base.SetSpeedMultiplier(multiplier);

			if(botClient != null)
				botClient.SetSpeedMultiplier(multiplier);
		}

		#endregion

		#region Flash

		public override void SetIsFlashed(bool flashed)
		{
			base.SetIsFlashed(flashed);

			//TODO:?
		}

		#endregion
	}
}
