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
using Steamworks;

namespace GMReloaded.Steam
{
	public class SteamCloudAPI : Cloud.CloudAPIImplBase
	{
		public SteamCloudAPI(Cloud.CloudAPI cloud) : base(cloud)
		{
		
		}

		public override bool isAvailable 
		{ 
			get 
			{ 
				#if UNITY_EDITOR
				//try
				//{
					return SteamRemoteStorage.IsCloudEnabledForApp(); 
				//}
				//catch
				//{
				//	return false;
				//}
				#else
				return SteamRemoteStorage.IsCloudEnabledForApp(); 
				#endif
			} 
		}

		public override byte[] LoadFile(string fileName)
		{
			if(SteamRemoteStorage.FileExists(fileName))
			{
				try
				{
					int length = SteamRemoteStorage.GetFileSize(fileName);

					byte[] bytes = new byte[length];

					if(length > 0)
					{
						SteamRemoteStorage.FileRead(fileName, bytes, length);

						//Debug.Log("Loaded from steam file " + fileName + "\n " + bytes);

						return bytes;
					}
				}
				catch(System.Exception e)
				{
					Debug.LogError("Failed to load SteamCloudAPI file " + fileName);
					Debug.LogException(e);
				}
			}
			else
			{
				//Debug.Log("File " + fileName + " not exists...");
			}

			return null;
		}

		public override bool SaveFile(string fileName, byte[] data)
		{
			if(data == null)
			{
				Debug.LogError("Failed to save SteamCloudAPI file " + fileName + " - data == null ");
				return false;
			}

			try
			{ 
				//Debug.Log("Written to steam file " + fileName + "\n " + data);
				return SteamRemoteStorage.FileWrite(fileName, data, data.Length);
			}
			catch(System.Exception e)
			{
				Debug.LogError("Failed to save SteamCloudAPI file " + fileName);
				Debug.LogException(e);
			}

			return false;
		}
	}
}
