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
using GMReloaded.Bonuses;

namespace GMReloaded.Entities
{
	public class EntityContainer : MonoBehaviourTO
	{
		public const string prefix = "EC_";

		private string _propsId = null;
		public string propsId 
		{ 
			get 
			{ 
				if(_propsId == null)
					_propsId = prefix + entityId;

				return _propsId;  
			} 
		}

		public abstract class EntityNetworkProperties : ISerializableStruct
		{
			public EntityNetworkProperties() {}

			public abstract char[] header { get; }

			public short entityId;

			#region ISerializableStruct implementation

			public virtual void OnSerializeStruct(System.IO.BinaryWriter bw)
			{
				bw.Write(header);
				bw.Write(entityId);
			}

			public virtual bool OnDeserializeStruct(System.IO.BinaryReader br)
			{
				char[] msgHeader = br.ReadChars(3);

				if(!msgHeader.SequenceEqual(header))
				{
					Debug.LogWarning("Received invalid message, with header " + msgHeader + " ignoring....");
					return false;
				}

				entityId = br.ReadInt16();

				return true;
			}

			#endregion
		}

		public class NetworkProperties : EntityNetworkProperties
		{
			public EntityType entityType;
			public byte[] entityData = new byte[0];

			public Bonus.Behaviour bonusBehaviour; // obsujuje EntityContainerBonus
			public BonusPickable.State bonusState; // obsujuje EntityContainerBonus

			//

			private char[] _header = new char[]{ 'x', 'E', 'C' };
			public override char[] header { get { return _header; } }

			public override void OnSerializeStruct(System.IO.BinaryWriter bw)
			{
				base.OnSerializeStruct(bw);

				bw.Write((byte)entityType);

				bw.Write(System.Convert.ToUInt16(entityData.Length));
				bw.Write(entityData);

				//

				bw.Write((int)bonusBehaviour);
				bw.Write((byte)bonusState);
			}

			public override bool OnDeserializeStruct(System.IO.BinaryReader br)
			{
				if(!base.OnDeserializeStruct(br))
					return false;

				entityType = (EntityType)br.ReadByte();

				ushort entityDataLength = br.ReadUInt16();
				entityData = br.ReadBytes(entityDataLength);

				//

				bonusBehaviour = (Bonus.Behaviour)br.ReadInt32();
				bonusState = (BonusPickable.State)br.ReadByte();

				return true;
			}
		}

		[SerializeField]
		public short entityId;

		public bool isOccupied { get { return entityType != EntityType.None; } }

		[SerializeField]
		public EntityType entityType;

		[SerializeField]
		private new BoxCollider collider;

		//

		private EntityContainerBonus _bonusImpl;
		public EntityContainerBonus bonusImpl
		{
			get
			{
				if(_bonusImpl == null)
					_bonusImpl = new EntityContainerBonus(this);
				
				return _bonusImpl;
			}
		}

		//

		public Bounds bounds { get { return collider == null ? new Bounds() : collider.bounds; } }

		//

		private ArenaEventDispatcher aed { get { return ArenaEventDispatcher.Instance; } }

		private DestroyableEntityController dec { get { return DestroyableEntityController.Instance; }}

		private EntityController entityController { get { return EntityController.Instance; }}

		private RoomPropertiesController rpc { get { return RoomPropertiesController.Instance; } }

		//

		#region Unity

		private void Awake()
		{
			entityController.RegisterEntityContainer(entityId, this);

			//

			SetEntityType(entityType, true, true);
		}

		private void OnDrawGizmos()
		{
			if(collider != null)
			{
				switch(entityType)
				{
					case EntityType.None:
						Gizmos.color = Color.red;
					break;
						
					case EntityType.BoxDestroyable:
						Gizmos.color = Color.yellow;
					break;
						
					case EntityType.ExplosiveBarrel:
						Gizmos.color = Color.blue;
					break;
				}


				Gizmos.matrix = transform.localToWorldMatrix;
				Gizmos.DrawWireCube(collider.center, collider.extents * 2f);


				if(bonusImpl.canBonusBePickedUp)
				{
					Gizmos.color = Color.green;

					Gizmos.DrawWireSphere(collider.center + new Vector3(0f, 0f, 1f), 0.5f);
				}
			}
		}

		#endregion


		#region DestroyableEntity

		public void Create(EntityType entityType)
		{
			SetEntityType(entityType, false, false, true);
		}

		public void Demolish()
		{
			var destroyableEntity = FindChildDestroyableEntity();

			if(destroyableEntity != null)
			{
				destroyableEntity.Demolish();
			}
		}

