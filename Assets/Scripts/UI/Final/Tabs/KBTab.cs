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
	public class KBTab : KBFocusableSuccessors
	{
		//

		protected KBMenuRenderer menuRenderer { get { return KBMenuRenderer.IsNull ? null : KBMenuRenderer.Instance; } }

		protected Popup.KBPopup popup { get { return menuRenderer == null ? null : menuRenderer.popup; } }

		//

		#region Unity

		protected virtual void OnEnable()
		{
			
		}

		protected virtual void Awake()
		{
		}

		#endregion

		#region Visibility

		public virtual void Show()
		{
			SetActive(true);
		}

		public virtual void Hide()
		{
			SetActive(false);
		}

		#endregion

		#region Parent GUI Visibility

		public virtual void OnParentGUIShow()
		{

		}

		public virtual void OnParentGUIHide()
		{

		}

		#endregion
	}
}