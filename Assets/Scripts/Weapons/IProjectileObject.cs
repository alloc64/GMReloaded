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
	public abstract class IProjectileObject : IRecyclablePrefab<IProjectileObject>, ISerializableStruct
	{
		protected new Rigidbody rigidbody;
		
		protected ObjectTypes hitFlags = ObjectTypes.Explosive | ObjectTypes.Alive | ObjectTypes.BoxDestroyable | ObjectTypes.ExplosiveBarrel;

		protected int assignedId;

		private Transform parentInitial;

		protected ArenaEventDispatcher arenaEventDispatcher { get { return ArenaEventDispatcher.Instance; } }

		public ProjectileType getRecyclerType 
		{
			get 
			{ 
				ProjectileType grenadeType;
				ProjectileRecycler.GetPrefabWeaponType(this, out grenadeType);

				return grenadeType;
			}
		}

		public override Vector3 position
		{
			get
			{
				if(this != null && rigidbody != null)
					return rigidbody.position;

				return Vector3.zero;
			}

			set
			{ 
				if(this != null && rigidbody != null)
					rigidbody.position = value;
			}
		}

		protected virtual void Awake()
		{
			rigidbody = GetComponent<Rigidbody>();
		}

		public override void Reinstantiate()
		{
			SetActive(true);
		}

		public virtual void SetOnPosition(Transform parent)
		{
			parentInitial = this.parent;
			this.parent = parent;

			localPosition = Vector3.zero;
			localRotation = Quaternion.identity;
			localScale = Vector3.one;
		}

		protected void ResetParent()
		{
			parent = parentInitial;
		}

		#region Network IDs

		public void GenerateHashId()
		{
			this.assignedId = Mathf.Abs(GetHashCode());
		}

		public void AssignHashId(int assignedId)
		{
			this.assignedId = assignedId;
		}

		public int GetAssignedHashId()
		{
			return this.assignedId;
		}

		#endregion

		#region Network Data Serialization

		public virtual void OnSerializeStruct(System.IO.BinaryWriter bw)
		{
		}

		public virtual bool OnDeserializeStruct(System.IO.BinaryReader br)
		{
			return true;
		}

		#endregion
	}
	
}
