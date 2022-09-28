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

namespace GMReloaded.Entities
{
	public class DestroyableEntityCreatorDisplay : MonoBehaviourTO
	{
		[System.Serializable]
		public class DisplayMat
		{
			[SerializeField]
			public EntityType entityType;

			[SerializeField]
			public Material material;
		}

		[SerializeField]
		private DisplayMat[] materials;

		[SerializeField]
		private MeshRenderer displayMeshRenderer;

		public void SetEntityType(EntityType entityType)
		{
			Material entityMaterial = null;

			foreach(var dm in materials)
			{
				if(dm != null && dm.entityType == entityType)
				{
					entityMaterial = dm.material;
					break;
				}
			}

			if(entityMaterial == null)
			{
				Debug.LogError("Failed to set " + entityType + " in BoxCreatorDisplay - entityMaterial == null");
				return;
			}

			if(displayMeshRenderer != null)
				displayMeshRenderer.material = entityMaterial;
		}
	}
}
