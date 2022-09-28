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
using GMReloaded.UI.Final.CreateGame;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GMReloaded.UI.Final.MainMenu
{
	public class KBMainMenu : KBFocusableSuccessorsGUI 
	{
		private bool tutorialFirstTimeLoaded = false;

		//

		private CreateGame.KBRoomJoiner roomJoiner = new CreateGame.KBRoomJoiner();

		//

		public override void Show(object bundle)
		{
			base.Show(bundle);

			//

			#if UNITY_EDITOR

			Cloud.CloudSyncedPlayerPrefs.UnitTest();

			#endif

			// odstranim stary data
			menuRenderer.gameRoom = null;

			Config.madnessMode.SetMadnessSteps();

			//UpgradeTree.UpgradeTreeController.Instance.Dump();

			if(Tutorial.TutorialManager.isTutorialCompleted || tutorialFirstTimeLoaded)
			{
				if(Changelog.KBChangelog.CanShow)
					Timer.DelayAsync(0.2f, () => menuRenderer.SetState(this, KBMenuRenderer.State.Changelog));
			}
			else
			{
				LoadTutorial();
				tutorialFirstTimeLoaded = true;
			}
		}

		public override void OnClick(KBFocusableGUIItem guiItem)
		{
			base.OnClick(guiItem);

			switch(guiItem.name)
			{
				case "PlayNow":
					menuRenderer.SetState(this, KBMenuRenderer.State.ServerList);
				break;

				case "CreateGame":
					menuRenderer.SetState(this, KBMenuRenderer.State.CreateGame);
				break;

				case "Tutorial":
					LoadTutorial();
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

				case "Quit":

					if(popup != null)
					{
						popup.SetTitle("ExitGameQuestion");
						popup.SetText("DoYouReallyWantToQuit");

						popup.SetAlertType(GMReloaded.UI.Final.Popup.KBPopup.Type.YesNO);

						popup.OnNegativeButtonClicked = () => 
						{
							
						};

						popup.OnPositiveButtonClicked = () =>
						{
							Application.Quit();
						};

						popup.Show();
					}

				break;
			}
		}

		private void LoadTutorial()
		{
			KBGameRoom gameRoom = new KBGameRoom();

			gameRoom.roomName = Config.tutorial.tutorialRoomNamePrefix + " Room - " + Random.Range(99999, 99999999);
			gameRoom.arenaId = "Tutorial";
			gameRoom.roundTime = 9999999;
			gameRoom.botCount = 0;

			//TODO: hiding roomu

			menuRenderer.gameRoom = gameRoom;

			roomJoiner.JoinOrCreateRoom(menuRenderer, FindObjectOfType<KBServerController>());
		}
	}
}
