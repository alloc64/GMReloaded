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
using GMReloaded.UI;
using GMReloaded.UI.Final;

namespace GMReloaded
{
	public class GlobalStateController : MonoSingletonPersistent<GlobalStateController> 
	{
		private API.Login login { get { return API.Login.Instance; } }

		private bool _isInGame = false;
		public bool isInGame { get { return _isInGame; } set { _isInGame = value; } }

		public bool hasStoppedGameLoading { get; set; }

		private RoomPropertiesController rpc { get { return RoomPropertiesController.Instance; } }

		private bool showCursor = true;

		public void ShowCursor(bool shown)
		{
			showCursor = shown;
		}

		private void Update()
		{
			LocalClientRobotEmil.user.playedTime += Time.deltaTime;

			Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
			Cursor.visible = showCursor;

			#if UNITY_EDITOR

			if(Input.GetKeyUp(KeyCode.O))
				LargeScreenShot.TakeLargeScreenshot(2);

			#endif
		}

		#region Events

		public void OnGameStarted()
		{
			login.InitiateLogin();
		}

		public void OnJoinedLobby()
		{
			login.OnJoinedLobby();
		}

		public void OnNickSet()
		{
			login.StartLogin();
		}

		#endregion
	}
}
