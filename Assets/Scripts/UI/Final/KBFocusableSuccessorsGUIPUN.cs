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

namespace GMReloaded.UI.Final
{

	public class KBFocusableSuccessorsGUIPUN : KBFocusableSuccessorsGUI, IPunCallbacks
	{
		private GlobalStateController globalStateController { get { return GlobalStateController.Instance; }}

		private GameStateController gameStateController { get { return GameStateController.Instance; } }

		//

		#region IPunCallbacks implementation

		public virtual void OnConnectedToPhoton() {}

		public virtual void OnLeftRoom() {}

		public virtual void OnMasterClientSwitched(PhotonPlayer newMasterClient) {}

		public virtual void OnPhotonCreateRoomFailed(object[] codeAndMsg) {}

		public virtual void OnPhotonJoinRoomFailed(object[] codeAndMsg) {}

		public virtual void OnCreatedRoom() 
		{
			if(menuRenderer.gameRoom == null)
			{
				OnConnectError("Invalid state detected - menuRenderer.gameRoom == null");
				return;
			}

			if(PhotonNetwork.isMasterClient)
			{
				ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.room.customProperties;
				properties[ArenaEventDispatcher.PropsConstants.ServerKey] = PhotonNetwork.room.name;
				PhotonNetwork.room.SetCustomProperties(properties);

				ArenaEventDispatcher.OnArenaChange(menuRenderer.gameRoom.arenaId, menuRenderer.gameRoom.roundTime);
			}

			Debug.Log("OnCreatedRoom");
		}

		public virtual void OnJoinedLobby() 
		{
			globalStateController.OnJoinedLobby();
			Debug.Log("OnJoinedLobby");
		}

		public virtual void OnLeftLobby() {}

		public virtual void OnFailedToConnectToPhoton(DisconnectCause cause) {}

		public virtual void OnConnectionFail(DisconnectCause cause) {}

		public virtual void OnDisconnectedFromPhoton() {}

		public virtual void OnPhotonInstantiate(PhotonMessageInfo info) {}

		public virtual void OnReceivedRoomListUpdate() {}

		public void OnJoinedRoom() 
		{
			//if(!activeInHierarchy)
			//	return;

			var room = PhotonNetwork.room;

			if(room != null)
			{ 
				///menuRenderer.SetState(this, KBMenuRenderer.State.Loading);

				gameStateController.SetServerName(room.name, false);

				string level = room.GetLevelId();

				if(level != null)
				{
					bool validLevel = false;  

					foreach(var kvp in Config.Arenas.arenaConfig)
					{
						var arena = kvp.Key;

						if(arena != null && arena == level && Application.CanStreamedLevelBeLoaded(arena))
						{
							validLevel = true;
							break;
						}
					}

					if(!validLevel)
					{
						OnConnectError("Unable connect to server, received invalid level!");

						return;
					}

					LevelLoader.Instance.LoadArena(level, 1f);
				}
				else
				{
					OnConnectError("Unable connect to server, level == null");
				}
			}
			else
			{
				OnConnectError("Unable connect to server, room == null");
			}
		}

		public virtual void OnPhotonPlayerConnected(PhotonPlayer newPlayer) {}

		public virtual void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer) {}

		public virtual void OnPhotonRandomJoinFailed(object[] codeAndMsg) {}

		public virtual void OnConnectedToMaster() {}

		public virtual void OnPhotonMaxCccuReached() {}

		public virtual void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged) {}

		public virtual void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps) {}

		public virtual void OnUpdatedFriendList() {}

		public virtual void OnCustomAuthenticationFailed(string debugMessage) {}

		public virtual void OnWebRpcResponse(ExitGames.Client.Photon.OperationResponse response) {}

		public virtual void OnOwnershipRequest(object[] viewAndPlayer) {}

		public virtual void OnLobbyStatisticsUpdate() {}

		#endregion

		//

		#region Connecting

		private void OnConnectError(string error)
		{
			if(error != null)
				menuRenderer.SetError(error);

			//TODO: main menu -- error handle?
		}

		#endregion
	}
	
}
