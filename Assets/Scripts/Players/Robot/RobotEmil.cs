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
using GMReloaded.Bonuses.Effects;
using System.Collections.Generic;
using Independent;
using TouchOrchestra;
using System;
using CodeStage.AntiCheat.ObscuredTypes;
using GMReloaded.Bonuses;

namespace GMReloaded
{

	public abstract class RobotEmil : IAnimatorMonoBehaviour, IAliveObject
	{
		public enum State : byte
		{
			Unspawned,
			Idle,
			Move,
			Dead
		}

		public enum Skin : byte
		{
			Red,
			Blue,
			Green,
			Yellow
		}

		public enum ClientType
		{
			RemoteClient,
			LocalClient,
			BotClient
		}

		public enum AttackType : byte
		{
			Primary,
			Secondary
		}

		public enum SecondaryAttackType
		{
			None,
			GunUpgrade,
			HolyExplosions
		}

		#region Constants

		#endregion

		public abstract string nick { get; }

		#region Serialized Fields


		[SerializeField]
		protected Ragdoll ragdoll;

		public class AnimatorHashIDs
		{
			public readonly int DirX = Animator.StringToHash("DirX");
			public readonly int DirY = Animator.StringToHash("DirY");
			public readonly int MouseDirY = Animator.StringToHash("MouseDirY");
			public readonly int Running = Animator.StringToHash("Running");
			public readonly int Walking = Animator.StringToHash("Walking");
			public readonly int Shooting = Animator.StringToHash("Shooting");
			public readonly int Stab = Animator.StringToHash("Stab");
			public readonly int Attack = Animator.StringToHash("Attack");
		}

		private AnimatorHashIDs animHashId = new AnimatorHashIDs();

		[SerializeField]
		protected Transform weaponGripBone;

		//

		private float grenadeDamageMultiplier_Bonus = 1f;
		private float grenadeDamageMultiplier_MadnessMode = 1f;

		public float grenadeDamageMultiplier
		{
			get 
			{ 
				float d = (grenadeDamageMultiplier_Bonus - 1f) + (grenadeDamageMultiplier_MadnessMode - 1f);
				return 1f + d;
			}
		}

		//

		private float grenadeRadiusMultiplier_Bonus = 1f;
		private float grenadeRadiusMultiplier_MadnessMode = 1f;

		public float grenadeRadiusMultiplier
		{
			get
			{
				float d = (grenadeRadiusMultiplier_Bonus - 1f) + (grenadeRadiusMultiplier_MadnessMode - 1f);
				return 1f + d;
			}
		}

		//

		[SerializeField]
		protected CharacterController characterController;

		[SerializeField]
		protected GameObject rigidCharacterCollider;

		[SerializeField]
		public RobotEmilViewObserver viewObserver;

		[SerializeField]
		protected Transform notRotatedContainer;

		[SerializeField]
		protected Transform rotatedContainer;

		[SerializeField]
		public Entities.BoxDestroyableIndicator cornerBoundingBox;


		[SerializeField]
		public ObscuredFloat speedMultiplier = 1f;

		[SerializeField]
		public ClientType _clientType = ClientType.RemoteClient;
		public virtual ClientType clientType
		{
			get { return _clientType; }
			set
			{ 
				_clientType = value;

				if(viewObserver != null)
					viewObserver.SetClientType(_clientType);

				switch(_clientType)
				{
					case ClientType.RemoteClient:
						OnSetClientTypeRemoteClient();
					break;
						
					case ClientType.BotClient:
						OnSetClientTypeBotClient();
					break;
						
					case ClientType.LocalClient:
						OnSetClientTypeLocalClient();
					break;
				}
			}
		}

		public abstract Color color { get; protected set; }

		public AI.Bots.RobotEmilBotClient botClient { get; protected set; }

		public RobotEmil ParentRobot { get { return this; } }

		#endregion

		#region Bonus Effects

		[SerializeField]
		public ShieldEffect shield;

		[SerializeField]
		public MagnetEffect magnet;

		#endregion

		#region Lives

		public override Vector3 localEulerAngles
		{
			get
			{
				if(rotatedContainer == null)
					return Vector3.zero;

				return rotatedContainer.localEulerAngles;
			}
			set
			{
				if(rotatedContainer != null)
					rotatedContainer.localEulerAngles = value;
			}
		}

		public override Quaternion localRotation
		{
			get
			{
				if(rotatedContainer == null)
					return Quaternion.identity;

				return rotatedContainer.localRotation;
			}

			set
			{ 
				if(rotatedContainer != null)
					rotatedContainer.localRotation = value;
			}
		}

		public float angleY
		{
			get { return localEulerAngles.y; }
			set 
			{ 
				Vector3 lea = localEulerAngles;
				lea.y = value;
				localEulerAngles = lea;
			}
		}

		[SerializeField]
		protected ObscuredFloat _lives;

		public virtual float lives
		{
			get { return _lives; }

			set 
			{ 
				_lives = Mathf.Clamp(value, 0f, initLives); 
				OnLivesChanged(_lives);
			}
		}

		[SerializeField]
		protected ObscuredFloat initLives = 13f;

