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
using System;
using System.Collections.Generic;

using GMReloaded.Entities;
using System.Linq;

namespace GMReloaded.Entities
{
	[Flags]
	public enum ObjectTypes
	{
		Explosive = 1,
		Alive = 2,
		BoxDestroyable = 4,
		ExplosiveBarrel = 8,
		DestroyableEntity = BoxDestroyable | ExplosiveBarrel
	}

	public class EntityController : MonoSingleton<EntityController> 
	{
		private Dictionary<short, EntityContainer> entityContainerInstances = new Dictionary<short, EntityContainer>();

		/// <summary>
		/// Controller všech aktivních objektů který můžou vybouchout
		/// </summary>
		protected IExplosiveObjectsController eoc { get { return IExplosiveObjectsController.Instance; } }

		/// <summary>
		/// Controller všech aktivních objektů který můžou dostat hit
		/// </summary>
		protected IAliveObjectsController aoc { get { return IAliveObjectsController.Instance; } }

		protected DestroyableEntityController bdc { get { return DestroyableEntityController.Instance; } }

		#region Entities

		public void RegisterEntityContainer(short entityContainerId, EntityContainer container)
		{
			entityContainerInstances[entityContainerId] = container;
		}

		public EntityContainer GetEntityContainer(short entityId)
		{
			EntityContainer ec = null;
			entityContainerInstances.TryGetValue(entityId, out ec);

			return ec;
		}

		public void HandleNetworkData(object value)
		{
			EntityContainer.NetworkProperties prop = StructSerializer.Deserialize<EntityContainer.NetworkProperties>((byte[])value);

			if(prop.entityId < 0 || prop.entityId >= entityContainerInstances.Count)
				return;

			EntityContainer ec = GetEntityContainer(prop.entityId);

			if(ec != null)
				ec.HandleNetworkData(prop);
		}

		public void SpawnExplosiveBarrels()
		{
			if(!PhotonNetwork.isMasterClient)
				return;

			bdc.BurnAllDestroyableBoxes();

			Timer.DelayAsyncIndependent(1f, _SpawnExplosiveBarrelsWorker);
		}

		private void _SpawnExplosiveBarrelsWorker()
		{
			if(!PhotonNetwork.isMasterClient || this == null)
				return;

			foreach(var kvp in entityContainerInstances)
			{
				var entityContainer = kvp.Value;

				if(entityContainer != null && entityContainer.entityType == EntityType.BoxDestroyable)
				{
					entityContainer.Create(EntityType.ExplosiveBarrel);
				}
			}
		}

		#endregion

		public void GetVisibleObjectsInDistance(Vector3 position, float radius, ObjectTypes objTypes, ref List<IObjectWithPosition> objects)
		{ 
			if((objTypes & ObjectTypes.Explosive) != 0)
				Debug.LogWarning("Explosive Not implemented");
				//eoc.GetObjectsInDistance(position, radius, objects);

			if((objTypes & ObjectTypes.Alive) != 0)
				Debug.LogWarning("Alive Not implemented");
				//aoc.GetObjectsInDistance(position, radius, objects);

			if((objTypes & ObjectTypes.DestroyableEntity) != 0)
				bdc.GetVisibleObjectsInDistance(position, radius, ref objects);
			
		}

		public void HitObjectsInRadius(IAttackerObject parent, float radius, float damage, ObjectTypes objTypes = ObjectTypes.Explosive | ObjectTypes.Alive | ObjectTypes.BoxDestroyable | ObjectTypes.ExplosiveBarrel)
		{
			if((objTypes & ObjectTypes.Explosive) != 0)
				eoc.HitObjectsInRadius(parent, radius, damage);

			if((objTypes & ObjectTypes.Alive) != 0)
				aoc.HitObjectsInRadius(parent, radius, damage);

			if((objTypes & ObjectTypes.DestroyableEntity) != 0)
				bdc.HitObjectsInRadius(parent, radius, damage);
		}
	}
}
