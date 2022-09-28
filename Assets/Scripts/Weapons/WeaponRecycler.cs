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
	public class WeaponRecycler : WeaponRecyclerBase<WeaponType, IHandWeaponObject>
	{
		public new static WeaponRecycler Instance { get { return MonoSingleton<WeaponRecycler>.GetInstance() as WeaponRecycler; } }
		public new static WeaponRecycler GetInstance() { return MonoSingleton<WeaponRecycler>.GetInstance() as WeaponRecycler; }


		[Serializable]
		public class SerializedItemContainer
		{
			public WeaponType type;

			public IHandWeaponObject reference;
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
