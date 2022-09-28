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
using System.Linq;

namespace GMReloaded.UI.Final.Equip
{	
	public class KBWeaponsMenu : KBFocusableSuccessorsGUI 
	{
		[SerializeField]
		private KBWeaponItem weaponItemTemplate;

		[SerializeField]
		private KBFocusableSuccessors weaponFoweaponTitleTextMeshsors;

		[SerializeField]
		private Transform contentContainer;

		[SerializeField]
		private tk2dUIScrollableArea scrollableArea;

		[SerializeField]
		private KBFocusableSuccessors weaponFocusableSuccessors;

		[SerializeField]
		private tk2dTextMesh weaponTitleTextMesh;

		[SerializeField]
		private tk2dTextMesh weaponTextTextMesh;

		[SerializeField]
		private KBWeaponProperty weaponPropertyTemplate;

		[SerializeField]
		private Transform weaponPropertyContainer;

		//

		[SerializeField]
		private KBFocusableGUIItem equipButton;

		[SerializeField]
		private KBFocusableGUIItem backButton;

		[SerializeField]
		private KBFocusableSuccessors bottomButtonsSuccessor;

		//

		private HashSet<KBWeaponItem> weaponItems = new HashSet<KBWeaponItem>();

		private PrefabsRecyclerBase<KBFocusableGUIItem> _weaponItemsRecycler;
		private PrefabsRecyclerBase<KBFocusableGUIItem> weaponItemsRecycler
		{
			get
			{
				if(_weaponItemsRecycler == null)
				{
					_weaponItemsRecycler = new PrefabsRecyclerBase<KBFocusableGUIItem>(weaponItemTemplate, contentContainer);
					_weaponItemsRecycler.Preinstantiate(15);
				}

				return _weaponItemsRecycler;
			}
		}

		private KBWeaponItem focusedWeaponItem = null;
		private KBWeaponSlot focusedWeaponSlot = null;

		//

		private PrefabsRecyclerBase<KBWeaponProperty> _weaponPropertiesRecycler;
		private PrefabsRecyclerBase<KBWeaponProperty> weaponPropertiesRecycler
		{
			get
			{ 
				if(_weaponPropertiesRecycler == null)
				{
					_weaponPropertiesRecycler = new PrefabsRecyclerBase<KBWeaponProperty>(weaponPropertyTemplate, weaponPropertyContainer);
					_weaponPropertiesRecycler.Preinstantiate(5);

					if(weaponPropertyTemplate != null)
						weaponPropertyTemplate.SetActive(false);
				}

				return _weaponPropertiesRecycler;
			}
		}

		private KBWeaponItem bestWeaponItem = null;

		//

		#region Unity

		protected override void Awake()
		{
			base.Awake();
		}

		#endregion

		#region Weapon items generation

		protected virtual void GenerateItems(WeaponSubType weaponSubtype, KBWeaponSlot.Order slotOrder)
		{
			if(weaponItemTemplate == null)
			{
				Debug.LogError("weaponItemTemplate == null");
				return;
			}

			if(weaponItems.Count > 0)
			{
				foreach(var item in weaponItems)
				{
					if(item != null)
					{
						weaponItemsRecycler.Enqueue(item);
					}
				}

				weaponItems.Clear();
			}

			var currEquippedWeaponType = Config.Weapons.localClientEquipedWeapons.GetWeaponAt((int)slotOrder);

			KBWeaponItem equippedWeapon = null;

			float xOffset = 0f;
			foreach(var kvp in Config.Weapons.weaponConfig.OrderBy((w) => ((int)w.Value.minLevel)))
			{
				var wc = kvp.Value;

				if(wc != null && wc.weaponSubType == weaponSubtype)
				{
					var item = weaponItemsRecycler.Dequeue() as KBWeaponItem;

					if(item != null)
					{
						item.SetXPosition(xOffset);
						item.SetWeaponType(kvp.Key);

						item.SetUnlockLevel(wc.minLevel);

						item.SetDamage(wc.damage);
						item.SetWeight(wc.weight);

						if(wc.use != Config.Weapons.WeaponConfig.Use.Forever)
						{
							item.SetReloadTime(wc.reloadTime);
							item.SetAmmo(wc.initProjectileCount);
						}

						if(wc.weaponType == currEquippedWeaponType)
							equippedWeapon = item;

						if(weaponFocusableSuccessors != null)
							weaponFocusableSuccessors.RegisterFocusableItem(item);
					

						xOffset += item.size.x;

						weaponItems.Add(item);

						bestWeaponItem = item;
					}	
				}
			}

			weaponItemTemplate.SetActive(false);

			SetContentLength(Mathf.Abs(xOffset));

			NotifyFocusableItemsDatasetChanged(false);

			if(equippedWeapon != null)
				SetFocusedGUIItem(equippedWeapon);
			else
				Debug.LogWarning("Failed to focus currEquippedWeaponType - weapon item not found");
		}

