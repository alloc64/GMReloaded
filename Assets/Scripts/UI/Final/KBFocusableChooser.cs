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
using System;
using System.Collections.Generic;

namespace GMReloaded.UI.Final
{
	public class KBFocusableChooser : KBFocusableGUIItemWithStateChange
	{
		[SerializeField]
		protected tk2dTextMesh titleTextMesh;

		[SerializeField]
		protected tk2dTextMesh textTextMesh;

		[SerializeField]
		protected tk2dUIItem arrowLeftUiItem;

		private tk2dSprite _arrowLeftSprite;
		protected tk2dSprite arrowLeftSprite { get { if(_arrowLeftSprite == null && arrowLeftUiItem != null) _arrowLeftSprite = arrowLeftUiItem.GetComponent<tk2dSprite>(); return _arrowLeftSprite; } }

		[SerializeField]
		protected tk2dUIItem arrowRightUiItem;

		private tk2dSprite _arrowRightSprite;
		protected tk2dSprite arrowRightSprite { get { if(_arrowRightSprite == null && arrowRightUiItem != null) _arrowRightSprite = arrowRightUiItem.GetComponent<tk2dSprite>(); return _arrowRightSprite; } }

		[SerializeField]
		private string presetTitleLocalizationId;

		public Action<int> OnItemChanged;

		public int Index { get { return listIndex; } }

		//

		private bool disabled;

		//

		#region Unity

		protected override void Awake()
		{
			base.Awake();

			SetTitleLocalized(presetTitleLocalizationId);

			if(arrowLeftUiItem == null)
			{
				Debug.LogError("arrowLeftUiItem == null");
				return;
			}

			if(arrowRightUiItem == null)
			{
				Debug.LogError("arrowRightUiItem == null");
				return;
			}

			arrowLeftUiItem.OnClick += OnLeftArrowClick;
			arrowRightUiItem.OnClick += OnRightArrowClick;
		}

		#endregion

		#region List management

		protected int listIndex = 0;
		protected int lastListIndex = 0;

		protected List<string> listItems = new List<string>();

		public void AddItem(string item)
		{
			listItems.Add(item);
		}

		public void AddItemLocalized(string itemLocalizationId)
		{
			if(itemLocalizationId == null)
				return;

			AddItem(localization.GetValue(itemLocalizationId));
		}

		//

		public void ClearItems()
		{
			listItems.Clear();

			listIndex = 0;
			lastListIndex = 0;
		}

		public void NotifyItemsChanged()
		{
			//TODO: reload items
			SwitchItem();
		}

		#endregion

		public void SetTitleLocalized(string titleLocalizationId)
		{
			if(titleTextMesh != null)
				titleTextMesh.text = localization.GetValue(titleLocalizationId);
		}

		public void SetTitle(string title)
		{
			if(titleTextMesh != null)
				titleTextMesh.text = title == null ? "" : title;
		}
			

		public virtual void SetDisabled(bool disabled)
		{
			this.disabled = disabled;

			SetAlpha(disabled ? 0.5f : 1f);
		}

		#region Item Switching

		public virtual void OnSwitchItem(int d)
		{
			if(!disabled)
			{
				lastListIndex = listIndex;

				listIndex += d;

				if(listIndex >= listItems.Count)
					listIndex = 0;

				if(listIndex < 0)
					listIndex = listItems.Count - 1;

				SwitchItem();
			}
		}

		private void OnLeftArrowClick()
		{
			Focus();

			OnSwitchItem(-1);
		}

		private void OnRightArrowClick()
		{
			Focus();

			OnSwitchItem(1);
		}

		public void SetListIndex(int listIndex)
		{
			this.lastListIndex = this.listIndex;
			this.listIndex = listIndex;

			SwitchItem();
		}

		protected void SwitchItem()
		{
			SwitchItem(listIndex);
		}

		protected void SwitchItem(int index)
		{
			if(textTextMesh == null)
				return;

			if(index < 0 || index >= listItems.Count)
			{
				Debug.LogError(index + " invalid - " + listItems.Count, transform);
				return;
			}

			textTextMesh.text = listItems[index];

			_OnItemChanged(index);
		}

		protected virtual void _OnItemChanged(int index)
		{
			if(OnItemChanged != null)
				OnItemChanged(index);
		}

		#endregion

		private void SetAlpha(float a)
		{
			if(titleTextMesh != null)
				titleTextMesh.SetAlpha(a);

			if(textTextMesh != null)
				textTextMesh.SetAlpha(a);

			if(arrowLeftSprite != null)
				arrowLeftSprite.SetAlpha(a);

			if(arrowRightSprite != null)
				arrowRightSprite.SetAlpha(a);
		}
	}
}