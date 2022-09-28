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
using GMReloaded;
using System;
using System.IO;

namespace GMReloaded
{
	public class EquipedWeapons
	{
		private const string equipedWeaponsKey = "EquipedWeapons";

		private WeaponType[] weapons;

		//

		public const int BonusWeaponIdx = 3;
		private WeaponType bonusWeapon = WeaponType.None;

		//

		private bool usePlayerPrefs;

		//

		public EquipedWeapons(bool usePlayerPrefs = true)
		{
			this.usePlayerPrefs = usePlayerPrefs;

			Load();
		}

		//

		public WeaponType GetWeaponAt(int currWeaponIdx)
		{
			if(currWeaponIdx == BonusWeaponIdx)
				return bonusWeapon;
			
			if(currWeaponIdx >= 0 && currWeaponIdx <= Config.Weapons.maxEquipedWeapons)
				return weapons[currWeaponIdx];

			return WeaponType.None;
		}

		public void SetWeaponAtSlot(int currWeaponIdx, WeaponType weaponType, bool useAnalytics = true)
		{
			if(currWeaponIdx < 0 || currWeaponIdx >= Config.Weapons.maxEquipedWeapons+1 || weapons == null) // +1 kvuli bonusWeapon
			{
				Debug.LogError("SetWeaponAtSlot invalid idx " + currWeaponIdx);
				return;
			}

			#if UNITY_EDITOR
			Debug.Log("Equipping weapon " + weaponType + " at idx " + currWeaponIdx);
			#endif

			//

			if(currWeaponIdx == BonusWeaponIdx)
			{
				bonusWeapon = weaponType;
			}
			else if(currWeaponIdx >= 0 && currWeaponIdx <= Config.Weapons.maxEquipedWeapons)
			{
				weapons[currWeaponIdx] = weaponType;
			}

			if(useAnalytics)
				GMReloaded.Analytics.GAI.Instance.LogEvent("Game", "Equiped weapons", currWeaponIdx + "/" + weaponType, 1);
			
			Save();
		}

		public bool HasEquippedWeapon(WeaponType weaponType)
		{
			for(int i = 0; i < 3; i++)
			{
				if(Config.Weapons.localClientEquipedWeapons.GetWeaponAt(i) == weaponType)
				{
					return true;
				}
			}

			return false;
		}

		public void VerifySlots()
		{
			if(weapons == null)
			{
				Debug.Log("Unable verify slots - weapons == null");
				return;
			}

			for(int i = 0; i < weapons.Length; i++)
			{
				var slotWeapon = weapons[i];

				bool slotsAlreadyContainsWeapon = false;

				for(int j = 0; j < weapons.Length; j++)
				{
					var w = weapons[j];

					if(j != i && w == slotWeapon)
					{
						slotsAlreadyContainsWeapon = true;
						break;
					}
				}
				   
				if(slotsAlreadyContainsWeapon)
				{
					Debug.LogError("found " + i + " slotsAlreadyContainsWeapon, reverting to default equip");
					SetWeaponAtSlot(i, Config.Weapons.defaultWeapons[i], false);
				}
			}
		}

		//

		public WeaponType[] Dump()
		{
			WeaponType [] local__weapons = new WeaponType[Config.Weapons.maxEquipedWeapons];

			for(int i = 0; i < weapons.Length; i++)
				local__weapons[i] = weapons[i];

			return local__weapons;
		}

		public void Restore(WeaponType[] weapons)
		{
			if(weapons == null)
				return;

			for(int i = 0; i < weapons.Length; i++)
			{
				SetWeaponAtSlot(i, weapons[i], false);
			}
		}

		//

		private void Load()
		{
			if(usePlayerPrefs)
			{
				PlayerPrefsExtended.GetArray(equipedWeaponsKey, (br) =>
				{
					if(br == null)
					{
						weapons = Config.Weapons.defaultWeapons;
						return;
					}

					weapons = new WeaponType[Config.Weapons.maxEquipedWeapons];

					for(int i = 0; i < weapons.Length; i++)
					{
						weapons[i] = (WeaponType)br.ReadByte();
					}
				});

				if(weapons == null)
					Debug.LogError("weapons == null");
			}
			else
			{
				weapons = new WeaponType[Config.Weapons.maxEquipedWeapons];
			}
		}

		private void Save()
		{
			if(!usePlayerPrefs)
				return;
			
			PlayerPrefsExtended.SetArray(equipedWeaponsKey, (sw) => 
			{
				for(int i = 0; i < weapons.Length; i++)
				{
					sw.Write((byte)weapons[i]);
				}
			});
		}
	}
}