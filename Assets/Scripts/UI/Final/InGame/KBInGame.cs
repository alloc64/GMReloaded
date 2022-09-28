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

using Input = TeamUtility.IO.InputManager;

namespace GMReloaded.UI.Final.InGame
{
	public class KBInGame : KBFocusableSuccessorsGUI 
	{
		protected override void Update()
		{
			base.Update();

			if(!tutorial.isActive && !menuRenderer.isInMenu && Input.GetButtonUp(Config.Player.KeyBind.Equip))
			{
				menuRenderer.SetState(this, KBMenuRenderer.State.Equip);
			}
		}

		protected override void UpdateItemInputFocus()
		{
			// tady nic focusnout nejde
		}

		public override void Show(object bundle)
		{
			base.Show(bundle);

			gsc.ShowCursor(false);
		}

		public override void Hide()
		{
			base.Hide();

			gsc.ShowCursor(true);
		}

		protected override void GoBack()
		{
			base.GoBack();

			menuRenderer.SetState(this, KBMenuRenderer.State.GamePaused);
		}
	}
}