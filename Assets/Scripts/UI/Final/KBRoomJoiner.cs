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
	public class KBRoomJoiner
	{
		private bool joinedRoom = false;

		public void JoinOrCreateRoom(KBMenuRenderer menuRenderer, CreateGame.KBServerController serverController, System.Action onGoNext = null)
		{
			if(menuRenderer == null)
			{
				Debug.LogError("failed to join to server - menuRenderer == null");
				return;
			}

			if(serverController == null)
			{
				Debug.LogError("failed to join to server - serverController == null");
				return;
			}

			var gameRoom = menuRenderer.gameRoom;

			if(gameRoom != null)
			{
				if(gameRoom.roomName == null)
				{
					menuRenderer.SetError("failed to join to server - gameRoom.roomName == null");
				}
				else
				{
					if(!joinedRoom)
					{
						Debug.Log("JoinOrCreateRoom " + gameRoom);

						bool success = serverController.JoinOrCreateRoom(gameRoom.roomName, gameRoom.arenaId);

						if(success)
						{
							joinedRoom = true;

							menuRenderer.GoToLoading(() =>
							{
								joinedRoom = false;

								if(onGoNext != null)
									onGoNext();
							});
						}
						else
						{
							menuRenderer.SetError("Failed to connect to room " + gameRoom.roomName);
						}
					}
				}
			}
			else
			{
				Debug.LogError("failed to join to server - gameRoom == null");
			}

		}
	}
	
}
