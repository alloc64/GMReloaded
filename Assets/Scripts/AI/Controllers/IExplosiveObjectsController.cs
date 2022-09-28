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

namespace GMReloaded
{
	public class IExplosiveObjectsController : IAliveObjectsControllerGeneric<IExplosiveObjectsController, IExplosiveObject> 
	{
		public bool IsSeenByHolyGrenade(RobotEmil robotEmil)
		{
			if(robotEmil == null)
				return false;

			for(int i = 0; i < Objects.Count; i++)
			{
				var obj = Objects[i];

				if(obj is HolyGrenade)
				{
					HolyGrenade grenade = (HolyGrenade)obj;

					if(grenade != null)
					{
						RaycastHit hit;
						if(!Physics.Linecast(robotEmil.position, grenade.position, out hit, ((1 << Layer.DestroyableEntity) | (1 << Layer.Default))))
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		//

		public void ExplodeAll()
		{
			ExplodeAllOf<IExplosiveObject>();
		}

		public void ExplodeAllOf<T>() where T : IExplosiveObject
		{
			List<IExplosiveObject> toExplode = new List<IExplosiveObject>();
			for(int i = 0; i < Objects.Count; i++)
				toExplode.Add(Objects[i]);
			// kolekce se modifikuje uz v loopu, proto ji musim klonnout

			foreach(var obj in toExplode)
			{
				if(obj is T)
				{
					T t = (T)obj;

					if(t != null)
					{
						t.SetExplosionRadiusMultiplier(2f);
						t.Explode();
					}
				}
			}
		}
	}
}
