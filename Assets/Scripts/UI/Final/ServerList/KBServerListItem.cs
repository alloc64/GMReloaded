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
using System.Linq;

namespace GMReloaded.UI.Final.ServerList
{
	public class KBServerListItem : KBFocusableGUIItemWithStateChange
	{
		[SerializeField]
		private tk2dTextMesh roomNameText;

		[SerializeField]
		private tk2dTextMesh arenaText;

		[SerializeField]
		private tk2dTextMesh playerInfoText;

		//

		public string roomName { get; private set; }

		public int playerCount { get; private set; }

		public int maxPlayers { get; private set; }

		public bool isPremadeRoom { get; private set; }

		public string arenaId { get; private set; }

		//

		public string playerCountInfo { get { return playerCount + "/" + maxPlayers; } }

		#region Setup

		public void Setup(RoomInfo roomInfo)
		{
			Setup(roomInfo.name, roomInfo.playerCount, roomInfo.maxPlayers, roomInfo.GetLevelId());
		}

		private void Setup(string roomName, int playerCount, int maxPlayers, string arenaId, bool isPremadeRoom = false)
		{
			this.roomName = roomName;
			this.playerCount = playerCount;
			this.maxPlayers = maxPlayers;

			this.isPremadeRoom = isPremadeRoom;
			this.arenaId = arenaId;

			if(roomNameText != null)
				roomNameText.text = roomName;

			if(playerInfoText != null)
				playerInfoText.text = playerCountInfo;

			if(arenaId != null && arenaText != null)
			{
				arenaText.text = localization.GetValue(arenaId);
				arenaText.SetActive(arenaId != null);
			}

			SetActive(true);
		}

		#endregion

		protected override void OnHover(bool over)
		{
			// nic
		}

		public override void Reinstantiate()
		{
			if(arenaText != null)
				arenaText.SetActive(false);
		}

		public void SetLocalPositionY(float y)
		{
			Vector3 lp = localPosition;
			lp.y = y;
			localPosition = lp;
		}
	}
	
}
