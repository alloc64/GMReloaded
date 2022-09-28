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
using System.Linq;

namespace GMReloaded.Entities
{
	public class BoxDestroyable : DestroyableEntity
	{
		private const string bonusPrefabsPath = "Prefabs/Bonuses/";

		public class NetworkProperties : EntityContainer.EntityNetworkProperties
		{
			public State state;
			public Vector3 burnPoint;

			//

			private char[] _header = new char[]{ 'x', 'B', 'D' };
			public override char[] header { get { return _header; } }

			#region ISerializableStruct implementation

			public override void OnSerializeStruct(System.IO.BinaryWriter bw)
			{
				base.OnSerializeStruct(bw);

				bw.Write((byte)state);

				bw.Write(burnPoint);
			}

			public override bool OnDeserializeStruct(System.IO.BinaryReader br)
			{
				if(!base.OnDeserializeStruct(br))
					return false;

				state = (State)br.ReadByte();

				burnPoint = br.ReadVector3();

				return true;
			}

			#endregion

			public override string ToString()
			{
				return string.Format("[NetworkProperties] " + entityId + " - " + state);
			}
		}

		public override byte[] networkProperties
		{
			get
			{ 
				NetworkProperties np = new NetworkProperties();

				np.entityId = entityId;
				np.state = state;
				np.burnPoint = burnPoint;

				return StructSerializer.Serialize<BoxDestroyable.NetworkProperties>(np);
			}
		}

		//

		[SerializeField]
		private Color createBoxFireGrad1 = new Color(0.0f, 1f, 0.56470588235294f, 1.0f);

		[SerializeField]
		private Color createBoxFireGrad2 = new Color(0.0f, 0.8f, 1.0f, 1.0f);

		//

		[SerializeField]
		private Color destroyBoxFireGrad1 = new Color32(214, 102, 0, 255);

		[SerializeField]
		private Color _destroyBoxFireGrad2 = new Color32(253, 183, 0, 255);

		//

		[SerializeField]
		private float burnTimerInit = 0.15f;

		//

		[SerializeField]
		private float burnSpeedMultiplier = 1f;

		private float burnTimer;

		//

		private bool isDemolishEnabled = true;

		//
	
		private Material material;

		public Bounds bounds { get { return collider == null ? new Bounds() : collider.bounds; } }

		[SerializeField]
		private new MeshRenderer renderer;

		[SerializeField]
		private Shader burnShader;

		//unity si mysli, ze ty materialy nepouzivam, tak tady na ne vytvorim referenci
		[SerializeField]
		private Material burnMaterialPreserver;

		private Vector3 burnPoint;

		protected override void Awake()
		{
			material = renderer.material;

			base.Awake();
		}

		#region IRecyclablePrefab

		public override void Reinstantiate()
		{
			burnPoint = Vector3.zero;

			base.Reinstantiate();
		}

		#endregion

		protected override void Spawn()
		{
			base.Spawn();

			this.burnTimer = burnTimerInit;

			renderer.material = material;
			SetActive(true);
		}

		public override void Setup(EntityContainer entityContainer, int entityId)
		{
			base.Setup(entityContainer, entityId);
		}

		#region Burn Effect

		private void Demolish(IAttackerObject attacker, Vector3 point)
		{
			//Debug.Log(name + "Demolish " + attacker, transform);

			if(state != State.Idle || (attacker.ParentRobot != null && attacker.ParentRobot.clientType == RobotEmil.ClientType.RemoteClient) || !isDemolishEnabled)
				return;

			// tady zalezi na poradi
			SetBurnPoint(point);
			Demolish();

			IProjectileObjectWithExplosion projectile = attacker as IProjectileObjectWithExplosion;

			if(projectile != null)
				projectile.OnBoxDestroyed(this);
		}

		private Coroutine reverseBurnCoroutineInstance = null;

		public void ReverseBurn()
		{
			reverseBurnCoroutineInstance = StartCoroutine(ReverseBurnCoroutine());
		}

		private void SetBurnPoint(Vector3 burnPoint)
		{
			var dir = burnPoint - position;
			float d = dir.magnitude;

			///Debug.Log(name + " - set burn point " + d, this);

			float maxD = 2.8f;

			if(d > maxD)
			{
				burnPoint = position + dir.normalized * (maxD * 0.8f);
			}

			this.burnPoint = burnPoint;
		}

		private IEnumerator ReverseBurnCoroutine()
		{
			renderer.material = burnMaterialPreserver;

			var m = renderer.material;

			var bp = position;

			m.SetVector("_BurnPoint", new Vector4(bp.x, bp.y, bp.z));

			//m.SetColor("_FireGrad1", new Color(0.0f, 1f, 0.88235294117647f, 1.0f));
			//m.SetColor("_FireGrad2", new Color(0.0f, 0.8f, 1.0f, 1.0f));

			m.SetColor("_FireGrad1", createBoxFireGrad1);
			m.SetColor("_FireGrad2", createBoxFireGrad2);


			float t = 0.4f;

			while(t < 1f)
			{
				t += Time.deltaTime * burnSpeedMultiplier * 1.5f;

				if(t < 1f)
				{
					m.SetFloat("_BurnFactor", 1f - t);
				}

				if(t >= 1f)
				{
					renderer.material = material;
				}

				yield return null;
			}

			reverseBurnCoroutineInstance = null;
		}

