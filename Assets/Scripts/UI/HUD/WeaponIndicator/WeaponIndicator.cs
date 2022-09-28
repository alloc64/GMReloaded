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

namespace GMReloaded
{
	public class WeaponIndicator : MonoBehaviourTO 
	{
		[SerializeField]
		private tk2dSprite weaponIcon;

		[SerializeField]
		private tk2dTextMesh weaponTitle;

		//

		[SerializeField]
		private tk2dTextMesh primaryProjectilesCount;

		[SerializeField]
		private tk2dTextMesh secondaryProjectilesCount;

		//

		public void SetWeapon(WeaponType weaponType)
		{
			weaponIcon.SetSpriteByID(weaponType.ToString(), "GrenadeLauncher");
			weaponIcon.SetActive(true);

			weaponTitle.text = localization.GetValue(weaponType.ToString());

			ResetProjectilesCount();
		}

		//

		public void SetWeaponProjectilesCount(RobotEmil.AttackType attackType, int usedCount, int maxProjectileCount)
		{
			SetProjectilesCountText(attackType, "(" + usedCount + "/" + maxProjectileCount +")");
		}

		private void ResetProjectilesCount()
		{
			SetProjectilesCountText(RobotEmil.AttackType.Primary, "");
			SetProjectilesCountText(RobotEmil.AttackType.Secondary, "");

		}

		private void SetProjectilesCountText(RobotEmil.AttackType attackType, string text)
		{
			switch(attackType)
			{
				case RobotEmil.AttackType.Primary:
					if(primaryProjectilesCount != null)
						primaryProjectilesCount.text = text;
				break;
					
				case RobotEmil.AttackType.Secondary:
					if(secondaryProjectilesCount != null)
						secondaryProjectilesCount.text = text;
				break;
			}

		}
	}
}
