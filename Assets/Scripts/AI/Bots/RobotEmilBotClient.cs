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
using GMReloaded.Entities;

namespace GMReloaded.AI.Bots
{
	public class RobotEmilBotClient : MonoBehaviourTO 
	{
		public enum State : byte
		{
			Idle,
			Move,
			Dead
		}

		private string _nick = "StupidBot";
		public string nick { get { return _nick; } }

		[SerializeField]
		private float agentRadius = 0.5f;

		[SerializeField]
		private float agentHeight = 2f;

		private float _agentInitSpeed = -1f;
		private float agentInitSpeed
		{
			get
			{ 
				if(_agentInitSpeed < 0f && agent != null)
					_agentInitSpeed = agent.speed;

				return _agentInitSpeed;
			}
		}

		[SerializeField]
		private float agentBaseOffset = 0.94f;

		[SerializeField]
		private float agentAngularSpeed = 0f;

		[SerializeField]
		private float agentVisibilityRadius = 7f;

		[SerializeField]
		private bool shootingEnabled = true;

		private NavMeshAgent _agent;
		private NavMeshAgent agent
		{
			get
			{
				if(_agent == null)
					_agent = gameObject.AddComponent<NavMeshAgent>();

				return _agent;
			}
		}

		[SerializeField]
		private State state;

		private EquipedWeapons _equipedWeapons = new EquipedWeapons(false);
		public EquipedWeapons equipedWeapons { get { return _equipedWeapons; } }

		public PhotonPlayer photonPlayer { get; private set; }

		#region Synced states

		[SerializeField]
		public Vector3 controlDirection;

		[SerializeField]
		public RobotEmilViewObserver.Direction directionState = RobotEmilViewObserver.Direction.Stand;

		[SerializeField]
		public bool running = false;

		#endregion

		private Vector3 lookDirection;

		private RobotEmilNetworked robotParent;

		public PhotonPlayer botOwner { get; private set; }

		private BotController botController;

		//

		private SpawnManager spawnManager { get { return SpawnManager.Instance; } }

		private EntityController entityController { get { return EntityController.Instance; } }

		private ArenaEventDispatcher arenaEventDispatcher { get { return ArenaEventDispatcher.Instance; } }

		//

		#region Unity

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(position, agentVisibilityRadius);

			Gizmos.color = Color.blue;
			if(currAgentPath != null)
			{
				for(int i = 0; i < currAgentPath.corners.Length-1; i++)
				{
					var p0 = currAgentPath.corners[i];
					var p1 = currAgentPath.corners[i+1];

					Gizmos.DrawLine(p0, p1);
				}
			}
		}

		private void Update()
		{
			if(robotParent == null || robotParent.state == RobotEmil.State.Unspawned || robotParent.state == RobotEmil.State.Dead)
				return;
			
			UpdateAgentState();
		}

		#endregion

		#region Bot Init

		public void SetBotParent(RobotEmilNetworked robotParent, PhotonPlayer botOwner, BotController botController, int photonBotId, int botId)
		{
			if(robotParent == null)
			{
				Debug.LogError("robotParent == null");
				return;
			}

			_nick = Config.Bots.robotNames[(botId + Random.Range(0, 100)) % Config.Bots.robotNames.Length];

			this.robotParent = robotParent;
			this.botOwner = botOwner;
			this.botController = botController;

			this.photonPlayer = new PhotonPlayer(true, photonBotId, nick);

			robotParent.SetPlayerColor(Color.red);

			robotParent.SetSkin((RobotEmil.Skin)((((int)LocalClientRobotEmil.user.skin) + botId + 1) % 4));

			robotParent.PairBotPhotonPlayer(photonPlayer, botOwner);

			robotParent.name = "BOT - RobotEmil " + photonBotId;
		}

		public void Spawn(SpawnPoint spawn)
		{
			if(robotParent == null)
				return;

			robotParent.Spawn(spawn);
		}

		#endregion

		#region Tutorial Specific Impl

		public void SetIdleBot_Tutorial()
		{
			if(!tutorial.isActive)
				return;

			agent.enabled = false;
			shootingEnabled = false;
		}

		#endregion

		#region AI State

