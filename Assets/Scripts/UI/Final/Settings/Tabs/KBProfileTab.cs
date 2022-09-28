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
using System.Collections;
using System.Linq;

namespace GMReloaded.UI.Final.Settings.Tabs
{
	public class KBProfileTab : GMReloaded.UI.Final.Tabs.KBTab
	{
		[SerializeField]
		private KBFocusableInput nickInput;

		[SerializeField]
		private KBFocusableChooser skinChooser;

		protected override void Awake()
		{
			base.Awake();

			//

			if(skinChooser != null)
			{			
				foreach(RobotEmil.Skin skin in Enum.GetValues(typeof(RobotEmil.Skin)))
					skinChooser.AddItemLocalized("SK_" + skin);

				skinChooser.NotifyItemsChanged();
			}
		}

		public override void Show()
		{
			base.Show();

			if(nickInput != null)
				nickInput.Text = LocalClientRobotEmil.user.nick;


			if(skinChooser != null)
				skinChooser.SetListIndex((int)LocalClientRobotEmil.user.skin);
		}

		public override void OnParentGUIHide()
		{
			base.OnParentGUIHide();

			//

			if(nickInput != null)
				LocalClientRobotEmil.user.nick = nickInput.Text;
			
			if(skinChooser != null)
				LocalClientRobotEmil.user.skin = (RobotEmil.Skin)skinChooser.Index;
		}
	}
}