		public void RecycleEntity(DestroyableEntity destroyableEntity)
		{
			SetEntityType(EntityType.None, false);
		}

		//

		/// <summary>
		/// Vygeneruje bonus, volá se pouze na master klientu
		/// </summary>
		/// <param name="childEntity">Child entity.</param>
		private bool GenerateBonus(DestroyableEntity childEntity)
		{
			if(tutorial.isActive && !tutorial.randomGenerateBonuses)
				return false;

			if(childEntity is BoxDestroyable && PhotonNetwork.isMasterClient) // ignorujeme vsechno krome krabic
			{
				return bonusImpl.GenerateBonus();
			}

			return false;
		}

		public bool GenerateBonus_Tutorial(Bonus.Behaviour bonusBehaviour)
		{
			if(bonusImpl == null || !tutorial.isActive)
				return false;

			bool ret = bonusImpl.GenerateBonus(bonusBehaviour);

			if(ret)
				RefreshNetworkState();

			return ret;
		}

		//

		public void PickUpBonus(BonusPickable.State pickupType, bool ignoreNetworkUpdate = false)
		{
			if(bonusImpl == null)
				return;

			bonusImpl.PickUpBonus(pickupType, ignoreNetworkUpdate);
		}

		private void RecycleBonus()
		{
			if(bonusImpl == null)
				return;

			bonusImpl.RecycleBonus();
		}

		#endregion

		#region Entity Management

		private void SetEntityType(EntityType entityType, bool ignoreNetworkUpdate, bool initialSet = false, bool showEffect = false)
		{
			if(this.entityType == entityType && !initialSet)
				return;

			this.entityType = entityType;

			gameObject.layer = (entityType == EntityType.None ? Layer.EntityContainerEmpty : Layer.EntityContainerOccupied);

			if(entityType != EntityType.None)
			{
				// mrknu se, jestli entita uz neexistuje

				var childEntity = FindChildDestroyableEntity();

				if(childEntity != null && childEntity.entityType != entityType)
				{
					dec.RecycleDestroyableEntity(childEntity.entityType, childEntity);
					childEntity = null;
				}

				if(childEntity == null)
				{
					// entita neexistuje, vytvorim ji

					childEntity = dec.DequeueDestroyableEntity(entityType);

					if(childEntity != null)
					{
						Debug.Log("Created entity in EntityContainer " + name + " / " + childEntity, childEntity);
					}

				}

				// podarilo se vytvorit entitu, registruji si ji + recykluju bonus pokud uz existuje
				if(childEntity != null)
				{					
					RecycleBonus();

					childEntity.Setup(this, entityId);

					if(childEntity is BoxDestroyable && showEffect)
					{
						var box = childEntity as BoxDestroyable;

						if(box != null)
							box.ReverseBurn();
						//
					}

					dec.RegisterEntity(entityId, childEntity);
				}
			}
			else
			{
				// podarilo se znicit entitu

				var childEntity = FindChildDestroyableEntity();

				if(childEntity != null)
				{
					bool bonusGenerated = GenerateBonus(childEntity);

					if(bonusGenerated && ignoreNetworkUpdate) // pokud se vygeneruje bonus, udelam dispatch 
						ignoreNetworkUpdate = false;

					if(PhotonNetwork.isMasterClient && !tutorial.isActive)
						StartEntityRespawn();

					dec.RecycleDestroyableEntity(childEntity.entityType, childEntity);
				}
			}

			if(!ignoreNetworkUpdate)
			{
				RefreshNetworkState();
			}
		}

		public DestroyableEntity FindChildDestroyableEntity()
		{
			return GetComponentInChildren<DestroyableEntity>();
		}

		#endregion

		#region Network

		public void RefreshNetworkState(byte [] destroyableEntityData = null)
		{
			if(!PhotonNetwork.connectedAndReady)
				return;

			NetworkProperties np = new NetworkProperties();
			np.entityId = entityId;
			np.entityType = entityType;

			if(destroyableEntityData != null)
			{
				np.entityData = destroyableEntityData;
			}

			bonusImpl.RefreshNetworkState(np);

			//Debug.Log("Refreshing state of entityContainer " + name + " / " + entityType);

			rpc.Set(propsId, StructSerializer.Serialize<NetworkProperties>(np));
		}

		public void HandleNetworkData(NetworkProperties np)
		{
			SetEntityType(np.entityType, true, false, true);

			if(np.entityData != null && np.entityData.Length > 0)
			{
				var childEntity = FindChildDestroyableEntity();

				if(childEntity != null)
					childEntity.HandleNetworkData(np.entityData);
			}

			bonusImpl.HandleNetworkData(np);
		}