		protected float normalizedLives { get { return _lives / initLives; } }

		[SerializeField]
		private float _damageRadius = 0.5f;
		public float damageRadius { get { return _damageRadius; } }

		#endregion

		#region RobotEmil View

		[SerializeField]
		public bool freezed;

		[SerializeField]
		public RobotEmilViewObserver.Direction directionState;

		[SerializeField]
		public Vector3 controlDirection;

		[SerializeField]
		public bool running;

		[SerializeField]
		public bool isFlashed;

		#endregion

		public State state;

		public Vector3 direction;

		public float activityTimeFromLastSpawn = 0f;

		public float activityTime = 0f;

		[SerializeField]
		protected int killsFromLastSpawn = 0;

		[SerializeField]
		private bool isAttacking = false;

		protected EquipedWeapons equipedWeapons { get { return botClient != null ? botClient.equipedWeapons : Config.Weapons.localClientEquipedWeapons; } }

		#region Skin

		[Serializable]
		public class SkinContainer
		{
			public Skin skin;
			public Material material;
		}

		[SerializeField]
		private SkinContainer [] predefinedSkins;

		[SerializeField]
		public Skin skin;

		protected Material lastSkinMaterial;
		protected Material skinMaterial;

		[SerializeField]
		private Renderer [] skinRenderers;

		//

		private Shader defaultSkinShader;

		[SerializeField]
		private Shader electrovisionShader;

		//

		[SerializeField]
		private Material ghostMaterial;

		public ObscuredFloat meleeDamageMultiplier = 1f;

		protected bool ghostBonusUsed = false;

		//

		#endregion

		#region Sounds

		[SerializeField]
		protected RobotEmilStepsSound stepsSound = new RobotEmilStepsSound();

		#endregion

		//public virtual int freeBoxes { get;  set; }

		//public bool canUseBoxDestroyableTool { get { return freeBoxes > 0; } }

		#region Singleton getters

		protected IAliveObjectsController aoc { get { return IAliveObjectsController.Instance; } }

		protected IExplosiveObjectsController eoc { get { return IExplosiveObjectsController.Instance; } }

		protected PlayersController playersController { get { return PlayersController.Instance; } }

		protected WeaponRecycler weaponRecycler { get { return WeaponRecycler.Instance; } }

		protected Bonuses.BonusController bonusController { get { return Bonuses.BonusController.Instance; } }

		protected GameMessage gameMessage { get { return HUD.IsNull ? null : HUD.Instance.gameMessage; }}

		protected RadialBlur radialBlur { get { return RobotEmilImageEffects.Instance.radialBlur; } }

		protected Pixelize pixelize { get { return RobotEmilImageEffects.Instance.pixelize; } }

		protected GMReloaded.Shaders.PostProcess.HolyGrenadeSignalization holyGrenadeSignalization { get { return RobotEmilImageEffects.Instance.holyGrenadeSignalization; } }

		protected HUD hud { get { return HUD.Instance; } }

		protected ArenaEventDispatcher arenaEventDispatcher { get { return ArenaEventDispatcher.Instance; } }

		protected SpawnManager spawnManager { get { return SpawnManager.Instance; } }

		protected Achievements.MissionsController missions { get { return Achievements.MissionsController.Instance; } }

		protected UpgradeTree.UpgradeTreeController upgradeTree { get { return UpgradeTree.UpgradeTreeController.Instance; } }

		#endregion

		private ISound walkSound;
		private SoundContainer hitSound;
		private ISound deathSound;

		#region Unity

		protected virtual void Awake() 
		{
			Assert.IsAssigned(viewObserver);

			//MixAnimations(AttackAnim, blendBone);
			//MixAnimations(StabAnim, blendBone);
			//MixAnimations(TaserStabAnim, blendBone);
		}

		protected virtual void Start()
		{
			OnLoaded();

			hitSound = new SoundContainer("SNDC_RobotEmil_HitContainer");

			for(int i = 0; i < 2; i++)
				hitSound.AddSound(Config.Sounds.robotHit + i);

			stepsSound.Initialize(this);
		}

		protected virtual void OnDrawGizmos()
		{
			//Gizmos.color = Color.red;
			//Gizmos.DrawWireSphere(position, damageRadius);
		}

		protected virtual void Update()
		{
			Vector3 lr = localEulerAngles;
			direction = new Vector3(Mathf.Sin(lr.y * Mathf.Deg2Rad), 0f, Mathf.Cos(lr.y * Mathf.Deg2Rad));

			bool seenByHolyGrenade = false;

			if(state != State.Dead)
			{
				activityTimeFromLastSpawn += Time.deltaTime;
				activityTime += Time.deltaTime;

				seenByHolyGrenade = eoc.IsSeenByHolyGrenade(this);
			}
			else
			{
				seenByHolyGrenade = false;
			}

			if(holyGrenadeSignalization != null && clientType == ClientType.LocalClient)
				holyGrenadeSignalization.SetActive(seenByHolyGrenade);
			
			/*if(state != State.Unspawned && state != State.Dead)
			{
				if(clientType != ClientType.RemoteClient && activityTime > Config.Grenades.secondGrenadeAddTime && numBonusGrenades == 0)
				{
					AddNewGrenade(true);
				}
			}*/

			stepsSound.Update(Time.deltaTime);


			switch(state)
			{
				case State.Unspawned:
				break;
					
				case State.Idle:
					OnUpdateIdleState();
				break;
					
				case State.Move:
					OnUpdateMoveState();
				break;
					
				case State.Dead:
				break;
			}

			OnUpdateAnimationStates();
		}

