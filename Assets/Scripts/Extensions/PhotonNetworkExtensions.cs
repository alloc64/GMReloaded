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

public static class PhotonNetworkExtensions  
{
	public static T LoadPrefab<T>(string path, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
	{
		GameObject go = PhotonNetwork.Instantiate(path, position, rotation, 0);

		if(go == null)
		{
			Debug.Log("Error, prefab " + path + " not found!");
			return null;
		}

		return go.GetComponent<T>();
	}
}
