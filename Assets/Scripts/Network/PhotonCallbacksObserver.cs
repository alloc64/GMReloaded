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

namespace GMReloaded.UI
{
	public class PhotonCallbacksObserver : Photon.PunBehaviour
	{
		[SerializeField]
		private List<UnityEngine.Component> callbackComponents;

		private List<IPunCallbacks> _callbacks = null;

		private List<IPunCallbacks> callbacks
		{
			get
			{ 
				if(_callbacks == null)
				{
					_callbacks = new List<IPunCallbacks>();

					for(int i = 0; i < callbackComponents.Count; i++)
					{
						var cbc = callbackComponents[i];

						if(cbc != null)
						{
							var ipc = cbc.GetComponent<IPunCallbacks>();

							if(ipc != null)
								_callbacks.Add(ipc);
						}
					}
				}

				return _callbacks;
			}
		}

		#region IPunCallbacks implementation

		public override void OnConnectedToPhoton()
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnConnectedToPhoton();
		}

		public override void OnLeftRoom()
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnLeftRoom();
		}

		public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnMasterClientSwitched(newMasterClient);
		}

		public override void OnPhotonCreateRoomFailed(object[] codeAndMsg)
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnPhotonCreateRoomFailed(codeAndMsg);	
		}
		public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnPhotonJoinRoomFailed(codeAndMsg);	
		}

		public override void OnCreatedRoom()
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnCreatedRoom();	
		}

		public override void OnJoinedLobby()
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnJoinedLobby();
		}

		public override void OnLobbyStatisticsUpdate()
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnLobbyStatisticsUpdate();
		}


		public override void OnLeftLobby()
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnLeftLobby();	
		}

		public override void OnFailedToConnectToPhoton(DisconnectCause cause)
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnFailedToConnectToPhoton(cause);	
		}

		public override void OnConnectionFail(DisconnectCause cause)
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnConnectionFail(cause);	
		}

		public override void OnDisconnectedFromPhoton()
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnDisconnectedFromPhoton();	
		}

		public override void OnPhotonInstantiate(PhotonMessageInfo info)
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnPhotonInstantiate(info);
		}

		public override void OnReceivedRoomListUpdate()
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnReceivedRoomListUpdate();	
		}

		public override void OnJoinedRoom()
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnJoinedRoom();	
		}

		public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnPhotonPlayerConnected(newPlayer);	
		}

		public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnPhotonPlayerDisconnected(otherPlayer);
		}

		public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnPhotonRandomJoinFailed(codeAndMsg);
		}

		public override void OnConnectedToMaster()
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnConnectedToMaster();
		}

		public override void OnPhotonMaxCccuReached()
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnPhotonMaxCccuReached();
		}

		public override void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnPhotonCustomRoomPropertiesChanged(propertiesThatChanged);
		}

		public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnPhotonPlayerPropertiesChanged(playerAndUpdatedProps);
		}

		public override void OnUpdatedFriendList()
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnUpdatedFriendList();	
		}

		public virtual void OnCustomAuthenticationFailed()
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnCustomAuthenticationFailed(null);
		}

		public override void OnCustomAuthenticationFailed(string debugMessage)
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnCustomAuthenticationFailed(debugMessage);
		}

		public override void OnWebRpcResponse(ExitGames.Client.Photon.OperationResponse response)
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnWebRpcResponse(response);	
		}

		public override void OnOwnershipRequest(object[] viewAndPlayer)
		{
			foreach(var callback in callbacks)
				if(callback != null)
					callback.OnOwnershipRequest(viewAndPlayer);
		}
		#endregion
	}
}