		protected virtual void LateUpdate()
		{
			if(clientType == ClientType.LocalClient)
			{
				var p = position;
				p.y = initSpawnPosition.y;
				position = p;
			}
		}

		public virtual void OnTriggerEnter(Collider c)
		{
		}

		protected virtual void OnUpdateIdleState()
		{
		}

		protected virtual void OnUpdateMoveState()
		{
		}

		protected virtual void OnUpdateAnimationStates()
		{
			if(animator != null && animator.isActiveAndEnabled)
			{
				animator.SetFloat(animHashId.DirX, controlDirection.x);
				animator.SetFloat(animHashId.DirY, controlDirection.y);

				animator.SetFloat(animHashId.MouseDirY, controlDirection.z);

				animator.SetBool(animHashId.Running, running && directionState != RobotEmilViewObserver.Direction.Stand);
				animator.SetBool(animHashId.Walking, !running && directionState != RobotEmilViewObserver.Direction.Stand);
			}
		}

		void OnControllerColliderHit(ControllerColliderHit hit)
		{
			stepsSound.OnControllerColliderHit(hit);
		}

		#endregion

		#region Skin

		public virtual void SetSkin(Skin skin)
		{
			Debug.Log(name + "Set Skin " + skin);

			this.skin = skin;

			SkinContainer predefinedSkin = null;

			foreach(var s in predefinedSkins)
			{
				if(s != null && s.skin == skin)
				{
					predefinedSkin = s;
					break;
				}
					
			}
			if(predefinedSkin != null)
			{
				skinMaterial = lastSkinMaterial = new Material(predefinedSkin.material);

				if(skinMaterial != null)
				{
					if(defaultSkinShader == null)
						defaultSkinShader = skinMaterial.shader;

					skinMaterial.EnableKeyword("_EMISSION");

					SetMaterial(skinMaterial);
				}
			}

			Assert.IsAssigned(skinMaterial);
		}

		private void SetMaterial(Material mat)
		{
			foreach(var mr in skinRenderers)
			{
				if(mr != null)
					mr.sharedMaterial = mat;
			}
		}

		private bool electrovisionActive = false;
		public void SetElectovisionActive(bool active)
		{
			this.electrovisionActive = active;

			UpdateElectrovisionState(true);
		}

		private void UpdateElectrovisionState(bool forced)
		{
			if(electrovisionActive || forced)
			{
				bool active = electrovisionActive;

				if(state == State.Dead || state == State.Idle)
					active = false;

				if(skinMaterial == null)
				{
					Debug.LogError("skinMaterial == null");
					return;
				}

				skinMaterial.shader = active ? electrovisionShader : defaultSkinShader;
			}
		}

		#endregion

		#region Client settings

		protected virtual void OnSetClientTypeRemoteClient()
		{
			SetColliderTypeCC(false);
		}

		protected virtual void OnSetClientTypeBotClient()
		{
			SetColliderTypeCC(false);

			if(botClient != null)
				botClient.OnSetClientTypeBotClient();
		}

		protected virtual void OnSetClientTypeLocalClient()
		{
			SetColliderTypeCC(true);

			var lre = LocalClientRobotEmil.Instance;

			if(lre != null)
				lre.OnSetClientTypeLocalClient();
		}

		#endregion

		#region Spawning

		protected virtual void OnInstantiated()
		{
			
		}

		protected virtual void OnLoaded()
		{
			aoc.Register(this);

			if(clientType == ClientType.RemoteClient)
				Spawn();
		}

		protected virtual void SetColliderTypeCC(bool useCC)
		{
			if(!useCC)
			{
				Destroy(characterController);
				rigidCharacterCollider.SetActive(true);
			}
		}

		public virtual void Spawn(SpawnPoint spawn)
		{
			if(spawn == null)
				return;

			Spawn(spawn.position, spawn.localEulerAngles.y);
		}

		private Vector3 initSpawnPosition;

		public virtual void Spawn(Vector3 spawnPosition, float angleY)
		{
			positionRaw = initSpawnPosition = spawnPosition;

			Vector3 lea = localEulerAngles;
			lea.y = angleY;

			localEulerAngles = lea;

			Spawn();
		}

		protected virtual void Spawn()
		{
			#if UNITY_EDITOR
			Debug.Log("Spawn()");
			#endif

			OnSpawn();

			if(clientType != ClientType.RemoteClient)
				SetState(State.Idle, true);
			
			DeathAnimationActive(false);
			GhostUsed(false, 0f);

			ResetAnimatorLayerWeights();
		
			SetWeapon(0, true);
			SetGrenadesMasked(false);

			activityTimeFromLastSpawn = 0f;

			this.lives = this.initLives;

			SetViewType(RobotEmilViewObserver.Type.PlayerView);

			//freeBoxes = Config.boxDestroyableToolDefaultBoxes;

			if(clientType == ClientType.LocalClient)
			{
				foreach(var kvp in weaponConfig)
				{
					var wc = kvp.Value;

					if(wc != null)
						wc.Reset();
				}
			}

			if(clientType == ClientType.LocalClient)
				gameMessage.Flush();

			OnSpawned();
		}

