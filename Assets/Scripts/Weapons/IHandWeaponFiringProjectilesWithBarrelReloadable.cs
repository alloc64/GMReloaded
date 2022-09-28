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
	public abstract class IHandWeaponFiringProjectilesWithBarrelReloadable : IHandWeaponFiringProjectilesWithBarrel
	{
		protected virtual float reloadTime  { get { var c = weaponConfig; return c == null ? 0f : c.reloadTime; } }

		[SerializeField]
		protected float reloadTimer = 0f;

		protected virtual float reloadTimeMultiplier { get { return 1f; } }


		#region Unity

		protected override void Update()
		{
			base.Update();

			UpdateReloadTimer();
		}

		#endregion

		protected virtual void UpdateReloadTimer()
		{
			if(reloadTimer < reloadTime)
			{
				reloadTimer += Time.deltaTime * reloadTimeMultiplier;
			}
		}

		protected override void UpdateWeaponDepeleted()
		{
			UpdateWeaponDepeletedProgress(reloadTimer / reloadTime);
		}

	}
}