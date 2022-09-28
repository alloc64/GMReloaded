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
using TouchOrchestra.Net;
using System.Collections.Generic;
using System;
using GMReloaded.UI;
using TouchOrchestra;
using GMReloaded.UI.Final;

namespace GMReloaded.API
{
	public class Login : MonoSingletonPersistent<Login>
	{	
		public bool isLoggedIn { get; private set; }

		private bool _remoteDemoLocked = true;
		public bool remoteDemoLocked { get { return _remoteDemoLocked; } private set { _remoteDemoLocked = value; } } 

		private KBMenuRenderer menuRenderer { get { return KBMenuRenderer.IsNull ? null : KBMenuRenderer.Instance; } }

		private LoginObserver _login = null;
		private LoginObserver login
		{
			get
			{
				if(_login == null)
					_login = new LoginObserver(this);

				return _login;
			}
		}

		private Action<JSONServerResponse> OnLoggedIn, OnLoginFailed;

		public void InitiateLogin()
		{
			GMReloaded.UI.Final.Popup.Nick.KBRobotNickPopup.InitiateLogin();
		}

		public void StartLogin(Action<JSONServerResponse> OnLoggedIn = null, Action<JSONServerResponse> OnLoginFailed = null)
		{
			this.OnLoggedIn = OnLoggedIn;
			this.OnLoginFailed = OnLoginFailed;

			login.StartLogin();
		}

		#region Events

		public void LogEvent(string type, string action)
		{
			login.LogEvent(type, action);
		}

		#endregion

		public void OnLoginStarted()
		{
			SetLoadingText("Loading_LoginUser");
		}

		public void _OnLoggedIn(JSONServerResponse response)
		{
			if(OnLoggedIn != null)
			{
				OnLoggedIn(response);
				OnLoggedIn = null;
			}

			#if UNITY_WEBPLAYER || UNITY_WEBGL
			JSONObject payloadJson = new JSONObject(response.payload);

			if(payloadJson != null)
			{
				var obj = payloadJson.GetField("demo_lock");

				if(obj != null)
				{
					this.remoteDemoLocked = obj.b; 

					UIBuyFull.UpdateLicenseStatus();
				}
			}
			#endif

			Debug.Log("Login successfull...");

			//

			if(response.jsonPayload != null)
			{
				long playTime = response.jsonPayload.GetFieldl("playTime");

				#if UNITY_EDITOR
				Debug.Log("playTime " + playTime);
				#endif

				if(playTime > 0)
				{
					#if !UNITY_EDITOR
					LocalClientRobotEmil.user.playedTime = playTime;
					#endif
				}
				else
				{
					Debug.LogWarning("Server returned playTime < 0");
				}
			}

			//

			InitiateGame();
		}

		public void _OnLoginFailed(JSONServerResponse response)
		{
			if(OnLoginFailed != null)
			{
				OnLoginFailed(response);
				OnLoginFailed = null;
			}

			Debug.Log("Login failed:" + response);

			SetLoadingText("Error_LoginServer");

			InitiateGame();
		}

		public void OnJoinedLobby()
		{
			if(isLoggedIn)
				return;

			GMReloaded.UI.Final.Popup.Nick.KBRobotNickPopup.OnJoinedLobby();
		}

		private void InitiateGame()
		{
			if(isLoggedIn)
				return;
			
			API.Score.GetInstance();

			if(!KBMenuRenderer.IsNull)
				KBMenuRenderer.Instance.SetState(null, KBMenuRenderer.State.MainMenu);
			
			isLoggedIn = true;
		}

		private void SetLoadingText(string localizationId)
		{	
			if(menuRenderer != null)
				menuRenderer.SetLoadingHintText(localizationId);
		}
	}
	
}