		private bool firstSpawn = true;
		protected virtual void OnSpawn()
		{
			if(firstSpawn)
			{
				OnFirstSpawn();
				firstSpawn = false;
			}
		}

		protected virtual void OnSpawned()
		{
		}

		protected virtual void OnFirstSpawn()
		{
			
		}

		public virtual Vector3 positionRaw { get { return position; } set { position = value; } }

		public void SetListener()
		{
			AudioListener listener = rotatedContainer.GetComponent<AudioListener>();
			if(listener == null)
				listener = rotatedContainer.gameObject.AddComponent<AudioListener>();

			snd.SetListener(listener);
		}

		#endregion

		/*
		#region Box BuildTool

		public void OnBoxDestroyableBuildToolBoxCreated()
		{
			freeBoxes--;

			if(!canUseBoxDestroyableTool)
			{
				gameMessage.SetMessageLocalized("Bonus_BoxPack_Exceeded", GameMessage.MsgType.Warning);
			}
		}

		#endregion
		*/

		#region Weapons

		private float lastScrollTime = 0f;
		private int currWeaponIdx = 0;

		public void OnWeaponChanged(int d)
		{
			if(Time.time < lastScrollTime || state == State.Unspawned || state == State.Dead)
				return;

			lastScrollTime = Time.time + 0.2f;

			SwitchToNextWeapon(d);
		}

		public void FastWeaponSwitch()
		{
			SetWeapon(lastWeaponType);
		}

		public int PeekNextWeaponIdx(int d)
		{
			int cnt = Config.Weapons.maxEquipedWeapons-1;

			var bonusWeapon = equipedWeapons.GetWeaponAt(EquipedWeapons.BonusWeaponIdx);

			if(bonusWeapon != WeaponType.None)
				cnt++;

			int localCurrWeaponIdx = currWeaponIdx;

			if(d == 1)
			{
				if(localCurrWeaponIdx + 1 <= cnt)
					localCurrWeaponIdx++;
				else
					localCurrWeaponIdx = 0;
			}
			else if(d == -1)
			{
				if(localCurrWeaponIdx - 1 >= 0)
					localCurrWeaponIdx--;
				else
					localCurrWeaponIdx = cnt;
			}

			return localCurrWeaponIdx;
		}

		public void SwitchToNextWeapon(int d)
		{
			SetWeapon(PeekNextWeaponIdx(d), false);
		}

		public virtual void SetWeapon(int slotId, bool forced)
		{
			currWeaponIdx = slotId;
			SetWeapon(equipedWeapons.GetWeaponAt(slotId), forced);
		}

		public virtual void SetWeapon(WeaponType weaponType, bool forced = false)
		{
			if(weaponType == WeaponType.None)
			{
				Debug.LogError("Failed to set weaponType to None");
				return;
			}

			if(this.currWeaponType == weaponType && !forced)// || isAttacking)
				return;

			if((state == State.Unspawned || state == State.Dead) && !forced)
				return;
			
			//Debug.Log("SetWeapon " + weaponType);

			this.lastWeaponType = this.currWeaponType;
			this.currWeaponType = weaponType;

			if(forced)
				this.lastWeaponType = this.currWeaponType;

			ReloadWeapon(currWeaponType);
		}

		protected virtual void ReloadWeapon(WeaponType weaponType)
		{
			GrabNewWeapon();
		}

		public virtual void ReequipWeapons()
		{
			SetWeapon(currWeaponIdx, false);
		}


		[SerializeField]
		public WeaponType currWeaponType = WeaponType.GrenadeLauncherSticky; 

		[SerializeField]
		protected WeaponType lastWeaponType = WeaponType.GrenadeLauncherSticky; 

		protected IHandWeaponObject currWeapon;

		public float currWeaponWeightSpeedMultiplier
		{
			get
			{ 
				if(currWeapon == null)
					return 1f;

				return currWeapon.weightSpeedMultiplier;
			}
		}

		[SerializeField]
		public int numBonusGrenades = 0;

		public int mp_currWeaponUsedCount = 0;

		private Dictionary<WeaponType, Config.Weapons.WeaponConfig> weaponConfig { get { return Config.Weapons.weaponConfig; } }

		private HashSet<IHandWeaponObject> grabbedWeapons = new HashSet<IHandWeaponObject>();

		public virtual IHandWeaponObject GrabNewWeapon() { return GrabNewWeapon(currWeaponType); }

