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

namespace GMReloaded.UI.Final.Popup.Nick
{
	public class KBRobotNickPopup : KBPopup
	{
		//TODO: udelat refaktor flowu v teto tride, je to moc duplicitni
		[SerializeField]
		private KBFocusableInput nickInput;

		public static void InitiateLogin()
		{
			var nick = LocalClientRobotEmil.user.nick;

			if(nick == null)
			{
				#if UNITY_XBOXONE && !UNITY_EDITOR

				XboxOneLoginFlow();

				#else

				#if STEAM_ENABLED

				bool validNick = SetNick(Steam.Steamworks.Instance.playerName);

				if(validNick)
					return;

				#endif

				Timer.DelayAsyncIndependent(1f, RequestNickFromPlayer);

				#endif
			}
		}

		public override void Show(object bundle = null)
		{
			base.Show(bundle);
			SetHideAfterButtonClick(false);
		}

		protected override void _OnPositiveButtonClicked()
		{
			base._OnPositiveButtonClicked();

			if(nickInput == null)
			{
				Debug.LogError("nickInput == null");
				return;
			}

			string nick = nickInput.Text;

			Debug.Log("Set custom player nick " + nick);

			if(LocalClientRobotEmil.user.ValidateNick(nick))
			{
				LocalClientRobotEmil.user.nick = nick;
				GlobalStateController.Instance.OnNickSet();

				Hide();
			}
			else
			{
				SetErrorLocalized("Nick_Length_Error");
			}
		}

		#if UNITY_XBOXONE && !UNITY_EDITOR

		private static void XboxOneLoginFlow()
		{
			GMReloaded.Xbox.XboxAPI.Instance.RequestLogin((nick) => 
			{
				bool validNick = SetNick(nick);

				if(validNick)
				{
					GlobalStateController.Instance.OnNickSet();
				}
				else
				{
					RequestNickFromPlayer();
				}
			});
		}

		#endif

		//

		public static void OnJoinedLobby()
		{
			if(LocalClientRobotEmil.user.nick != null)
				GlobalStateController.Instance.OnNickSet();
			else
			{
				#if !UNITY_XBOXONE && !UNITY_EDITOR
				Debug.Log("nick not set -- trying get one from player");
				RequestNickFromPlayer();
				#endif
			}
		}

		private static void RequestNickFromPlayer()
		{
			if(!KBMenuRenderer.IsNull)
			{
				var menuRenderer = KBMenuRenderer.Instance;

				var robotNickPopup = menuRenderer.robotNickPopup;

				if(robotNickPopup != null)
				{
					robotNickPopup.Show();
				}
				else
				{
					Debug.LogError("Failed to request nick from player - robotNickPopup == null");
				}
			}
			else
			{
				Debug.LogError("Failed to request nick from player - KBMenuRenderer == null");
			}
		}
		
		private static bool SetNick(string nick)
		{
			Debug.Log("User nick " + nick);
			
			if(LocalClientRobotEmil.user.ValidateNick(nick))
			{
				LocalClientRobotEmil.user.nick = nick;
				return true;
			}

			return false;
		}
	}
}
