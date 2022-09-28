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
using Hashtable = ExitGames.Client.Photon.Hashtable;

public static class PhotonPlayerCustomPropertiesExtension 
{
	public static void SetColumn(this PhotonPlayer player, string key, object value)
	{
		Hashtable score = new Hashtable();  // using PUN's implementation of Hashtable
		score[key] = value;

		player.SetCustomProperties(score);  // this locally sets the score and will sync it in-game asap.
	}

	public static T GetColumnStruct<T>(this PhotonPlayer player, string key) where T : struct
	{ 
		object value;
		if (player.customProperties.TryGetValue(key, out value))
		{
			return (T)value;
		}

		return default(T);
	}

	public static T GetColumn<T>(this PhotonPlayer player, string key) where T : class
	{ 
		object value;
		if (player.customProperties.TryGetValue(key, out value))
		{
			return (T)value;
		}

		return default(T);
	}
}