		public bool SetBonusGrenades(int diff)
		{
			/*
			var wc = currWeapon.weaponConfig;

			int maxBonusGrenadesCount = wc.maxProjectileCount - wc.initProjectileCount;

			var nbg = numBonusGrenades + diff;

			if(nbg > maxBonusGrenadesCount)
				return false;

			numBonusGrenades = Mathf.Clamp(nbg, 0, maxBonusGrenadesCount);
			*/

			numBonusGrenades += diff;

			#if UNITY_EDITOR
			Debug.Log("SetBonusGrenades " + numBonusGrenades);
			#endif

			return numBonusGrenades > 0;
		}

		public virtual IHandWeaponObject GrabNewWeapon(WeaponType weaponType)
		{
			FlushAllGrabbedWeapons();
			FlushCurrWeapon();

			isAttacking = false;

			currWeapon = weaponRecycler.GetPrefab<IHandWeaponObject>(weaponType);

			if(currWeapon == null)
			{
				OnUnableGrabNewWeapon(weaponType);
				return null;
			}

			grabbedWeapons.Add(currWeapon);

			currWeapon.GrabToHand(this, weaponGripBone);

			OnNewWeaponGrabbed(weaponType, currWeapon);

			return currWeapon;
		}

		protected virtual void OnNewWeaponGrabbed(WeaponType weaponType, IHandWeaponObject weapon)
		{
			UpdateGrenadeMasking();
		}

		protected virtual void OnUnableGrabNewWeapon(WeaponType weaponType)
		{
			Debug.LogError("Unable GrabNewWeapon " + weaponType);
		
		}

		public virtual void OnUpdateWeaponDepeletedProgress(float p)
		{
			
		}

		//

		private bool IsAttackAllowed(AttackType attackType)
		{
			if(attackType == AttackType.Primary)
				return true;

			return currWeapon != null && currWeapon.isSecondaryAttackAllowed;
		}

		public virtual bool PrepareForAttack(RobotEmil.AttackType attackType)
		{
			if(currWeapon == null || isAttacking || activityTimeFromLastSpawn < 0.25f || state == State.Unspawned || state == State.Dead || !IsAttackAllowed(attackType))
				return false;

			if(currWeapon == null)
				return false;
			
			currWeapon.PrepareForAttack(this, attackType);

			isAttacking = true;
			return true;
		}

		public virtual bool AttackBeingHeld(RobotEmil.AttackType attackType)
		{
			if(activityTimeFromLastSpawn < 0.25f || currWeapon == null || state == State.Unspawned || state == State.Dead || !IsAttackAllowed(attackType))
				return false;

			return true;
		}

		public virtual int Attack(AttackType attackType)
		{
			isAttacking = false;

			if(!IsAttackAllowed(attackType))
			{
				return -1;
			}

			if(activityTimeFromLastSpawn < 0.25f || currWeapon == null || state == State.Unspawned || state == State.Dead)
			{
				Debug.Log("activityTime " + activityTimeFromLastSpawn + ", state - " + state);
				return -1;
			}

			if(currWeapon == null)
			{
				//Debug.LogError("currWeapon null - " + currWeapon);
				return -1;
			}

			return OnStartAttack(attackType);
		}

		// toto je tady schvalně debile
		protected virtual int OnStartAttack(AttackType attackType, double timestamp = -1, int projectileHashId = -1)
		{
			if(currWeapon == null)
				return -1;

			if(currWeapon is IHandWeaponFiringProjectilesWithBarrel)
			{
				return (currWeapon as IHandWeaponFiringProjectilesWithBarrel).Attack(this, attackType, timestamp, projectileHashId);
			}
			else
			{
				return currWeapon.Attack(this, attackType, timestamp);
			}
		}

		public virtual void TriggerShootingAnimation()
		{
			animator.SetTrigger(animHashId.Shooting);
		}

		#region Knives only implementation

		public virtual void TriggerKnifeStabAnimation()
		{
			animator.ResetTrigger(animHashId.Stab);
			animator.SetTrigger(animHashId.Stab);
		}

		public virtual void TriggerTaserStabAnimation()
		{
			animator.ResetTrigger(animHashId.Stab);
			animator.SetTrigger(animHashId.Stab);
		}

		public void TriggerBaseballBatAttackAnimation()
		{
			animator.SetTrigger(animHashId.Attack);
		}


		#endregion

		#region IStabWeaponObject Callbacks 

		public virtual void OnActivateStabWeaponAnimation()
		{
			//var stabObject = currWeapon as IMeleeWeaponObject;

			//if(stabObject != null)
			//	stabObject.OnActivateStabWeaponAnimation(this);

		}

		public virtual void OnEndOfStabAnimation()
		{
			var stabObject = currWeapon as IMeleeWeaponObject;

			if(stabObject != null)
				stabObject.OnEndOfStabAnimation(this);

			isAttacking = false;
		}

		#endregion

		#region Grenades only implementation

		protected List<GrenadeBase> activeGrenades = new List<GrenadeBase>();


		public void FlushAllGrabbedWeapons(bool recycle = true)
		{
			if(recycle)
			{
				foreach(var gw in grabbedWeapons)
				{
					if(gw != null)
					{
						gw.OnRemoveFromHand(this);
						weaponRecycler.EnqueuePrefab(gw);
					}
				}
			}

			grabbedWeapons.Clear();
		}

		public void FlushCurrWeapon()
		{
			currWeapon = null;
		}

