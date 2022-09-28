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
using System.Collections.Generic;
using System;
using GMReloaded.Entities;

namespace GMReloaded
{	
	public abstract class IProjectileObjectWithExplosion : IProjectileObject, IExplosiveObject, IRagdollExplosiveInfluencer
	{
		public enum State : byte
		{
			Uninitialized,
			Idle,
			Triggered,
			Exploded
		}

		public State state;

		[SerializeField]
		protected ParticleSystem explosion;

		//

		private Config.Weapons.WeaponConfig weaponConfigPreset;

		//

		protected virtual float explosionRadius
		{
			get
			{
				Config.Weapons.WeaponConfig c = null;

				if(weaponConfigPreset != null)
					c = weaponConfigPreset;

				if(parentWeapon == null && c == null)
					return 0f;

				if(parentWeapon != null)
					c = parentWeapon.weaponConfig;

				if(c == null)
					return 0f;

				return explosionRadiusMultiplier * c.damageRadius;
			}
		}

		protected float explosionRadiusMultiplier = 1f;

		//

		protected virtual float explosionDamage
		{
			get
			{
				Config.Weapons.WeaponConfig c = null;

				if(weaponConfigPreset != null)
					c = weaponConfigPreset;

				if(parentWeapon == null && c == null)
					return 0f;
				
				if(parentWeapon != null)
					c = parentWeapon.weaponConfig;

				if(c == null)
					return 0f;

				return c.damage;
			}
		}

		//

		public virtual float damageRadius { get { return explosionRadius; } }

		[SerializeField]
		protected GameObject objectModel;

		[SerializeField]
		protected new Collider collider;

		protected IHandWeaponFiringProjectilesWithBarrel parentWeapon;

		protected RobotEmil parentRobot;

		//

		public RobotEmil ParentRobot { get { return parentRobot; } }

		public WeaponType WeaponType 
		{
			get 
			{ 
				if(parentWeapon == null)
				{
					Debug.Log("parentWeapon not set");
					return EnumsExtensions.UnknownEnum<WeaponType>();
				}

				return parentWeapon.weaponType; 
			} 
		}

		public int ProjectileType 
		{ 
			get 
			{  
				if(parentWeapon == null)
					return -1;

				return (int)parentWeapon.projectileType; 
			} 
		}

		//

		private ISound explosionSound;

		#region IRagdollExplosiveInfluencer implementation

		[SerializeField]
		private float _explosionForceRadius = 3f;
		public float explosionForceRadius { get { return damageRadius * _explosionForceRadius; } }

		[SerializeField]
		private float _explosionForce = 1000f;
		public float explosionForce { get { return _explosionForce; } }

		public Vector3 hitForce { get { return Vector3.one; } }

		#endregion

		#region Singleton Getters

		protected EntityController entityController { get { return EntityController.GetInstance(); }}
		protected IExplosiveObjectsController eoc { get { return IExplosiveObjectsController.GetInstance(); }}

		#endregion

		public override void Reinstantiate()
		{
			base.Reinstantiate();

			weaponConfigPreset = null;

			SetKinematic(true);
			SetCollisionsActive(true);
			SetModelActive(true);

			explosionRadiusMultiplier = 1f;
		}

		public override void SetOnPosition(Transform parent)
		{
			base.SetOnPosition(parent);

			SetState(State.Idle);
			SetKinematic(true);
		}


		public void SetState(State state)
		{
			if(this.state == state)
				return;

			this.state = state;

			switch(state)
			{
				case State.Uninitialized:
				break;

				case State.Idle:
				break;

				case State.Triggered:
					OnSetStateTriggered();
				break;

				case State.Exploded:
					OnSetStateExploded();
				break;
			}
		}

		protected virtual void OnSetStateTriggered()
		{
			SetModelActive(true);
		}

		protected virtual void OnSetStateExploded()
		{
			PlayExplodeAnimation();
			PlayExplosionSound();

			SetCollisionsActive(false);
			SetModelActive(false);
			SetKinematic(true);

			if(assignedId > 0)
				arenaEventDispatcher.ExplodeSyncedProjectile(this);
		}

		public virtual void Fire(IHandWeaponFiringProjectilesWithBarrel weapon, RobotEmil parentRobot)
		{
			this.parentWeapon = weapon;
			this.parentRobot = parentRobot;
		}

		/// <summary>
		/// Vypali granat, pouziva se pouze zridka, mimo implementaci robota. Napevno nastavuje weapon config, ten se po recyklaci nulluje, parentWeapon a parentRobot jsou null
		/// </summary>
		/// <param name="weaponConfig">Weapon config.</param>
		public virtual void Fire(Config.Weapons.WeaponConfig weaponConfig)
		{
			this.weaponConfigPreset = weaponConfig;
		}

		#region IExplosiveObject implementation

		public virtual void Explode()
		{
			if(state != State.Triggered)
				return;

			SetState(State.Exploded);
		}

		protected virtual void PlayExplodeAnimation()
		{
			explosion.Play(OnExploded);

			OnExplodeEventCaller();
		}

		public virtual void OnExploded()
		{
			if(parentWeapon != null)
			{
				parentWeapon.OnProjectileExploded(this);
			}

			// recycle
			parentRecycler.Enqueue(this);

			if(explosion != null)
				explosion.Stop();
		}

		#endregion

		#region IAliveObject implementation

		public virtual bool Hit(IAttackerObject attacker, float percentualDamage, float damage) 
		{
			// not used
			return true;
		}

		#endregion

		#region Network Data Serialization

		public override bool OnDeserializeStruct(System.IO.BinaryReader br)
		{
			float explosionRadiusMultiplier = br.ReadSingle();

			if(explosionRadiusMultiplier >= 1f)
				SetExplosionRadiusMultiplier(explosionRadiusMultiplier);

			return base.OnDeserializeStruct(br);
		}

		public override void OnSerializeStruct(System.IO.BinaryWriter bw)
		{
			bw.Write(explosionRadiusMultiplier);

			base.OnSerializeStruct(bw);
		}

		#endregion

		protected virtual void PlayExplosionSound()
		{
			if(explosionSound != null)
			{
				SoundContainer explosionSoundSC = explosionSound as SoundContainer;

				if(explosionSoundSC != null)
					explosionSoundSC.PlayRandom(transform);
				else 
					explosionSound.Play(transform);
			}

		}

		protected virtual void OnExplodeEventCaller()
		{
			if(parentWeapon != null)
			{
				parentWeapon.OnProjectileExplode(this);
			}
		}

		protected void SetExplosionSound(string id)
		{
			explosionSound = snd.Load(id);
		}

		protected void SetExplosionSound(ISound sound)
		{
			explosionSound = sound;
		}

		protected void SetKinematic(bool kinematic)
		{
			rigidbody.isKinematic = kinematic;

			if(!rigidbody.isKinematic)
				rigidbody.velocity = Vector3.zero;
		}

		protected virtual void SetModelActive(bool active)
		{
			if(objectModel != null)
				objectModel.SetActive(active);
		}

		private void SetCollisionsActive(bool active)
		{
			collider.enabled = active;
		}

		public void SetExplosionRadiusMultiplier(float radiusMultiplier)
		{
			//#if UNITY_EDITOR
			//Debug.Log("SetExplosionRadiusMultiplier " + radiusMultiplier);
			//#endif

			this.explosionRadiusMultiplier = Mathf.Clamp(radiusMultiplier, 1f, 2f);
		}

		public void OnBoxDestroyed(Entities.BoxDestroyable box)
		{
			if(box != null && parentRobot != null)
			{
				parentRobot.OnBoxDestroyed(box, this);
			}
		}
	}
}
