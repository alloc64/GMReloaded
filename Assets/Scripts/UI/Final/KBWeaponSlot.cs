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

namespace GMReloaded.UI.Final.Equip
{
	public class KBWeaponSlot : KBFocusableGUIItemWithStateChange
	{
		public enum Order
		{
			Primary,
			Secondary,
			Third
		}

		[SerializeField]
		public Order order;

		[SerializeField]
		public WeaponSubType weaponSubType;

		//
		private WeaponType weaponType;

		[SerializeField]
		private tk2dBaseSprite iconSprite; 

		[SerializeField]
		private tk2dTextMesh clickToChangeText; 

		#region Unity

		protected override void OnEnable()
		{
			base.OnEnable();

			SetWeaponType(Config.Weapons.localClientEquipedWeapons.GetWeaponAt((int)order));
		}

		#endregion

		public override void SetFocused(bool active)
		{
			base.SetFocused(active);

			if(clickToChangeText != null)
				clickToChangeText.SetActive(active);
		}

		public void SetWeaponType(WeaponType weaponType)
		{
			this.name = weaponType.ToString();

			this.weaponType = weaponType;

			if(iconSprite != null)
				iconSprite.SetSpriteByID(weaponType.ToString());
		}

		public void SetWeaponInfo(tk2dTextMesh slotTitle, tk2dTextMesh slotText)
		{
			string key = weaponType.ToString();

			if(slotTitle != null)
				slotTitle.text = localization.GetValue(key);

			if(slotText != null)
				slotText.text = localization.GetValue(key + "_Desc");
		}

		//

		public void Equip(KBWeaponItem weaponItem)
		{
			if(weaponItem == null)
				return;

			var weaponType = weaponItem.weaponType;

			SetWeaponType(weaponType);

			Config.Weapons.localClientEquipedWeapons.SetWeaponAtSlot((int)order, weaponType);
		}
	}
}