		#endregion

		#endregion

		#region State / Animations
		public void SetState(State state, bool forceSetState = false)
		{
			//if(isLocalClient)
			//	Debug.Log("localClient - SetState " + state);

			//if(!isLocalClient && this.state != state)
			//	Debug.Log(name + " SetState " + state);

			if(this.state == state)
				return;
			
			if(this.state == State.Dead && !forceSetState)
				return;

			if(clientType == ClientType.RemoteClient && !forceSetState)
				return;

			switch(state)
			{
				case State.Idle:

					//PlayAnimation(StandAnim);

				break;

				case State.Move:

				break;

				case State.Dead:
					
				break;
			}

			this.state = state;

			UpdateElectrovisionState(false);
		}

		#endregion

		#region Boxes

		public virtual void OnDestroyableBoxCreatedWithBoxCreator()
		{
			if(clientType == ClientType.LocalClient)
			{
				tutorial.HandleEvent(TutorialEvent.OnDestroyableBoxCreatedWithBoxCreator);
			}
		}

		public virtual void OnBoxDestroyed(Entities.BoxDestroyable box, IProjectileObjectWithExplosion projectile)
		{
			if(clientType == ClientType.LocalClient)
			{
				tutorial.HandleEvent(TutorialEvent.OnBoxDestroyed);
			}
		}

		public void OnStickableGrenadeStickedOnDestroyableBox(StickableGrenade stickableGrenade)
		{
			if(clientType == ClientType.LocalClient)
			{
				tutorial.HandleEvent(TutorialEvent.OnStickableGrenadeStickedOnDestroyableBox);
			}
		}

		public void OnBouncyGrenadeExploded(BouncyGrenade bouncyGrenade)
		{
			if(clientType == ClientType.LocalClient)
			{
				tutorial.HandleEvent(TutorialEvent.OnBouncyGrenadeExploded);
			}
		}

		public void OnHolyGrenadeExploded(HolyGrenade holyGrenade)
		{
			if(clientType == ClientType.LocalClient)
			{
				tutorial.HandleEvent(TutorialEvent.OnHolyGrenadeExploded);
			}
		}

		#endregion

		#region Movement

		public void Move(Vector3 motion)
		{
			if(freezed || state == State.Unspawned || characterController == null || !characterController.enabled)
				return;

			// LOck Y pozice

			//if(transform.position.z != zPosition)
			//{
			//	movementOffSet.z = (zPosition - transform.position.z) * 0.05f;
			//}

			motion.y += Physics.gravity.y;

			characterController.Move(motion * Time.deltaTime * speedMultiplier);
		}

		public void RotateOffset(float yOffset, float angleLerpSpeed)
		{
			var currRot = localEulerAngles;

			Vector3 finalRot = currRot;
			finalRot.y = Mathf.LerpAngle(currRot.y, currRot.y + yOffset, angleLerpSpeed);

			localEulerAngles = finalRot;
		}

		public void Rotate(Vector3 eulerAngles, float lerpSpeed)
		{
			localRotation = Quaternion.Slerp(localRotation, Quaternion.Euler(eulerAngles), lerpSpeed);
		}

		public void Rotate(Vector3 eulerAngles)
		{
			localEulerAngles = eulerAngles;
		}

		#endregion

		#region Hits

		protected virtual void OnLivesChanged(float val)
		{
		
		}

		protected virtual void OnLocalPlayerHit(IAttackerObject attacker, float percentualDamage, float damage)
		{
		}

		public virtual bool Hit(IAttackerObject attacker, float percentualDamage, float damage)
		{
			if(attacker == null || this == null || shieldUsed)
				return false;
			
			if(clientType != ClientType.RemoteClient)
			{
				OnLocalPlayerHit(attacker, percentualDamage, damage);	
				return true;
			}

			return false;
		}

		protected virtual bool Hit(RobotEmil attackerRobot, WeaponType attackerWeaponType, int attackerWeaponProjectileType, float percentualDamage, float damage)
		{
			if(shieldUsed)
				return false;
			
			Debug.Log("Hit " + attackerRobot + " - " + attackerWeaponType + " / " + attackerWeaponProjectileType + " / " + percentualDamage + " / " + damage);

			lives -= percentualDamage * damage;

			if(hitSound != null && state != State.Dead)
				hitSound.PlayRandom(transform);
			
			//Debug.Log(lives);

			if(lives <= 0f)
			{
				lives = 0f;

				if(state != State.Dead)
				{
					bool suicide = attackerRobot == this;

					if(attackerRobot != null)
						attackerRobot.OnEnemyDied(attackerWeaponType, attackerWeaponProjectileType, this, suicide);
					
					OnDeath(suicide);
				}
			}

			return true;
		}

		public virtual void HitByMeleeWeapon(IAttackerObject attacker, float percentualDamage, float damage)
		{
		}

		public virtual void OnEnemyDied(WeaponType attackerWeaponType, int attackerWeaponProjectileType, RobotEmil enemy, bool suicide)
		{
			
		}

