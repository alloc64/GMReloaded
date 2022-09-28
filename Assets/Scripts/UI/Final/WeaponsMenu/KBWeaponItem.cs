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
using System.Linq;

namespace GMReloaded.UI.Final.Equip
{
	public class KBWeaponItem : KBFocusableGUIItemWithStateChange
	{
		[SerializeField]
		private tk2dBaseSprite iconSprite;

		[SerializeField]
		private tk2dBaseSprite lockSprite;

		[SerializeField]
		private tk2dTextMesh equippedText;

		[SerializeField]
		private new BoxCollider collider;

		public Vector3 size { get { return collider == null ? Vector3.zero : collider.size; } }

		// Properties

		public WeaponType weaponType;

		public float damage { get; private set; }
		public int weight { get; private set; }
		public float reloadTime { get; private set; }
		public float ammo { get; private set; }

		public int unlockLevel { get; private set; }

		public bool unlocked { get; private set; }

		public bool equipped { get { return Config.Weapons.localClientEquipedWeapons.HasEquippedWeapon(weaponType); } }

		#region Setup

		public override void Reinstantiate()
		{
			SetActive(true);
		}

		public void SetWeaponType(WeaponType weaponType)
		{
			this.weaponType = weaponType;

			this.name = weaponType.ToString();

			if(iconSprite != null)
				iconSprite.SetSpriteByID(weaponType.ToString());
		}

		public bool SetUnlockLevel(int level)
		{
			this.unlockLevel = level;

			unlocked = LocalClientRobotEmil.level >= unlockLevel;

			if(lockSprite != null)
				lockSprite.SetActive(!unlocked);
			
			if(equippedText != null)
				equippedText.SetActive(equipped);

			SetTransparent(!unlocked || equipped);

			return unlocked;
		}

		public void SetXPosition(float xOffset)
		{
			var lp = Vector3.zero;
			lp.x = xOffset;
			localPosition = lp;
		}

		private void SetTransparent(bool transparent)
		{
			if(iconSprite != null)
				iconSprite.SetAlpha(transparent ? 0.4f : 1f);
		}

		#endregion

		protected override void OnHover(bool over)
		{
			//base.OnHover(over);
		}

		#region Properties

		public void SetDamage(float damage)
		{
			this.damage = damage;
		}

		public void SetWeight(int weight)
		{
			this.weight = weight;
		}

		public void SetReloadTime(float reloadTime)
		{
			this.reloadTime = reloadTime;
		}

		public void SetAmmo(float ammo)
		{
			this.ammo = ammo;
		}

		#endregion
	}
	
}
