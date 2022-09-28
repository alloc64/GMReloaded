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
using System;

namespace GMReloaded.Entities
{
	public class ExplosiveBarrel : DestroyableEntity, IExplosiveObject, IRagdollExplosiveInfluencer
	{
		[SerializeField]
		private float _damageRadius = 1f;
		public float damageRadius { get { return _damageRadius; } } // pouziva se i jako Explosion Radius

		public float explosionRadius { get { return explosionRadiusMultiplier * damageRadius; } } 

		protected float explosionRadiusMultiplier = 1f;

		//

		[SerializeField]
		private float explosionDamage = 20f;

		// ragdoll

		[SerializeField]
		private float _explosionForceRadius = 1f;
		public float explosionForceRadius { get { return _explosionForceRadius; } }

		[SerializeField]
		private float _explosionForce = 100f;
		public float explosionForce { get { return _explosionForce; } }

		//

		[SerializeField]
		private GameObject barrelModel;

		[SerializeField]
		private ParticleSystem fireParticles;

		[SerializeField]
		private ParticleSystem explosionParticles;

		private SoundContainer explosionSound;

		private UnityEngine.Coroutine explodeCoroutine;

		private IAttackerObject lastAttacker;

		#region Network Properties

		public class NetworkProperties : EntityContainer.EntityNetworkProperties
		{
			public State state;

			private char[] _header = new char[]{ 'x', 'E', 'B' };
			public override char[] header { get { return _header; } }

			#region ISerializableStruct implementation

			public override void OnSerializeStruct(System.IO.BinaryWriter bw)
			{
				base.OnSerializeStruct(bw);

				bw.Write((byte)state);
			}

			public override bool OnDeserializeStruct(System.IO.BinaryReader br)
			{
				if(!base.OnDeserializeStruct(br))
					return false; // vadna msg

				state = (State)br.ReadByte();

				return true;
			}

			#endregion
		}

		public override byte[] networkProperties
		{
			get
			{ 
				NetworkProperties np = new NetworkProperties();

				np.entityId = entityId;
				np.state = state;

				return StructSerializer.Serialize<NetworkProperties>(np);
			}
		}

		#endregion

		protected EntityController hitController { get { return EntityController.GetInstance(); }}

		#region Not Implemented

		public RobotEmil ParentRobot { get { return lastAttacker == null ? null : lastAttacker.ParentRobot; } }

		public WeaponType WeaponType { get { return WeaponType.ExplosiveBarrel; } }

		public int ProjectileType { get {  return -1; } }

		#endregion

		#region Unity

		protected override void Awake()
		{
			base.Awake();

			explosionSound = new SoundContainer("SNDC_Barrel_Explosion");

			for(int i = 0; i < 4; i++)
				explosionSound.AddSound(Config.Sounds.grenadeExplosion + i);
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(position, damageRadius);
		}

		#endregion


		protected override void Spawn()
		{
			base.Spawn();

			SetModelActive(true);

			explodeCoroutine = null;
			lastAttacker = null;

			explosionRadiusMultiplier = 1f;
		}

		#region State Set

		protected override void OnIdleStateSet()
		{
			base.OnIdleStateSet();

		}

		protected override void OnRuiningStateSet(bool updateFromNet)
		{
			base.OnRuiningStateSet(updateFromNet);

			Explode();

			if(fireParticles != null)
				fireParticles.Play();
		}

		protected override void OnRuinedStateSet()
		{
			base.OnRuinedStateSet();

		}

		#endregion


		#region Network

		protected override void RefreshNetworkState()
		{
			NetworkProperties np = new NetworkProperties();

			np.state = state;

			RefreshNetworkState(np);
		}

		public override void HandleNetworkData(byte[] data)
		{
			NetworkProperties p = StructSerializer.Deserialize<NetworkProperties>(data);

			SetState(p.state, true);
		}

		#endregion

		#region Explosion

		public override bool Hit(IAttackerObject attacker, float percentualDamage, Vector3 point)
		{
			bool ret = base.Hit(attacker, percentualDamage, point);

			if(percentualDamage > 0f)
			{
				lastAttacker = attacker;

				Demolish();
				return true;
			}

			return ret;
		}

		public bool Hit(IAttackerObject attacker, float percentualDamage, float damage)
		{
			throw new NotImplementedException();
		}

		public void Explode()
		{
			if(explodeCoroutine == null)
			{
				explodeCoroutine = StartCoroutine(ExplodeCoroutine());
			}
			else
			{
				// force explode
				explodeTimer = explosionTime;
			}
		}

		private float explodeTimer = 0f;
		private float explosionTime = 2f;

		private IEnumerator ExplodeCoroutine()
		{
			while(explodeTimer < explosionTime)
			{
				explodeTimer += Time.deltaTime;
				yield return null;
			}

			SetModelActive(false);

			if(explosionParticles != null)
				explosionParticles.Play();

			if(explosionSound != null)
				explosionSound.Play(transform);

			hitController.HitObjectsInRadius(this, explosionRadius, explosionDamage);

			explodeTimer = 0f;

			DispatchOnBeingRuinedEvent();

			if(explosionParticles != null)
			{
				float t = 0;

				float explosionParticlesTime = explosionParticles.duration + explosionParticles.startLifetime;

				while(t < explosionParticlesTime)
				{
					t += Time.deltaTime;
					yield return null;
				}
			}

			DispatchOnBeingRuinedEvent();

			SetState(State.Ruined);
		}

		public void SetExplosionRadiusMultiplier(float radiusMultiplier)
		{
			this.explosionRadiusMultiplier = Mathf.Clamp(radiusMultiplier, 1f, 2f);
		}

		private void SetModelActive(bool active)
		{
			if(barrelModel != null)
				barrelModel.SetActive(active);
		}

		#endregion
	}
}
