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

	public abstract class IHandWeaponFiringProjectilesWithBarrelReloadableSequientially : IHandWeaponFiringProjectilesWithBarrelReloadable
	{
		protected override bool canGrabNewProjectile
		{
			get
			{
				if(reloadTimer < reloadTime)
					return false;

				return base.canGrabNewProjectile;
			}
		}

		public override void Reinstantiate()
		{
			base.Reinstantiate();

			reloadTimer = reloadTime;
		}

		#region Proejctile events

		protected override void OnProjectileFired(IObjectWithPosition projectile)
		{
			base.OnProjectileFired(projectile);

			reloadTimer = 0f;
		}

		public override void OnProjectileExplode(IObjectWithPosition projectile)
		{
			base.OnProjectileExplode(projectile);

			if(robotParent != null && robotParent.clientType == RobotEmil.ClientType.LocalClient)
			{
				var cfg = weaponConfig;

				if(cfg.usedCount > 0)
				{
					// odectu projektil po explozi a pridam ho do pouzitelnejch projektilu
					cfg.usedCount--;
				}
			}
		}

		#endregion
	}
	
}