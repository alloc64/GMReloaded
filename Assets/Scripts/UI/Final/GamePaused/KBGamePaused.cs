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

namespace GMReloaded.UI.Final.GamePaused
{
	public class KBGamePaused : KBFocusableSuccessorsGUI 
	{
		public override void OnClick(KBFocusableGUIItem guiItem)
		{
			base.OnClick(guiItem);

			switch(guiItem.name)
			{
				case "Continue":
					menuRenderer.SetState(this, KBMenuRenderer.State.InGame);
				break;
					
				case "Equip":
					if(!tutorial.isActive)
						menuRenderer.SetState(this, KBMenuRenderer.State.Equip);
				break;

				case "Missions":
					menuRenderer.SetState(this, KBMenuRenderer.State.Missions);
				break;

				case "Options":
					menuRenderer.SetState(this, KBMenuRenderer.State.Settings);
				break;

				case "Charts":
					menuRenderer.SetState(this, KBMenuRenderer.State.Charts);
				break;

				case "Leave":

					if(popup != null)
					{
						popup.SetTitle("ExitGameQuestion");
						popup.SetText("DoYouReallyWantToLeave");

						popup.SetAlertType(GMReloaded.UI.Final.Popup.KBPopup.Type.YesNO);

						popup.OnNegativeButtonClicked = () => 
						{

						};

						popup.OnPositiveButtonClicked = () =>
						{
							GameStateController.Instance.LeaveRoom();
						};

						popup.Show();
					}

				break;
			}
		}
	}
}