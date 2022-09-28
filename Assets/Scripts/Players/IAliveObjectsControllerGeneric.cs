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
	public class IAliveObjectsControllerGeneric<U, W> : GenericObjectContainerMonoSingleton<U, W> 
		where W : class
		where U : Component
	{
		public virtual void HitObjectsInRadius(IAttackerObject parent, float radius, float damage)
		{
			for(int i = 0; i < Objects.Count; i++)
			{
				var obj = Objects[i];
				IAliveObject ao = obj as IAliveObject;

				if(ao != null && ao != parent)
				{
					Vector3 thisPos = parent.position;
					Vector3 objPos = ao.position;

					RaycastHit hit;

					if(!Physics.Linecast(thisPos, objPos, out hit, ((1 << Layer.DestroyableEntity) | (1 << Layer.Default))))
					{
						float d = Vector3.Distance(thisPos, objPos);
						float r = radius + ao.damageRadius;

						if(d <= r)
						{
							float percentualDamage = 1f - Mathf.Clamp(d / r, 0f, r);
							if(percentualDamage > 0f)
							{
								ao.Hit(parent, percentualDamage, damage);
							}
						}
					}
				}
			}
		}
	}
	
}