		protected virtual void SetContentLength(float l)
		{
			if(scrollableArea != null)
				scrollableArea.ContentLength = l + 0.05f;
		}

		#endregion

		public override void Show(object bundle)
		{
			base.Show(bundle);

			focusedWeaponSlot = bundle as KBWeaponSlot;

			if(focusedWeaponSlot != null)
			{
				var slotOrder = focusedWeaponSlot.order;
			
				SetTitle(localization.GetValue("WeaponsMenu_Title") + localization.GetValue(slotOrder.ToString()));

				GenerateItems(focusedWeaponSlot.weaponSubType, slotOrder);

				//

				focusedWeaponItem = null;
			}

			SetEquipButtonActive(false);
		}

		#region Event Management

		public override void OnClick(KBFocusableGUIItem guiItem)
		{
			base.OnClick(guiItem);

			switch(guiItem.name)
			{
				case "Equip":

					GoNext();

				break;
			}
		}

		protected override void OnFocused(KBFocusableGUIItem guiItem)
		{
			base.OnFocused(guiItem);

			SetWeaponInfo(guiItem as KBWeaponItem, bestWeaponItem);
		}

		#endregion

		#region Weapon Info

		private void SetWeaponInfo(KBWeaponItem weaponItem, KBWeaponItem bestWeaponItem)
		{
			if(weaponItem == null)
				return;

			if(bestWeaponItem == null)
			{
				Debug.LogError("bestWeaponItem == null");
				return;
			}

			focusedWeaponItem = null;

			ClearWeaponProperties();

			WeaponType weaponType = weaponItem.weaponType;
			bool locked = !weaponItem.unlocked;

			if(!locked)
			{
				float damage = weaponItem.damage;

				if(damage > 0)
					SetWeaponProperty("Damage", damage, bestWeaponItem.damage);

				float weight = weaponItem.weight;

				Debug.Log(weight + " , " + bestWeaponItem.weight);

				if(weight > 0)
					SetWeaponProperty("Weight", weight, bestWeaponItem.weight);

				float reloadTime = weaponItem.reloadTime;

				if(reloadTime > 0)
					SetWeaponProperty("ReloadTime", reloadTime, bestWeaponItem.reloadTime);

				float ammo = weaponItem.ammo;

				if(ammo > 0)
					SetWeaponProperty("Ammo", ammo, bestWeaponItem.ammo);

				if(!weaponItem.equipped)
				{
					this.focusedWeaponItem = weaponItem;
				}
			}

			Debug.Log("focusedWeaponItem " + focusedWeaponItem);

			SetEquipButtonActive(focusedWeaponItem != null);

			//

			string key = weaponType.ToString();

			if(weaponTitleTextMesh != null)
			{
				weaponTitleTextMesh.text = localization.GetValue(key);
			}

			if(weaponTextTextMesh != null)
			{
				weaponTextTextMesh.text = localization.GetValue(locked ? "Item_Locked_Info" : (key + "_Desc"));
			}
		}

		//

		private HashSet<KBWeaponProperty> weaponProperties = new HashSet<KBWeaponProperty>();

		private void SetWeaponProperty(string titleLocalizationId, float value, float maxValue, bool inverse = false)
		{
			var property = weaponPropertiesRecycler.Dequeue();

			if(property != null)
			{
				property.Setup(weaponProperties.Count, titleLocalizationId, value, maxValue, inverse);
				weaponProperties.Add(property);
			}
		}

		private void ClearWeaponProperties()
		{
			foreach(var property in weaponProperties)
			{
				if(property != null)
					weaponPropertiesRecycler.Enqueue(property);
			}

			weaponProperties.Clear();
		}

		#endregion

		private void SetEquipButtonActive(bool active)
		{
			if(bottomButtonsSuccessor == null)
				return;

			bottomButtonsSuccessor.FocusableItems.Clear();

			if(equipButton != null)
			{
				if(active)
					bottomButtonsSuccessor.RegisterFocusableItem(equipButton);

				equipButton.SetActive(active);
			}

			if(backButton != null)
				bottomButtonsSuccessor.RegisterFocusableItem(backButton);

			//NotifyFocusableItemsDatasetChanged(false);
		}

		public override void GoNext()
		{
			base.GoNext();

			if(focusedWeaponItem != null && focusedWeaponSlot != null)
			{
				focusedWeaponSlot.Equip(focusedWeaponItem);

				//

				var localClientRobotEmil = LocalClientRobotEmil.Instance;

				if(localClientRobotEmil != null)
				{
					localClientRobotEmil.VerifyEquips();
					localClientRobotEmil.ReequipWeapons();
				}
			}

			GoBack();
		}
	}
}