		private void SetState(State state, bool forced = false)
		{
			if(this.state == state)
				return;

			this.state = state;

			switch(state)
			{
				case State.Idle:

					//idleTime = Random.Range(1f, 4f);
					idleTime = Random.Range(0f, 1.3f);


					robotParent.SetState(RobotEmil.State.Idle);

					directionState = RobotEmilViewObserver.Direction.Stand;

				break;
					
				case State.Move:

					//directionState = RobotEmilViewObserver.Direction.Front;

					robotParent.SetState(RobotEmil.State.Move);

				break;

				case State.Dead:
				break;
					
				default:
				break;
			}
		}

		private float idleTime = 0f;
		private float idleTimer = 0f;

		private float shootTimer = 0f;

		private float bonusUpdateTime = 12f;
		private float bonusUpdateTimer = 0f;

		private float diarrheaShootTimer = 0f;

		private void UpdateAgentState()
		{
			UpdateWeaponEquip();

			bool silenceEnabled = false;
			float diarrheaShootTime = -1.0f;

			if(arenaEventDispatcher != null && arenaEventDispatcher.madnessMode != null)
			{
				silenceEnabled = arenaEventDispatcher.madnessMode.isSilenceEnabled;
				diarrheaShootTime = arenaEventDispatcher.madnessMode.diarrheaShootTime;
			}

			//

			if(!silenceEnabled && shootingEnabled)
			{
				if(diarrheaShootTime >= 0.0f) // pokud je spustena bude cas >= 0
				{
					if(diarrheaShootTimer < diarrheaShootTime)
						diarrheaShootTimer += Time.deltaTime;

					if(diarrheaShootTimer >= diarrheaShootTime)
					{
						diarrheaShootTimer = 0.0f;
						UpdateAgentShooting();
					}
				}
				else
				{
					var cwp = currWeaponProbability;

					float shootTime = cwp == null ? 3f : cwp.shootTime;

					if(shootTimer < shootTime)
						shootTimer += Time.deltaTime;

					if(shootTimer > shootTime)
						UpdateAgentShooting();
				}
			}

			//

			if(bonusUpdateTimer < bonusUpdateTime)
				bonusUpdateTimer += Time.deltaTime;

			if(bonusUpdateTimer > bonusUpdateTime)
			{
				OnTryUseBonus();
				bonusUpdateTimer = 0f;
			}


			if(state != State.Dead)
			{
				lookDirection = agent.velocity.normalized;

				if(lookDirection.magnitude > 0.0f)
				{
					directionState = (lookDirection.x > 0f || lookDirection.z > 0f) ? RobotEmilViewObserver.Direction.Front : RobotEmilViewObserver.Direction.Back;
				}
				else
				{
					directionState = RobotEmilViewObserver.Direction.Stand;
				}
			}



			switch(state)
			{
				case State.Idle:

					//directionState = RobotEmilViewObserver.Direction.Stand;

					if(idleTimer < idleTime) 
						idleTimer += Time.deltaTime;

					if(idleTimer >= idleTime)
					{
						OnIdleRefreshTriggered();

						idleTimer = 0f;
					}

				break;
					
				case State.Move:

					running = true;

					//Debug.Log(lookDirection + " - " + directionState);

					controlDirection = new Vector3(0f, Mathf.Abs(lookDirection.z), 0f);

					if(agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
					{
						if(!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
						{
							OnReachedPathDestination();
						}
					}

				break;


				case State.Dead:
				break;
			}

			if(lookDirection.sqrMagnitude > 0.0f)
			{
				robotParent.Rotate(Quaternion.LookRotation(lookDirection).eulerAngles, Timer.unscaledDeltaTime * 10f);
			}
		}

		#endregion 

		#region Weapons + Shooting

		private Dictionary<WeaponType, WeaponProbability> weaponProbabilities = new Dictionary<WeaponType, WeaponProbability>()
		{
			{ 
				WeaponType.GrenadeLauncherSticky, 
				new WeaponProbability()
				{
					firstTimeWeapon = true,
					minActivityTime = 0f,
					probabilityMin = 0.5f,
					probabilityMax = 1f,
					maxUsageTime = 50f,

					shootTimeMin = 0.5f,
					shootTimeMax = 1.8f,
				}.Setup()
			},
			{ 
				WeaponType.GrenadeLauncherBouncy, 
				new WeaponProbability() 
				{
					minActivityTime = 20f,
					probabilityMin = 0.6f,
					probabilityMax = 1f,
					maxUsageTime = 70f,

					shootTimeMin = 1.7f,
					shootTimeMax = 3f,
				}.Setup()
			},
			{ 
				WeaponType.GrenadeLauncherFlash, 
				new WeaponProbability() 
				{
					minActivityTime = 120f,
					probabilityMin = 0.1f,
					probabilityMax = 0.4f,
					maxUsageTime = 10f,

					shootTimeMin = 8f,
					shootTimeMax = 10f,
				}.Setup()
			},
			{ 
				WeaponType.GrenadeLauncherSmoke, 
				new WeaponProbability()
				{
					minActivityTime = 120f,
					probabilityMin = 0.01f,
					probabilityMax = 0.02f,
					maxUsageTime = 10f,

					shootTimeMin = 10f,
					shootTimeMax = 15f,
				}.Setup()
			},
			{ 
				WeaponType.GrenadeLauncherLaserMine, 
				new WeaponProbability() 
				{
					minActivityTime = 270f,
					probabilityMin = 0.05f,
					probabilityMax = 0.1f,
					maxUsageTime = 30f,

					shootTimeMin = 3f,
					shootTimeMax = 8f,
				}.Setup()
			},
			{ 
				WeaponType.GrenadeLauncherTesla, 
				new WeaponProbability() 
				{
					minActivityTime = 120f,
					probabilityMin = 0.1f,
					probabilityMax = 0.6f,
					maxUsageTime = 40f,

					shootTimeMin = 2f,
					shootTimeMax = 3f,
				}.Setup()
			},
			{ 
				WeaponType.BoxCreator, 
				new WeaponProbability() 
				{
					firstTimeWeapon = true,
					minActivityTime = 360f,
					probabilityMin = 5f,
					probabilityMax = 1f,
					maxUsageTime = 20f,

					shootTimeMin = 1f,
					shootTimeMax = 3f,
				}.Setup()
			},
			/*{ 
				WeaponType.ExplosiveBarrelCreator, 
				new WeaponProbability() 
				{
					minActivityTime = 120f,
					probabilityMin = 0.05f,
					probabilityMax = 0.1f,
					maxUsageTime = 30f,

					shootTimeMin = 3f,
					shootTimeMax = 4f,
				}.Setup()
			},*/
			{ 
				WeaponType.Knife, 
				new WeaponProbability() 
				{
					firstTimeWeapon = true,
					minActivityTime = 720f,
					probabilityMin = 0.5f,
					probabilityMax = 1f,
					maxUsageTime = 20f,

					shootTimeMin = 3f,
					shootTimeMax = 10f,
				}.Setup()
			},
		};

		private WeaponProbability currWeaponProbability { get { return robotParent == null ? null : GetWeaponProbability(robotParent.currWeaponType); } }

		private WeaponProbability GetWeaponProbability(WeaponType weaponType)
		{
			WeaponProbability prob = null;

			weaponProbabilities.TryGetValue(weaponType, out prob);

			return prob;
		}

		private void EquipWeapons(bool initialEquip = false)
		{
			HashSet<WeaponType> currEquippedWeapons = new HashSet<WeaponType>();

			int steps = 100;
			while(currEquippedWeapons.Count < Config.Weapons.maxEquipedWeapons && steps-- > 0)
			{
				foreach(var kvp in weaponProbabilities)
				{
					var wp = kvp.Value;

					if(wp != null && ((initialEquip && wp.firstTimeWeapon) || Random.Range(0f, 100f) < wp.probability * 100f))
					{
						var weaponType = kvp.Key;

						int idx = currEquippedWeapons.Count;

						if(idx >= Config.Weapons.maxEquipedWeapons || !currEquippedWeapons.Add(weaponType))
							continue;
						
						Debug.Log("BOT - Equipping weapon " + weaponType + " / " + idx);

						equipedWeapons.SetWeaponAtSlot(idx, weaponType, false);
					}	
				}
			}

			if(steps <= 0)
				Debug.LogError("Failed to equip bot weapons -- invalid probabilities");
		}

		private int weaponChangeCounter = 0;
		private void TryChangeCurrentWeapon()
		{
			if(weaponChangeCounter > 5)
			{
				Debug.Log("weaponChangeCounter " + weaponChangeCounter);

				EquipWeapons();
				weaponChangeCounter = 0;
			}

			int d = 1;

			int weaponIdx = robotParent.PeekNextWeaponIdx(d);

			var weaponProbability = GetWeaponProbability(equipedWeapons.GetWeaponAt(weaponIdx));

			Debug.Log("weaponIdx " + weaponIdx + " - " + robotParent.activityTime +" >= " + weaponProbability.minActivityTime);

			if(weaponProbability != null && robotParent.activityTime >= weaponProbability.minActivityTime)
			{
				robotParent.SwitchToNextWeapon(d);
			}

			weaponChangeCounter++;
		}

		private void UpdateWeaponEquip()
		{
			var cvp = currWeaponProbability;

			if(cvp != null)
			{
				float maxTime = currWeaponProbability.maxUsageTime;
				float usedTime = currWeaponProbability.usedTime;

				if(usedTime < maxTime)
					usedTime += Time.deltaTime;

				if(usedTime >= maxTime)
				{
					usedTime = 0f;
					TryChangeCurrentWeapon();
				}

				currWeaponProbability.usedTime = usedTime;
			}
		}

		private void UpdateAgentShooting()
		{
			var entityToAttack = FindEntityToAttack();

			if(entityToAttack != null)
			{
				robotParent.PrepareForAttack(RobotEmil.AttackType.Primary);
				robotParent.AttackBeingHeld(RobotEmil.AttackType.Primary);
				int attackResult = robotParent.Attack(RobotEmil.AttackType.Primary); //TODO: boti by meli mit moznost zkusit i secondary attack

				if(attackResult < 0)
				{
					TryChangeCurrentWeapon();
				}

				if(currWeaponProbability != null)
					currWeaponProbability.RefreshShootTime();

				MoveToRandomPoint();

				shootTimer = 0f;
			}
		}

		#endregion

		private void OnTryUseBonus()
		{
			for(int i = 0; i < Config.Player.KeyBind.bonusKeys.Length; i++)
			{
				if(robotParent.UseActiveBonus(i, false))
				{
					Debug.Log("Bot " + nick + " Used bonus " + i);
					break;
				}
			}
		}

		#region AI Movement

		private void OnIdleRefreshTriggered()
		{
			MoveToRandomPoint();
		}

		private void OnReachedPathDestination()
		{
			SetState(State.Idle);
			/*
			var p = position;
			var d = robotParent.direction;
			var left = Vector3.Cross(d, Vector3.up);

			Ray[] rays = new Ray[]
			{ 
				new Ray(p, d),
				new Ray(p, -d),
				new Ray(p, left),
				new Ray(p, -left),
			};

			float hitDistance = 0f;
			Vector3 desiredDir = Vector3.zero;
			for(int i = 0; i < rays.Length; i++)
			{
				var r = rays[i];

				RaycastHit hitOutput;
				bool hit = Physics.Raycast(r, out hitOutput, 3f);
					
				if(i == 0)
				{
					if(!hit)
						break;
				}
				else
				{
					if(hit)
					{
						//if(hitOutput.distance > hitDistance)
						//{
						//	hitDistance = hitOutput.distance;
						//	desiredDir = r.direction;
						//}
					}
					else
					{
						desiredDir = r.direction;
					}
				}
			}

			Debug.Log("desiredDir " + desiredDir);

			if(desiredDir.sqrMagnitude > 0)
				lookDirection = desiredDir;
			*/
		}

		private bool MoveToRandomPoint()
		{
			Vector3 moveToPoint;

			// mas 10 pokusu negre
			for(int i = 0; i < 200; i++)
			{
				bool foundPoint = FindRandomPointOnNavMesh(position, 10f, out moveToPoint);

				if(foundPoint)
					return MoveTo(moveToPoint);
			}

			Debug.LogWarning("Failed to find random point to move");

			return false;
		}

		private NavMeshPath currAgentPath = new NavMeshPath();
		private bool MoveTo(Vector3 point)
		{
			if(!agent.isOnNavMesh)
				return false;

			agent.CalculatePath(point, currAgentPath);

			switch(currAgentPath.status)
			{
				case NavMeshPathStatus.PathComplete:

					agent.path = currAgentPath;
					SetState(State.Move);

				return true;

				default:
				case NavMeshPathStatus.PathPartial:
				case NavMeshPathStatus.PathInvalid:
				break;
			}

			return false;
		}

		#endregion

		#region AI Entities search

		private List<IObjectWithPosition> entitiesInRadius = new List<IObjectWithPosition>();
		private IObjectWithPosition FindEntityToAttack()
		{
			entitiesInRadius.Clear();

			var otherRobots = PlayersController.Instance.Objects;
			foreach(var kvp in otherRobots)
			{
				var otherRobot = kvp.Value;

				if(otherRobot != null && otherRobot != robotParent)
				{
					entitiesInRadius.Add(otherRobot);
				}
			}

			entityController.GetVisibleObjectsInDistance(position, agentVisibilityRadius, ObjectTypes.BoxDestroyable, ref entitiesInRadius);

			IObjectWithPosition bestEntity = null;


			if(entitiesInRadius.Count > 0)
			{
				var botPosition = position;

				entitiesInRadius.Sort((IObjectWithPosition o1, IObjectWithPosition o2) =>
				{ 
					float da = (botPosition - o1.position).sqrMagnitude;
					float db = (botPosition - o2.position).sqrMagnitude;

					if(da < db)
						return 1;
					else if(db < da)
						return -1;
					
					return 0;
				});

				foreach(var entity in entitiesInRadius)
					DebugExtension.DebugWireSphere(entity.position, Color.red, 1f, 0f, true);
			
				bestEntity = entitiesInRadius[0];

				if(bestEntity != null)
				{
					DebugExtension.DebugWireSphere(bestEntity.position, Color.green, 1.1f, 0f, true);
				}
			}

			return bestEntity;
		}

		#endregion

		#region AI Navmesh

		public static bool FindRandomPointOnNavMesh(Vector3 center, float range, out Vector3 result) 
		{
			return FindPointOnNavMesh(center + Random.insideUnitSphere * range, out result);
		}

		public static bool FindPointOnNavMesh(Vector3 center, out Vector3 result)
		{
			for(int i = 0; i < 30; i++) 
			{
				NavMeshHit hit;

				if(NavMesh.SamplePosition(center, out hit, 1.0f, NavMesh.AllAreas)) 
				{
					result = hit.position;
					return true;
				}
			}

			result = Vector3.zero;
			return false;
		}

		#endregion

		private void SetAgentActive(bool active)
		{
			//toto je tady schvalne debile
			if(_agent != null)
				_agent.enabled = active;
		}

		public void FirstSpawn()
		{
			if(robotParent == null)
			{
				Debug.LogError("robotParent == null");
				return;
			}

			EquipWeapons(true);
			
			robotParent.SpawnOnFreeSpawn();

			agent.radius = agentRadius;
			agent.height = agentHeight;
			agent.baseOffset = agentBaseOffset;
			agent.angularSpeed = agentAngularSpeed;
			agent.updateRotation = false;
		}

		public void OnSetClientTypeBotClient()
		{
			if(robotParent == null)
			{
				Debug.LogError("robotParent == null");
				return;
			}

			var robotView = robotParent.viewObserver.view as RobotEmilViewBotClient;

			if(robotView == null)
			{
				Debug.LogError("Failed to set RobotEmilViewBotClient parent bot");
				return;
			}

			robotView.SetBotClient(this);
		}

		#region Bonuses Events

		public void GlobalSlowdownUsed(bool used, bool usedRemote)
		{

		}

		public void GhostUsed(bool used)
		{

		}

		public void MagnetUsed(bool used)
		{
			SetAgentActive(!used);
		}

		public void ShieldUsed(bool used)
		{

		}

		public void SetSpeedMultiplier(float multiplier)
		{
			agent.speed = agentInitSpeed * multiplier;
		}

		#endregion

		public void OnFlashPlayer(float visibility)
		{
		}
	
		// toto se vola po spawnu
		public void OnRemoteSpawnPositionAcquired(int photonPlayerId, Vector3 spawnPosition, float spawnPositionAngleY)
		{
			SetAgentActive(true);
			SetState(State.Idle);
		}

		public void OnDeath(bool suicide)
		{
			SetAgentActive(false);
			SetState(State.Dead);
		}

		public override void Destroy()
		{
			if(this != null)
				Destroy(gameObject);
		}

		protected override void OnDestroy()
		{
			if(botController != null)
				botController.RemoveBotOnDestroy(this);

			base.OnDestroy();
			botController = null;
		}
	}
}