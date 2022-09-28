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

#if UNITY_XBOXONE && !UNITY_EDITOR

using Storage;
using Users;
using UnityAOT;


namespace GMReloaded.Xbox
{
	public class XboxAPI : MonoSingletonPersistent<XboxAPI> 
	{
		private User loggedUser;

		//

		private System.Action<string> onLoginSuccessCallback;

		//

		protected override void Awake()
		{
			base.Awake();

			Initialize();
		}

		private void Initialize()
		{
			Debug.Log("Initializing Xbox services...");

			UnityPluginLog.PluginLogManager.Create();
			UnityPluginLog.PluginLogManager.OnLog += PluginLogManager_OnLog;

			StorageManager.Create();

			UsersManager.Create();
			UsersManager.OnSignInComplete += OnSignInComplete;
		}

		#region Login

		public void RequestLogin(System.Action<string> onLoginSuccessCallback)
		{
			this.onLoginSuccessCallback = onLoginSuccessCallback;

			RequestLoginInternal();
		}

		private void RequestLoginInternal()
		{
			UsersManager.RequestSignIn(AccountPickerOptions.AllowGuests);
		}
		
		private void OnSignInComplete(int resultType, int id)
		{
			Debug.Log("OnSignInComplete " + resultType + " - " + id);

			loggedUser = UsersManager.GetAppCurrentUser();

			if(loggedUser == null)
			{
				Debug.LogError("ERROR: failed to retrieve logged user");
				
				RequestLoginInternal();

				return;
			}

			LoadUserProfile(loggedUser);
		}

		private void LoadUserProfile(User u)
		{
			ProfileService.GetUserProfileAsync(u.Id, u.UID, (XboxUserProfile prof, GetObjectAsyncOp<XboxUserProfile> op) =>
			{
				if (!op.Success)
				{
					Debug.LogError("ERROR: failed to get XboxUserProfile for user!");
					return;
				}

				if(onLoginSuccessCallback != null)
					onLoginSuccessCallback(prof.Gamertag);

				Debug.Log("Got user profile for: " + prof.Gamertag);

				/*
				int picDim     = Picture.GetDimension(Picture.Size.Medium);
				remoteRect     = new Rect(xOffset, 100, picDim, picDim);
				remoteGamerPic = new Texture2D(picDim, picDim, TextureFormat.DXT1, false);

				string url = prof.GetGameDisplayPictureResizeUriForSize(Picture.Size.Medium);
				*/
			});
		}

		#endregion

		#region Logging

		private void PluginLogManager_OnLog(UnityPluginLog.LogChannels channel, string message)
		{
			// Let us see useful information about any failures happening in the plugins.
			if(channel == UnityPluginLog.LogChannels.kLogErrors || channel == UnityPluginLog.LogChannels.kLogExceptions)
				Debug.LogError(message);
		}

		#endregion

		protected override void OnDestroy()
		{
			base.OnDestroy();

			UsersManager.Destroy();
		}
	}
}

#endif