		#endregion

		#region Auto respawn

		//Tato funkce se vola jenom na master clientovi
		private void StartEntityRespawn()
		{
			if(!PhotonNetwork.isMasterClient || entityType != EntityType.None)
				return;

			if(aed != null && aed.madnessMode != null && aed.madnessMode.isStopCratesAutoRespawnActive)
				return;

			StartCoroutine(RespawnCoroutine());
		}

		//Tato funkce se vola jenom na master clientovi
		private IEnumerator RespawnCoroutine()
		{
			float respawnTime = Config.entityRespawnTime;

			Madness.Craaatesss_MadnessModeImpl cratesMMEvent = aed.madnessMode.GetMadnessStepDispatcher<Madness.Craaatesss_MadnessModeImpl>(MadnessStepType.Craaatesss);

			bool hasCraaatesss = cratesMMEvent != null && cratesMMEvent.isActive;


			float t = 0f;
			while(t < (hasCraaatesss ? (respawnTime * cratesMMEvent.respawnRatio) : respawnTime))
			{
				if(aed.madnessMode.isStopCratesAutoRespawnActive)
					yield break;

				t += Time.deltaTime;
				yield return null;
			}

			//

			float barrelProbabilty = Config.explosiveBarrelSpawnProbability;
			if(hasCraaatesss)
			{
				barrelProbabilty = cratesMMEvent.barrelProbabilty;
			}

			if(entityType == EntityType.None)
			{
				if(IsEntityContainerOccupiedByPlayer(1.5f))
				{
					StartEntityRespawn();
				}
				else
				{
					// postavim entitu
					EntityType et = (Random.Range(0f, 100f) < barrelProbabilty * 100f) ? EntityType.ExplosiveBarrel : EntityType.BoxDestroyable;

					SetEntityType(et, false, false, true);
				}
			}
		}

		public bool IsEntityContainerOccupiedByPlayer(float distance)
		{
			foreach(var kvp in PlayersController.Instance.Objects)
			{
				var p = kvp.Value;
				if(p != null)
				{
					float dp = Vector3.Distance(p.position, position);

					if(dp < 1.5f)
						return true;
				}
			}

			return false;
		}

		#endregion

		//
		// - editor
		//


		#if UNITY_EDITOR

		[UnityEditor.MenuItem("TouchOrchestra/Grenade Madness/Setup EntityContainers")]
		private static void SetupEntityContainers()
		{
			EntityContainer [] entityContainers = Object.FindObjectsOfType<EntityContainer>();	

			short entityId = 0;

			foreach(var ec in entityContainers)
			{
				if(ec != null)
				{
					ec.CreateDefaultBoxCollider();

					ec.AssignEntityId(entityId);
					entityId++;
				}
			}
		}

		[UnityEditor.MenuItem("TouchOrchestra/Grenade Madness/Select duplicite EntityContainers")]
		private static void SelectDupliciteEntityContainers()
		{
			EntityContainer [] entityContainer = Object.FindObjectsOfType<EntityContainer>();	

			List<GameObject> selected = new List<GameObject>();
			foreach(var ec0 in entityContainer)
			{
				if(ec0 != null)
				{
					foreach(var ec1 in entityContainer)
					{
						if(ec1 != null && ec0 != ec1)
						{
							float d = Vector3.Distance(ec0.position, ec1.position);
							Debug.Log(d);
						
							if(d < 1f && !selected.Contains(ec1.gameObject))
								selected.Add(ec1.gameObject);
						}
					}
				}
			}

			UnityEditor.Selection.objects = selected.ToArray();
		}

		public void AssignEntityId(short entityId)
		{
			this.entityId = entityId;
		}

		public void CreateDefaultBoxCollider()
		{
			collider = GetComponent<BoxCollider>();

			if(collider == null)
				collider = gameObject.AddComponent<BoxCollider>();

			Bounds localContainerBounds = new Bounds(new Vector3(0f, -0.01000037f, -0.12f), new Vector3(1.08f, 1.08f, 2.12f));

			collider.center = localContainerBounds.center;
			collider.extents = localContainerBounds.extents;
			collider.isTrigger = true;

			var childEntity = FindChildDestroyableEntity();

			SetEntityType(DestroyableEntity.GetDestroyableEntityType(childEntity), true, true); // tady je zbytecny aby se updatoval net - editor
		}

		public void DestroyChildEntity()
		{
			var childEntity = FindChildDestroyableEntity();

			if(childEntity != null)
				DestroyImmediate(childEntity.gameObject);
		}

		#endif
	}
}
