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

namespace GMReloaded.UI.Final
{
	public class KBFocusableGUIItem : IRecyclablePrefab<KBFocusableGUIItem>
	{
		[SerializeField]
		private tk2dUIItem uiItem;

		[SerializeField]
		private tk2dBaseSprite pointer;

		[SerializeField]
		private bool isPointerActivable = true;

		[SerializeField]
		public bool unfocusOldItems = true;

		[SerializeField]
		private bool enterGoNextEnabled = false;

		//

		protected KBFocusableSuccessorsGUI parentGUI;

		private HashSet<IKBFocusableSuccessorClickReceiver> clickReceivers = new HashSet<IKBFocusableSuccessorClickReceiver>();

		//

		protected KBFocusableSuccessors parentSuccessor;

		#region Unity

		protected virtual void OnEnable()
		{
			
		}

		protected virtual void Awake()
		{
			if(uiItem == null)
				uiItem = GetComponent<tk2dUIItem>();

			if(uiItem == null)
			{
				Debug.LogError("uiItem == null unable to initialize KBFocusableGUIItem");
				return;
			}

			uiItem.OnClick += () => OnClick(false);
			uiItem.OnHoverOver += () => OnHover(true);
			uiItem.OnHoverOut += () => OnHover(false);
		}

		#endregion

		#region Setup 

		public void Setup(KBFocusableSuccessors successor, KBFocusableSuccessorsGUI gui)
		{
			this.parentSuccessor = successor;
			this.parentGUI = gui;
		}

		#endregion

		#region Focusable GUI Item Events

		protected virtual void OnHover(bool over)
		{
			if(over && parentGUI != null)
			{
				parentGUI.SetFocusedGUIItem(this, unfocusOldItems);
			}
		}

		public virtual void OnClick(bool keyboardInput)
		{
			if(parentGUI != null)
			{
				Focus();
				parentGUI.OnClick(this);

				if(keyboardInput && enterGoNextEnabled)
				{
					parentGUI.GoNext();
				}
			}

			for(var i = clickReceivers.GetEnumerator(); i.MoveNext();)
			{
				var cl = i.Current;

				if(cl != null)
					cl.OnClick(this);
			}
		}

		#endregion

		#region implemented abstract members of IRecyclablePrefab

		public override void Reinstantiate()
		{
			//defaultne nepouzito, pouzivaj to jenom nektery komponenty
			throw new System.NotImplementedException();
		}

		#endregion

		#region Click Receivers

		public bool RegisterClickReceiver(IKBFocusableSuccessorClickReceiver clickReceiver)
		{
			return clickReceivers.Add(clickReceiver);
		}

		public bool UnregisterClickReceiver(IKBFocusableSuccessorClickReceiver clickReceiver)
		{
			return clickReceivers.Remove(clickReceiver);
		}

		#endregion

		public void Focus()
		{
			if(parentGUI != null)
				parentGUI.SetFocusedGUIItem(this);
		}

		public virtual void SetFocused(bool active)
		{
			if(isPointerActivable && pointer != null)
				pointer.SetActive(active);
		}

		public void SetPointerAlpha(float a)
		{
			if(pointer != null)
				pointer.SetAlpha(a);
		}
	}
	
}