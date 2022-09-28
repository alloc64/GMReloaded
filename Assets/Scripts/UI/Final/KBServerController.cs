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
	public class KBServerController : PhotonCallbacksObserver
	{
		//

		private Timer reconnectTimer = new Timer();

		//

		private GameStateController gameStateController { get { return GameStateController.Instance; } }

		private KBMenuRenderer menuRenderer { get { return KBMenuRenderer.IsNull ? null : KBMenuRenderer.Instance; } }

		protected Popup.KBPopup popup { get { return menuRenderer == null ? null : menuRenderer.popup; } }

		#region Unity

		private void Awake()
		{
			FirstConnect();
		}

		private void Update()
		{
			if(PhotonNetwork.connectionState == ConnectionState.Disconnected)
			{
				reconnectTimer.Delay(1f, TryConnect);
			}
		}

		#endregion

		public void FirstConnect()
		{
			PhotonNetwork.automaticallySyncScene = false;
			PhotonNetwork.SendMonoMessageTargetType = typeof(Photon.PunBehaviour);

			if(PhotonNetwork.connectionStateDetailed == PeerState.PeerCreated)
			{
				TryConnect();
			}
		}

		public void TryConnect()
		{
			PhotonNetwork.AuthValues = new AuthenticationValues();
			PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.Custom;
			PhotonNetwork.AuthValues.AuthGetParameters = "app_id=" + System.Uri.EscapeDataString(Config.appID) + "&token=" + System.Uri.EscapeDataString(Config.token);

			PhotonNetwork.ConnectUsingSettings(Config.clientVersion);
		}
			
		public bool JoinOrCreateRoom(string roomName, string selectedLevelId)
		{
			var cfg = Config.Arenas.GetConfig(selectedLevelId);

			if(cfg == null)
				return false;
			
			var roomOptions = cfg.roomOptions;

			Madness.MadnessModeStepsMPStruct madness = new GMReloaded.Madness.MadnessModeStepsMPStruct();

			roomOptions.customRoomProperties = new ExitGames.Client.Photon.Hashtable() { { ArenaEventDispatcher.PropsConstants.MadnessKey, madness.Serialize() } };

			bool success = PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, null);

			if(success)
				gameStateController.SetServerName(roomName);

			return success;
		}

		public override void OnCreatedRoom()
		{
			base.OnCreatedRoom();
		}

		public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
		{
			base.OnPhotonRandomJoinFailed(codeAndMsg);

			Debug.Log("OnPhotonRandomJoinFailed");

			menuRenderer.SetError("Join random room failed");
		}

		public override void OnPhotonMaxCccuReached()
		{
			base.OnPhotonMaxCccuReached();

			Debug.Log("OnPhotonMaxCccuReached");

			menuRenderer.SetError("Maximal number of concurrent players reached");
		}

		public override void OnFailedToConnectToPhoton(DisconnectCause cause)
		{
			base.OnFailedToConnectToPhoton(cause);
			Debug.Log("OnFailedToConnectToPhoton " + cause);

			menuRenderer.SetError("Check your internet connection.\n" + cause);
		}

		public override void OnConnectionFail(DisconnectCause cause)
		{
			base.OnConnectionFail(cause);

			Debug.Log("OnConnectionFail " + cause);

			menuRenderer.SetError("Connection to server failed.\n" + cause);
		}


		public override void OnDisconnectedFromPhoton()
		{
			base.OnDisconnectedFromPhoton();
		}

		private bool customAuthFailed = false;

		public override void OnCustomAuthenticationFailed()
		{
			base.OnCustomAuthenticationFailed();

			Debug.Log("OnCustomAuthenticationFailed");

			customAuthFailed = true;

			OnInvalidVersionAlert();

			Analytics.GAI.Instance.LogEvent("Servers", "Failed Authentication", Config.clientVersion, 1);

			menuRenderer.SetError("Custom authentication failed");
		}

		private void OnInvalidVersionAlert()
		{
			if(customAuthFailed)
			{
				if(popup != null)
				{
					popup.SetTitle("AB_Title");
					popup.SetText("AB_InvalidVersion");

					popup.SetAlertType(GMReloaded.UI.Final.Popup.KBPopup.Type.OK);

					popup.OnNegativeButtonClicked = () => 
					{

					};

					popup.OnPositiveButtonClicked = () =>
					{
						Utils.OpenURL(Config.invalidVersionURL);
					};

					popup.SetHideAfterButtonClick(false);

					popup.Show();
				}
			}
		}
	}
	
}
