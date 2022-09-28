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
using GMReloaded.UI.Final.ServerList;

namespace GMReloaded.UI.Final.Equip
{
	public class KBEquip : KBFocusableSuccessorsGUI
	{
		// weapon info

		[SerializeField]
		private tk2dTextMesh slotTitle;

		[SerializeField]
		private tk2dTextMesh slotText;

		// missions

		[SerializeField]
		private tk2dTextMesh instructionsTextMesh;

		[SerializeField]
		private tk2dTextMesh rewardTextMesh;

		[SerializeField]
		private GameObject missionContainer; 

		[SerializeField]
		private GMReloaded.UI.Final.CreateGame.KBServerController serverController;

		//

		private CreateGame.KBRoomJoiner roomJoiner = new CreateGame.KBRoomJoiner();

		//

		[SerializeField]
		private KBFocusableButton joinGameButton;

		public override void Show(object bundle)
		{
			base.Show(bundle);

			var mission = Achievements.MissionsController.Instance.nextMission;

			if(missionContainer != null)
				missionContainer.SetActive(mission != null);

			if(mission != null)
			{
				if(instructionsTextMesh != null)
					instructionsTextMesh.text = localization.GetValue(mission.key);

				if(rewardTextMesh != null)
					rewardTextMesh.text = mission.experience + " EXP";
			}

			if(joinGameButton != null)
				joinGameButton.SetActive(!gsc.isInGame);
		}

		public override void OnClick(KBFocusableGUIItem guiItem)
		{
			base.OnClick(guiItem);

			var weaponSlot = guiItem as KBWeaponSlot;

			if(weaponSlot != null)
			{
				menuRenderer.SetState(this, KBMenuRenderer.State.Weapons, weaponSlot);
			}
			else
			{
				// neco jinyho
				switch(guiItem.name)
				{
					case "JoinGame":

						GoNext();

					break;
				}
			}
		}

		protected override void OnFocused(KBFocusableGUIItem guiItem)
		{
			base.OnFocused(guiItem);

			var weaponSlot = guiItem as KBWeaponSlot;

			if(weaponSlot != null)
			{
				weaponSlot.SetWeaponInfo(slotTitle, slotText);
			}
		}

		public override void GoNext()
		{
			roomJoiner.JoinOrCreateRoom(menuRenderer, serverController, base.GoNext);	
		}
	}
}