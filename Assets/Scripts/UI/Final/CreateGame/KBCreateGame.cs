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

namespace GMReloaded.UI.Final.CreateGame
{
	public class KBCreateGame : KBFocusableSuccessorsGUI
	{
		[SerializeField]
		private tk2dBaseSprite arenaScreen;

		[SerializeField]
		private tk2dTextMesh arenaTextMesh;

		[SerializeField]
		private tk2dTextMesh maxPlayersTextMesh;

		//

		[SerializeField]
		private KBFocusableInput roomNameInput;

		[SerializeField]
		private KBFocusableChooser arenaChooser;

		[SerializeField]
		private KBFocusableChooser roundTimeChooser;

		[SerializeField]
		private KBFocusableChooser botCountChooser;

		#region Unity

		protected override void Awake()
		{
			base.Awake();

			if(arenaChooser != null)
			{
				foreach(var arenaId in Config.Arenas.availableArenaSceneIds)
				{
					arenaChooser.AddItemLocalized(arenaId);
				}

				arenaChooser.OnItemChanged = (i) => OnArenaChooserItemChanged(Config.Arenas.availableArenaSceneIds[i]);

				arenaChooser.NotifyItemsChanged();
			}

			//

			if(roundTimeChooser != null)
			{
				int selectedTimeIdx = -1;
				 
				for(int i = 0; i < Config.Arenas.roundTimes.Length; i++)
				{
					int idx = (Config.Arenas.roundTimes.Length - 1) - i;

					var t = Config.Arenas.roundTimes[idx];

					if(t.time == Config.Arenas.roundTimeDefault)
					{
						selectedTimeIdx = i;
					}
						
					roundTimeChooser.AddItemLocalized(t.name);
				}

				roundTimeChooser.NotifyItemsChanged();

				if(selectedTimeIdx != -1)
					roundTimeChooser.SetListIndex(selectedTimeIdx);
			}
		}

		private void UpdateBotCountChooser(int maxPlayers)
		{
			if(botCountChooser == null)
				return;

			botCountChooser.ClearItems();

			for(int i = 0; i < maxPlayers+1; i++)
			{
				botCountChooser.AddItem(i.ToString());
			}


			botCountChooser.NotifyItemsChanged();
			botCountChooser.SetListIndex(Mathf.Clamp(Config.Bots.defaultBotCount, 0, maxPlayers)); // index
		}

		#endregion

		public override void Show(object bundle)
		{
			base.Show(bundle);

			//#if UNITY_EDITOR

			if(roomNameInput.Text.Length < 1)
			{
				roomNameInput.Text = "Random room " + Random.Range(0, 99999);
			}

			//#endif
		}

		#region Events

		public override void OnClick(KBFocusableGUIItem guiItem)
		{
			base.OnClick(guiItem);

			switch(guiItem.name)
			{
				case "Next":
					SwitchToNextStepMadnessMode();
				break;
			}
		}

		//

		private void OnArenaChooserItemChanged(string arenaId)
		{
			//Debug.Log("OnArenaChooserItemChanged " + arenaId);
			//

			Config.Arenas.ArenaConfig cfg = null;

			Config.Arenas.arenaConfig.TryGetValue(arenaId, out cfg);

			if(arenaScreen != null)
			{
				arenaScreen.SetActive(true);
				arenaScreen.SetSpriteByID(arenaId, "NotAvailable");
			}

			if(arenaTextMesh != null)
				arenaTextMesh.text = localization.GetValue(arenaId);

			int maxPlayers = cfg.roomOptions.maxPlayers;

			if(maxPlayersTextMesh != null && cfg != null)
				maxPlayersTextMesh.text = localization.GetValue("MaxPlayers_Param1", maxPlayers);
			
			UpdateBotCountChooser(maxPlayers);
		}

		#endregion

		#region Room Creation

		private void SwitchToNextStepMadnessMode()
		{
			KBGameRoom gameRoom = new KBGameRoom();

			if(roomNameInput != null)
				gameRoom.roomName = roomNameInput.Text;

			if(arenaChooser != null)
				gameRoom.arenaId = Config.Arenas.availableArenaSceneIds[arenaChooser.Index];

			if(roundTimeChooser != null)
				gameRoom.roundTime = Config.Arenas.roundTimes[(Config.Arenas.roundTimes.Length - 1) - roundTimeChooser.Index].time;

			if(botCountChooser != null)
				gameRoom.botCount = botCountChooser.Index+1;

			var roomName = gameRoom.roomName;

			if(roomName == null || roomName.Length <= 4)
			{
				SetError("Room name must be longer than 4 or more characters!");
			}
			else
			{
				bool roomExists = false;
				foreach(RoomInfo roomInfo in PhotonNetwork.GetRoomList())
				{
					if(roomInfo != null && roomInfo.name == roomName)
					{
						roomExists = true;
						break;
					}
				}

				if(roomExists)
				{
					SetError("Server " + gameRoom.roomName + " already exists");
				}
				else
				{
					menuRenderer.gameRoom = gameRoom;
					menuRenderer.SetState(this, KBMenuRenderer.State.CreateGame_MadnessMode);
				}
			}
		}

		#endregion
	}
}
