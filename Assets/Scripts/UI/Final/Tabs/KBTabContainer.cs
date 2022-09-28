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

namespace GMReloaded.UI.Final.Tabs
{
	public class KBTabContainer : MonoBehaviourTO, IKBFocusableSuccessorClickReceiver
	{
		[System.Serializable]
		public class KBTabRef
		{
			public KBTab tab;
			public KBFocusableGUIItem parentGUIItem;
		}

		[SerializeField]
		protected List<KBTabRef> tabs = new List<KBTabRef>();

		[SerializeField]
		private List<KBFocusableSuccessors> successors = new List<KBFocusableSuccessors>();

		//

		[SerializeField]
		private KBFocusableSuccessorsGUI parentGUI;

		//

		private KBTab currTab;

		//

		protected virtual void OnEnable()
		{
			FocusTabAtIdx(0);
		}

		protected virtual void Awake()
		{
			for(int i = 0; i < tabs.Count; i++)
			{
				var tr = tabs[i];

				if(tr != null)
				{
					var guiItem = tr.parentGUIItem;

					if(guiItem != null)
					{
						guiItem.RegisterClickReceiver(this);
					}
				}
			}

			if(parentGUI != null)
			{
				parentGUI.OnShow += OnParentGUIShow;
				parentGUI.OnHide += OnParentGUIHide;
			}
		}

		public void FocusTabAtIdx(int idx)
		{
			if(idx < 0 || idx >= tabs.Count)
			{
				Debug.LogError("Unable focus tab at idx " + idx + " - invalid idx value");
				return;
			}

			for(int i = 0; i < tabs.Count; i++)
			{
				var tr = tabs[i];

				if(tr != null)
				{
					var tab = tr.tab;
					if(tab != null)
					{
						tab.Hide();
					}

					var guiItem = tr.parentGUIItem;

					if(guiItem != null)
						guiItem.SetFocused(false);
				}
			}

			var tabRef = tabs[idx];

			if(tabRef == null)
			{
				Debug.LogError("Failed to find tab at idx " + idx);
				return;
			}

			currTab = tabRef.tab;

			if(currTab != null)
			{
				currTab.Show();

				if(parentGUI != null)
				{
					parentGUI.ClearSuccessors();
					parentGUI.AddSuccessor(currTab);

					parentGUI.AddSuccessorRange(successors);

					parentGUI.NotifyFocusableItemsDatasetChanged();

					//

					var parentGUIItem = tabRef.parentGUIItem;

					if(parentGUIItem != null)
					{
						parentGUI.SetFocusedGUIItem(parentGUIItem, true);
					}
				}

			}
		}

		#region IKBFocusableSuccessorClickReceiver implementation

		public void OnClick(KBFocusableGUIItem guiItem)
		{
			for(int i = 0; i < tabs.Count; i++)
			{
				var tr = tabs[i];

				if(tr != null)
				{
					if(tr.parentGUIItem == guiItem)
					{
						FocusTabAtIdx(i);
						break;
					}
				}
			}
		}

		#endregion

		private void OnParentGUIShow()
		{
			if(currTab != null)
				currTab.OnParentGUIShow();
		}

		private void OnParentGUIHide()
		{
			if(currTab != null)
				currTab.OnParentGUIHide();
		}
	}
}