		private IEnumerator ForwardBurnCoroutine(bool updateFromNet)
		{
			if(reverseBurnCoroutineInstance != null)
			{
				StopCoroutine(reverseBurnCoroutineInstance);
				reverseBurnCoroutineInstance = null;

				yield return null;
			}

			renderer.material = burnMaterialPreserver;

			DispatchOnBeingRuinedEvent();

			var m = renderer.material;

			if(burnPoint == Vector3.zero)
				burnPoint = position;

			m.SetVector("_BurnPoint", new Vector4(burnPoint.x, burnPoint.y, burnPoint.z));

			m.SetColor("_FireGrad1", destroyBoxFireGrad1);
			m.SetColor("_FireGrad2", _destroyBoxFireGrad2);

			bool grenadeEmitted = false;

			while(state == State.Ruining)
			{
				burnTimer += Time.deltaTime * burnSpeedMultiplier;

				if(burnTimer >= 0.5f && !grenadeEmitted)
				{
					ThrowGrenades();
					grenadeEmitted = true;
				}

				if(burnTimer < 1f)
				{
					m.SetFloat("_BurnFactor", burnTimer);
				}

				if(burnTimer >= 1f)
				{
					SetState(State.Ruined);

					burnTimer = burnTimerInit;
				}

				yield return null;
			}

			DispatchOnBeingRuinedEvent();
		}

		#endregion

		#region State Set

		protected override void OnIdleStateSet()
		{
			base.OnIdleStateSet();
		}

		protected override void OnRuiningStateSet(bool updateFromNet)
		{
			base.OnRuiningStateSet(updateFromNet);

			StartCoroutine(ForwardBurnCoroutine(updateFromNet));
		}

		protected override void OnRuinedStateSet()
		{
			base.OnRuinedStateSet();
		}

		#endregion

		public override bool Hit(IAttackerObject attacker, float percentualDamage, Vector3 point)
		{
			bool ret = base.Hit(attacker, percentualDamage, point);

			if(percentualDamage > 0f)
			{
				Demolish(attacker, point);
				return true;
			}

			return ret;
		}

		#region Network

		protected override void RefreshNetworkState()
		{
			NetworkProperties np = new NetworkProperties();

			np.state = state;
			np.burnPoint = burnPoint;

			RefreshNetworkState(np);
		}

		public override void HandleNetworkData(byte[] data)
		{
			NetworkProperties p = StructSerializer.Deserialize<NetworkProperties>(data);

			SetBurnPoint(p.burnPoint);
			SetState(p.state, true);
		}

		#endregion

		#region Madness Mods

		private void ThrowGrenades()
		{
			if(arenaEventDispatcher == null || arenaEventDispatcher.madnessMode == null)
				return;

			int useCount = arenaEventDispatcher.madnessMode.explosiveCratesUseCount;

			for(int i = 0; i < useCount; i++)
			{
				ThrowGrenade(i);
			}
		}

		private void ThrowGrenade(int idx)
		{
			Vector3[] directions = new Vector3[] 
			{
				new Vector3(-1f, 0f, 0f),
				new Vector3(0f, 1f, 0f),
				new Vector3(1f, 0f, 0f),
				new Vector3(0f, -1f, 0f),
			};

			Vector3[] offsets = new Vector3[] 
			{
				new Vector3(-1f, 0f, 0f),
				new Vector3(1f, 0f, 0f),
				new Vector3(-1f, 0f, 0f),
				new Vector3(1f, 0f, 0f),
			};

			if(idx >= directions.Length)
			{
				Debug.LogError("Failed to throw grenade, too much grenades for one DestroyableBox");
				return;
			}

			var projectileType = ProjectileType.StickyGrenade;
			var weaponType = WeaponType.GrenadeLauncherBouncy;

			Config.Weapons.WeaponConfig weaponConfig = null;

			Config.Weapons.weaponConfig.TryGetValue(weaponType, out weaponConfig);

			if(weaponConfig == null)
			{
				Debug.LogError("Failed to select weapon config for weaponType " + weaponType);
				return;
			}

			var currGrenade = ProjectileRecycler.Instance.GetPrefab<GrenadeBase>(projectileType);

			if(currGrenade == null)
			{
				Debug.LogWarning("BoxDestroyable :: ThrowBouncyGrenade unable get grenade ");
			}
			else
			{

				Vector3 fireForce = directions[idx] * 3.1f;

				currGrenade.SetOnPosition(transform);

				Vector3 lp = currGrenade.localPosition;
				lp = offsets[idx] * 0.2f;
				currGrenade.localPosition = lp;

				currGrenade.SetMasked(false);
				currGrenade.Fire(weaponConfig, fireForce, null, 1f, 1f, 0f, -1f);
			}
		}

		#endregion

		#region Other

		public void SetDestroyable(bool destroyable)
		{
			if(!tutorial.isActive)
				return;

			Debug.Log("SetDestroyable " + destroyable);

			this.isDemolishEnabled = destroyable;
		}

		protected override void OnDestroy()
		{
			material = null;

			base.OnDestroy();
		}

		#endregion
	}
}