		protected void PlayDeathAnimation()
		{
			if(ragdoll != null)
			{
				DeathAnimationActive(true);

				SetViewType(RobotEmilViewObserver.Type.DeathCam);

				Timer.DelayAsync(3f, OnDeathEnd);
			}
		}

		protected void DeathAnimationActive(bool active)
		{
			ragdoll.SetRagdollActive(active);

			if(rigidCharacterCollider != null && clientType != ClientType.LocalClient)
				rigidCharacterCollider.SetActive(!active);

			Debug.Log("DeathAnimationActive " + active);

			//NOTICE: nejde aniz bych se s tim nemusel ultramonstrozne srat, takze si vyliz prdel ty kundo zmrdana
			//characterController.detectCollisions = !active;
			//characterController.enabled = !active;
		}

		protected virtual void OnDeath(bool suicide)
		{
			if(state != State.Dead)
			{
				SetState(State.Dead, true);

				PlayDeathAnimation();

				if(currWeapon != null)
					currWeapon.OnRobotDeath();

				if(botClient != null)
					botClient.OnDeath(suicide);

				//if(deathSound != null)
				//	deathSound.Play(transform);

				//Debug.Log(name + " OnDeath");
			}
		}

		protected virtual void OnDeathEnd()
		{
			if(clientType == ClientType.LocalClient)
				gameMessage.Flush();
		}

		#endregion

		#region Bonus implementations

		private Bonuses.Active.RobotEmilPickedActiveBonusesStack _activeBonusStack = null;

		public Bonuses.Active.RobotEmilPickedActiveBonusesStack activeBonusStack
		{
			get
			{ 
				if(_activeBonusStack == null)
					_activeBonusStack = new Bonuses.Active.RobotEmilPickedActiveBonusesStack(this);

				return _activeBonusStack;
			}
		}

		public bool PickUpBonus(Bonus bonus, bool usedRemote)
		{
			if(bonus == null)
				return false;

			bool pickedUp = false;

			bonus.pickerRobot = this;

			Debug.Log("PickUpBonus " + bonus.behaviour);

			switch(bonus.type)
			{
				case Bonus.Type.Healing:
				case Bonus.Type.Passive:
				case Bonus.Type.XP:
				case Bonus.Type.Danger:
				case Bonus.Type.Special:
					
					pickedUp = UseBonus(bonus, usedRemote);

					if(pickedUp)
					{
						OnBonusPickedUp(bonus);
						OnPassiveBonusUsed(bonus);
					}
					else
					{
						OnPassiveBonusUseRefused(bonus);
					}

				break;
					
				case Bonus.Type.Active:

					pickedUp = activeBonusStack.PickBonus(bonus);

					if(pickedUp)
					{
						OnBonusPickedUp(bonus);
						OnActiveBonusPickedUp(bonus);
					}
					else
					{
						OnActiveBonusPickUpRefused(bonus);
					}

				break;
					
				default:
					Debug.LogError("invalid bonus type " + bonus.type + " to pick up");
				break;
			}

			return pickedUp;
		}

		public virtual bool UseBonus(Bonus bonus, bool usedRemote)
		{
			return false;
		}

		#region Pick Up

		protected virtual void OnBonusPickedUp(Bonus bonus)
		{
			
		}

		//

		protected virtual void OnActiveBonusPickedUp(Bonus bonus)
		{
		}

		protected virtual void OnActiveBonusPickUpRefused(Bonus bonus)
		{
			activeBonusStack.OnBonusPickupRefused(bonus);
		}

		//

		protected virtual void OnPassiveBonusUsed(Bonus bonus)
		{
			
		}

		protected virtual void OnPassiveBonusUseRefused(Bonus bonus)
		{
			if(clientType == RobotEmil.ClientType.LocalClient)
			{
				hud.OnLocalPlayerPasiveBonusPickUpRefused(bonus);
			}
		}

		#endregion

		public virtual void OnBonusUsageStarted(Bonus bonus)
		{
			
		}

		public virtual void OnBonusUsageComplete(Bonus bonus)
		{
			activeBonusStack.BonusUsageComplete(bonus.activeBonusStackItemId);
		}

		public virtual bool UseActiveBonus(int idx, bool usedRemote, double timestamp = -1)
		{
			return activeBonusStack.UseBonusStackItem(idx, usedRemote, timestamp);
		}

		#region Grenades
		private bool grenadesMasked = false;

		public void SetGrenadesMasked(bool masked)
		{
			grenadesMasked = masked;
			UpdateGrenadeMasking();
		}

		private void UpdateGrenadeMasking()
		{
			var grenadeLauncher = currWeapon as GrenadeLauncherBase;

			if(grenadeLauncher != null)
				grenadeLauncher.SetGrenadesMasked(grenadesMasked);
		}

		public void SetGrenadeDamageMultiplier_Bonus(float multiplier)
		{
			#if UNITY_EDITOR
			Debug.Log("SetGrenadeDamageMultiplier_Bonus " + multiplier);
			#endif

			this.grenadeDamageMultiplier_Bonus = multiplier;
		}


		public void SetGrenadeDamageMultiplier_MadnessMode(float multiplier)
		{
			#if UNITY_EDITOR
			Debug.Log("SetGrenadeDamageMultiplier_MadnessMode " + multiplier);
			#endif

			this.grenadeDamageMultiplier_MadnessMode = multiplier;
		}

