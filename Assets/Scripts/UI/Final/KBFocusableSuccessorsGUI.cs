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

using Input = TeamUtility.IO.InputManager;

namespace GMReloaded.UI.Final
{
	public abstract class KBFocusableSuccessorsGUI : MonoBehaviourTO
	{
		[SerializeField]
		private GameObject content;

		[SerializeField]
		private tk2dTextMesh titleTextMesh;

		[SerializeField]
		private List<KBFocusableSuccessors> successors = new List<KBFocusableSuccessors>();

		[SerializeField]
		public GMReloaded.UI.Final.KBMenuRenderer.State state;

		private List<KBFocusableGUIItem> _guiItemsList;
		private List<KBFocusableGUIItem> guiItemsList
		{
			get
			{ 
				if(_guiItemsList == null)
				{
					NotifyFocusableItemsDatasetChanged();
				}

				return _guiItemsList;
			}
		}

		private KBFocusableGUIItem focusedGUIItem = null;

		private int focusedGUIItemIdx = 0;

		private Timer loopPointerAlfaTimer = new Timer();

		private float focusableItemUpdateTime = 0.2f;
		private float focusableItemUpdateTimer = 0f;

		//

		private bool focusFirstItem = true;

		//

		public event System.Action OnShow;
		public event System.Action OnHide;

		//

		protected KBMenuRenderer menuRenderer { get { return KBMenuRenderer.IsNull ? null : KBMenuRenderer.Instance; } }

		protected Popup.KBPopup popup { get { return menuRenderer == null ? null : menuRenderer.popup; } }

		protected GlobalStateController gsc { get { return GlobalStateController.Instance; } }

		#region Unity

		protected virtual void Awake()
		{
		}

		protected virtual void Start()
		{
			NotifyFocusableItemsDatasetChanged();
		}

		protected virtual void Update()
		{
			if(menuRenderer.focusedGUI != this)
				return;

			UpdateItemInputFocus();

			UpdateFocusedItem();

			UpdateGoBack();
		}

		#endregion

		#region Input updates

		protected virtual void UpdateItemInputFocus()
		{
			if(focusableItemUpdateTimer > 0f)
				focusableItemUpdateTimer -= Time.deltaTime;

			if(focusableItemUpdateTimer <= 0)
			{
				Vector2 input = new Vector2(Input.GetAxis(Config.Player.KeyBind.DPADVertical), Input.GetAxis(Config.Player.KeyBind.DPADHorizontal));

				bool resetTime = false;

				if(input.x < 0f)
				{
					FocusItem(1);
					resetTime = true;
				}
				else if(input.x > 0f)
				{
					FocusItem(-1);
					resetTime = true;
				}

				var chooser = focusedGUIItem as KBFocusableChooser;

				if(chooser != null)
				{
					if(input.y < 0f)
					{
						chooser.OnSwitchItem(1);
						resetTime = true;
					}
					else if(input.y > 0f)
					{
						chooser.OnSwitchItem(-1);
						resetTime = true;
					}
				}

				if(resetTime)
					focusableItemUpdateTimer = focusableItemUpdateTime;
			}
		}

		protected virtual void UpdateFocusedItem()
		{
			if(focusedGUIItem != null)
			{
				float pointerAlpha = loopPointerAlfaTimer.Loop(0.5f, 1f, 1f);

				focusedGUIItem.SetPointerAlpha(pointerAlpha);

				if(Input.GetButtonUp(Config.Player.KeyBind.Submit))
				{
					focusedGUIItem.OnClick(true);
				}
			}
		}

		protected virtual void UpdateGoBack()
		{
			if(Input.GetButtonUp(Config.Player.KeyBind.Cancel))
			{
				GoBack();
			}
		}

		#endregion

		#region Visibility

		public virtual void Show(object bundle = null)
		{
			menuRenderer.SetFocusedGUI(this);

			SetActive(true);

			if(content != null)
				content.SetActive(true);

			if(OnShow != null)
				OnShow();
		}

		public virtual void Hide()
		{
			if(content != null)
				content.SetActive(false);
			
			SetActive(false);

			if(OnHide != null)
				OnHide();
		}

		protected virtual void GoBack()
		{
			menuRenderer.GoBack();
		}

		public virtual void GoNext()
		{
			
		}

		#endregion

		#region Events

