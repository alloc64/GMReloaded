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
	public abstract class DestroyableEntity : IRecyclablePrefab<DestroyableEntity>, IObjectWithPosition
	{
		public enum State : byte
		{
			Idle,
			Ruining,
			Ruined,
		}

		public abstract byte[] networkProperties { get; } 

		private Action _OnBeingRuinedEvent = null;
		public Action OnBeingRuinedEvent { get { return _OnBeingRuinedEvent; } set { _OnBeingRuinedEvent = value; } }

		[SerializeField]
		public State state;

		[SerializeField]
		public State lastState;

		public bool isDestroyed { get { return state != State.Idle; } }

		[SerializeField]
		public short entityId;

		protected EntityType ?_entityType;
		public EntityType entityType
		{
			get
			{
				if(!_entityType.HasValue)
					_entityType = GetDestroyableEntityType(this);

				return _entityType.Value;
			}
		}

		[SerializeField]
		protected EntityContainer entityContainer;

		[SerializeField]
		protected new Collider collider;

		[SerializeField]
		private MeshFilter[] indicatorMeshes;

		//

		protected DestroyableEntityController dec { get { return DestroyableEntityController.Instance; } }

		protected ArenaEventDispatcher arenaEventDispatcher { get { return ArenaEventDispatcher.Instance; } }

		//

		#region Unity

		protected virtual void Awake()
		{
			Spawn();
		}

		#endregion

		#region IRecyclablePrefab

		public override void Reinstantiate()
		{
			SetActive(true);
			SetState(State.Idle);

			Spawn();
		}

		private void Recycle()
		{
			if(entityContainer != null)
				entityContainer.RecycleEntity(this);
			
			entityContainer = null;
		}

		#endregion

		#region State Set

		public void SetState(State state, bool updateFromNet = false, bool ignoreDupliciteStates = true)
		{
			if(state == this.state && ignoreDupliciteStates)
				return;

			this.lastState = this.state;

			this.state = state;

			switch(state)
			{
				case State.Idle:

					if(lastState != State.Idle)
					{
						OnIdleStateSet();
					}

				break;

				case State.Ruining:
					
					OnRuiningStateSet(updateFromNet);

				break;

				case State.Ruined:


					OnRuinedStateSet();

				break;
			}
		}

		protected virtual void OnIdleStateSet()
		{
			Spawn();
		}

		protected virtual void OnRuiningStateSet(bool updateFromNet)
		{
			SetColliderDisabled(true);
		}

		protected virtual void OnRuinedStateSet()
		{
			Recycle();
		}

		#endregion


		public virtual void Setup(EntityContainer entityContainer, int entityId)
		{
			this.entityContainer = entityContainer;
			this.entityId = Convert.ToInt16(entityId);

			this.name = GetType().FullName + " - " + entityId;

			parent = entityContainer.transform;
			localRotation = Quaternion.identity;
			localPosition = Vector3.zero;
			localScale = Vector3.one;
		}

		protected virtual void Spawn()
		{
			SetColliderDisabled(false);
		}

		// vola se vzdy kdyz chci znicit DestroyableEntity
		public virtual void Demolish()
		{
			if(state != State.Idle)
				return;

			SetState(State.Ruining);

			RefreshNetworkState();
		}

		public virtual bool Hit(IAttackerObject attacker, float percentualDamage, Vector3 point)
		{
			return false;
		}
			
		#region Network

		protected abstract void RefreshNetworkState();

		protected virtual void RefreshNetworkState(EntityContainer.EntityNetworkProperties np)
		{
			if(entityContainer != null)
				entityContainer.RefreshNetworkState(StructSerializer.Serialize<EntityContainer.EntityNetworkProperties>(np));
		}

		public abstract void HandleNetworkData(byte[] data);

		#endregion


		#region Indicator

		public DestroyableEntityIndicator MakeIndicator()
		{
			GameObject go = new GameObject(name + " - Indicator");

			var indicator = go.AddComponent<DestroyableEntityIndicator>();

			indicator.Process(transform);

			Destroy(gameObject);

			return indicator;
		}

		#endregion

		protected void DispatchOnBeingRuinedEvent()
		{
			if(OnBeingRuinedEvent != null)
				OnBeingRuinedEvent();
		}

		private void SetColliderDisabled(bool disabled)
		{
			collider.enabled = !disabled;
			SetLayerRecursively(disabled ? Layer.EntityContainerEmpty : Layer.DestroyableEntity);
		}

		public static EntityType GetDestroyableEntityType(DestroyableEntity entity)
		{
			EntityType entityType = EntityType.None;

			if(entity is BoxDestroyable)
				entityType = EntityType.BoxDestroyable;
			else if(entity is ExplosiveBarrel)
				entityType = EntityType.ExplosiveBarrel;

			return entityType;
		}
	}
}