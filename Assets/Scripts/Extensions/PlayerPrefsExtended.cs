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
using GMReloaded;
using System;

namespace GMReloaded
{
	public class PlayerPrefsExtended
	{
		public static byte[] GetBytes(string key)
		{
			string value = PlayerPrefs.GetString(key);

			if(value == null || value.Length < 1)
				return null;

			return System.Convert.FromBase64String(value);
		}

		public static void SetBytes(string key, byte [] bytes)
		{
			if(bytes == null || bytes.Length < 1)
				return;

			PlayerPrefs.SetString(key, System.Convert.ToBase64String(bytes));
		}

		public static void GetArray(string key, Action<System.IO.BinaryReader> callback)
		{
			string value = PlayerPrefs.GetString(key, null);

			if(string.IsNullOrEmpty(value))
			{
				if(callback != null)
					callback(null);

				return;
			}

			byte[] bytes = System.Convert.FromBase64String(value);

			using (System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes))
			{
				using(System.IO.BinaryReader br = new System.IO.BinaryReader(ms))
				{
					if(callback != null)
					{
						try
						{
							callback(br);
						}
						catch(Exception e)
						{
							Debug.LogException(e);
							callback(null);
						}
					}
				}
			}
		}

		public static void SetArray(string key, Action<System.IO.BinaryWriter> callback)
		{
			using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
			{
				using(System.IO.BinaryWriter sw = new System.IO.BinaryWriter(ms))
				{
					if(callback != null)
					{
						try
						{
							callback(sw);
						}
						catch(Exception e)
						{
							Debug.LogException(e);
							callback(null);
						}
					}

					string converted = System.Convert.ToBase64String(ms.ToArray());

					PlayerPrefs.SetString(key, converted);
				}
			}
		}
	}
	
}