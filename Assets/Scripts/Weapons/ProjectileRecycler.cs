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

namespace GMReloaded
{	
	public class ProjectileRecycler : WeaponRecyclerBase<ProjectileType, IProjectileObject>
	{
		public new static ProjectileRecycler Instance { get { return MonoSingleton<ProjectileRecycler>.GetInstance() as ProjectileRecycler; } }
		public new static ProjectileRecycler GetInstance() { return MonoSingleton<ProjectileRecycler>.GetInstance() as ProjectileRecycler; }


		[Serializable]
		public class SerializedItemContainer
		{
			public ProjectileType type;

			public IProjectileObject reference;
		}

		[SerializeField]
		private SerializedItemContainer [] items;

		private void Awake()
		{
			foreach(SerializedItemContainer i in items)
			{
				if(i != null)
				{
					AddPrefab(i.type, i.reference);
				}
			}
		}
	}
	
}
