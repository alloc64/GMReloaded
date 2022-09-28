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

namespace GMReloaded
{
	public class RoomPropertiesController : MonoSingleton<RoomPropertiesController>
	{
		private ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();

		public void Set(string key, object value)
		{
			if(!PhotonNetwork.connectedAndReady)
			{
				Debug.LogWarning("Unable set RoomProperties " + key + " / " + value);
				return;
			}
			
			props.Clear();
			props[key] = value;

			var room = PhotonNetwork.room;

			if(room != null)
				room.SetCustomProperties(props);
		}

		public void SetBatch(ExitGames.Client.Photon.Hashtable values)
		{
			var room = PhotonNetwork.room;

			if(room == null)
				return;

			var customProperties = room.customProperties;

			customProperties.Merge(values);

			room.SetCustomProperties(customProperties);
		}
	}
}