		public virtual void OnClick(KBFocusableGUIItem guiItem)
		{
			switch(guiItem.name)
			{
				case "Back":
					GoBack();
				break;
			}

			//Debug.Log("OnClick " + guiItem.name, guiItem);

			//OnGUIInteraction(guiItem);
		}

		protected virtual void OnFocused(KBFocusableGUIItem guiItem)
		{
			//Debug.Log("OnFocused " + guiItem.name, guiItem);
		}

		#endregion

		#region Focus management

		private void FocusItem(int delta)
		{
			focusedGUIItemIdx += delta;

			int guiItemsCount = guiItemsList.Count-1;

			if(focusedGUIItemIdx > guiItemsCount)
				focusedGUIItemIdx = 0;

			if(focusedGUIItemIdx < 0)
				focusedGUIItemIdx = guiItemsCount;

			FocusItemAtIndex(focusedGUIItemIdx);
		}

		private bool FocusItemAtIndex(int idx)
		{
			var items = guiItemsList;

			if(items != null && items.Count > 0 && idx >= 0 && idx <= items.Count-1)
			{
				FocusFocusableGUIItem(items[idx]);
				return true;
			}
			else
			{
				Debug.LogError("Failed to FocusFirstFocusableGUIItem - idx " + idx + " / " + items.Count);
				return false;
			}
		}

		private void FocusFocusableGUIItem(KBFocusableGUIItem guiItem, bool unfocusOldItems = true)
		{
			var items = guiItemsList;

			if(unfocusOldItems)
			{
				if(items != null && items.Count > 0)
				{
					foreach(var item in items)
						item.SetFocused(false);
				}
			}

			if(guiItem != null)
				guiItem.SetFocused(true);
			
			this.focusedGUIItem = guiItem;

			for(int i = 0; i < guiItemsList.Count; i++)
			{
				if(guiItemsList[i] == guiItem)
				{
					focusedGUIItemIdx = i;

					OnFocused(guiItem);
					break;
				}
			}
		}

		public void SetFocusedGUIItem(KBFocusableGUIItem guiItem, bool unfocusOldItems = true)
		{
			if(this.focusedGUIItem == guiItem)
				return;

			FocusFocusableGUIItem(guiItem, unfocusOldItems);
		}

		#endregion

		public void SetTitle(string localizationId)
		{
			if(titleTextMesh != null)
				titleTextMesh.text = localization.GetValue(localizationId);
		}

		protected void SetFocusFirstItem(bool focusFirstItem)
		{
			this.focusFirstItem = focusFirstItem;
		}

		//

		#region Successors

		public bool AddSuccessorRange(List<KBFocusableSuccessors> addedSuccessors)
		{
			if(addedSuccessors == null || addedSuccessors.Count < 1)
				return false;

			foreach(var successor in addedSuccessors)
			{
				if(successor != null)
					AddSuccessor(successor);
			}

			return true;
		}

		public bool AddSuccessor(KBFocusableSuccessors successor)
		{
			if(successors.Contains(successor))
				return false;

			successors.Add(successor);

			return true;
		}

		public void ClearSuccessors()
		{
			successors.Clear();
		}

		public void NotifyFocusableItemsDatasetChanged()
		{
			NotifyFocusableItemsDatasetChanged(focusFirstItem);
		}

		public void NotifyFocusableItemsDatasetChanged(bool focusFirstItem)
		{			
//			Debug.Log("NotifyFocusableItemsDatasetChanged");

			if(_guiItemsList == null)
				_guiItemsList = new List<KBFocusableGUIItem>();
			else
				_guiItemsList.Clear();

			foreach(var successor in successors)
			{
				if(successor != null)
				{
					var successorItems = successor.FocusableItems;

					if(successorItems != null)
					{
						foreach(var sItem in successorItems)
						{
							if(sItem != null)
							{
								sItem.Setup(successor, this);
								_guiItemsList.Add(sItem);
							}
						}
					}
				}
			}

			if(_guiItemsList.Count == 0)
			{
				Debug.LogWarning("_guiItemsList.Count == 0 in " + name);
				return;
			}

			if(focusFirstItem)
				FocusItemAtIndex(0);
		}

		#endregion

		#region Error

		public void SetErrorLocalized(string locId)
		{
			menuRenderer.SetErrorLocalized(locId);
		}

		public void SetError(string text)
		{
			menuRenderer.SetError(text);
		}

		#endregion
	}
}