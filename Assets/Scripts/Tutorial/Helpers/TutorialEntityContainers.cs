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
using GMReloaded.Entities;

namespace GMReloaded.Tutorial.Helpers
{
	public class TutorialEntityContainers : MonoBehaviourTO
	{
		public List<EntityContainer> entityContainers = new List<EntityContainer>();	

		//

		public EntityContainer GetEntityContainer(string entityId)
		{
			foreach(var ec in entityContainers)
			{
				if(ec != null && ec.name.Equals(entityId))
					return ec;
			}

			return null;
		}

		public void DemolishEntities()
		{
			if(entityContainers == null)
				return;

			foreach(var ec in entityContainers)
			{
				if(ec != null)
				{
					ec.Demolish();
				}
			}
		}

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("TouchOrchestra/Grenade Madness/Tutorial/TutorialEntityContainers - Load EntityContainers")]
		private static void DestroyDestroyableEntitiesBoxes()
		{
			var tecs = Object.FindObjectsOfType<TutorialEntityContainers>();

			foreach(var t in tecs)
			{
				t.entityContainers.AddRange(Object.FindObjectsOfType<EntityContainer>());
				UnityEditor.EditorUtility.SetDirty(t);
			}
		}
		#endif
	}
}