		//

		public void SetGrenadeRadiusMultiplier_Bonus(float multiplier)
		{
			#if UNITY_EDITOR
			Debug.Log("SetGrenadeRadiusMultiplier_Bonus " + multiplier);
			#endif

			this.grenadeRadiusMultiplier_Bonus = multiplier;
		}


		public void SetGrenadeDamageRadiusMultiplier_MadnessMode(float multiplier)
		{
			#if UNITY_EDITOR
			Debug.Log("SetGrenadeDamageRadiusMultiplier_MadnessMode " + multiplier);
			#endif

			this.grenadeRadiusMultiplier_MadnessMode = multiplier;
		}


		#endregion


		#region GlobalSlowdown
		public virtual void GlobalSlowdownUsed(bool used, bool usedRemote)
		{
			//Debug.Log("GlobalSlowdownUsed used:" + used + ", usedRemote: " + usedRemote);

			if(usedRemote)
			{
				if(used)
				{
					Ease.Instance.Alpha(0f, 0.1f, 0.5f, EaseType.In, (d) => 
					{
						if(radialBlur != null)
							radialBlur.density = d;
					});

					SetSpeedMultiplier(Config.Bonuses.GlobalSlowdownBonus_Min);
				}
				else
				{
					Ease.Instance.Alpha(0.1f, 0f, 0.5f, EaseType.Out, (d) => 
					{
						if(radialBlur != null)
							radialBlur.density = d;
					}, () => 
					{
						SetSpeedMultiplier(Config.Bonuses.GlobalSlowdownBonus_Max);
					});
				}
			}
		}
		#endregion

		#region Ghost

		public virtual void GhostUsed(bool used, float ghostSpeedMultiplier)
		{
			this.ghostBonusUsed = used;

			if(skinMaterial == null)
			{
				Debug.LogError("SkinMaterial null");
				return;
			}

			SetMaterial(used ? ghostMaterial : lastSkinMaterial);

			SetSpeedMultiplier(speedMultiplier + ghostSpeedMultiplier);
		}

		public virtual void SetMeleeDamageMultiplier(float meleeDamageMultiplier)
		{
			this.meleeDamageMultiplier = Mathf.Clamp(meleeDamageMultiplier, 1f, 6f); 
		}

		#endregion

		#region Magnet
		public virtual void MagnetUsed(bool used)
		{
			freezed = used;

			if(used)
				magnet.Show();
			else
				magnet.Hide();
		}
		#endregion

		#region Shield
		protected bool shieldUsed;
		public virtual void ShieldUsed(bool used)
		{
			shieldUsed = used;

			if(used)
				shield.Show();
			else
				shield.Hide();
		}
		#endregion

		#region Speed
		public virtual void SetSpeedMultiplier(float multiplier)
		{
			if(speedMultiplier == multiplier)
				return;

			//TODO: tady by to chcelo clamp

			speedMultiplier = multiplier;

			if(animator != null)
				animator.speed = speedMultiplier;
		}
		#endregion

		#region Flash

		public virtual void SetIsFlashed(bool flashed)
		{
			this.isFlashed = flashed;
		}

		#endregion

		#region SecondaryAttackType + GunUpgrade + HolyExplosions

		public SecondaryAttackType secondaryAttackType { get; private set; }

		public virtual void AddSecondaryAttackType(SecondaryAttackType attackType)
		{
			secondaryAttackType = secondaryAttackType.Add(attackType);

			#if UNITY_EDITOR
			Debug.Log("AddSecondaryAttackType " + secondaryAttackType);
			#endif

		}

		public virtual void RemoveSecondaryAttackType(SecondaryAttackType attackType)
		{
			secondaryAttackType = secondaryAttackType.Remove(attackType);

			#if UNITY_EDITOR
			Debug.Log("RemoveSecondaryAttackType " + secondaryAttackType);
			#endif
		}

		#endregion

		#region Quick explodes

		public float grenadeExplosionSpeedUp { get; private set; }

		public void SetGrenadeExposionSpeedUp(float explosionSpeedUp)
		{
			#if UNITY_EDITOR
			Debug.Log("SetGrenadeExposionSpeedUp " + explosionSpeedUp);
			#endif

			this.grenadeExplosionSpeedUp = explosionSpeedUp;
		}

		#endregion

		#endregion


		#region View

		protected virtual void SetViewType(RobotEmilViewObserver.Type viewType)
		{
			viewObserver.SetViewType(viewType);

			SetRagdollRotatedWithCamera(viewType == RobotEmilViewObserver.Type.PlayerView);
		}

		protected virtual void SetRagdollRotatedWithCamera(bool rotate)
		{
			ragdoll.SetParent(rotate ? rotatedContainer : notRotatedContainer);

			if(rotate)
				ragdoll.localRotation = Quaternion.identity;
		}

		#endregion

		#region Physics

		public void IgnoreCollision(Collider c1, bool ignore)
		{
			//Physics.IgnoreCollision(characterControllerCollider, c1, ignore);
		}

		#endregion
	}
}
