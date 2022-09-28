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
using CodeStage.AntiCheat.ObscuredTypes;

namespace GMReloaded.Cloud
{		
	public class CloudAPI : MonoSingletonPersistent<CloudAPI> 
	{
		public enum DataType : int
		{
			Bytes,
			Integer,
			Long,
			Float,
			Double,
			String,
			Bool
		}

		//

		private CloudAPIImplBase _cloudAPI = null;
		private CloudAPIImplBase cloudAPI
		{
			get
			{
				if(_cloudAPI == null)
				{
					#if STEAM_ENABLED	

					_cloudAPI = new Steam.SteamCloudAPI(this);

					#endif
				}

				return _cloudAPI;
			}
		}

		private CloudAsyncWritingQueue _asyncQueue;
		public CloudAsyncWritingQueue asyncQueue 
		{
			get
			{
				if(_asyncQueue == null)
					_asyncQueue = new CloudAsyncWritingQueue(this);

				return _asyncQueue;
			}
		}

		//

		public T LoadFile<T>(string fileName, out bool found)
		{
			found = false;

			try
			{
				var ret = LoadFile(fileName);

				found = true;

				if(ret != null)
					return (T)ret;
			}
			catch(System.Exception e)
			{
				Debug.LogException(e);
			}

			return default(T);
		}

		public object LoadFile(string fileName)
		{
			if(cloudAPI == null || !cloudAPI.isAvailable)
				return null;
			
			byte[] fileBytes = cloudAPI.LoadFile(fileName);

			Debug.Log("Loaded " + fileName + " - " + fileBytes);

			if(fileBytes != null)
			{
				using(System.IO.MemoryStream ms = new System.IO.MemoryStream(fileBytes))
				{
					using(System.IO.BinaryReader br = new System.IO.BinaryReader(ms))
					{
						if(br.PeekChar() != -1)
						{
							DataType dataType = DetermineDataType(br.ReadInt32());

							var stream = br.BaseStream;

							Debug.Log("Loaded " + dataType);

							switch(dataType)
							{
								case DataType.Integer:
								return br.ReadInt32();

								case DataType.Long:
								return br.ReadInt64();

								case DataType.Float:
								return br.ReadSingle();

								case DataType.Double:
								return br.ReadDouble();

								case DataType.String:
									
								byte[] buffer = br.ReadBytes((int)(stream.Length - stream.Position));

								return System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);

								case DataType.Bool:
								return br.ReadBoolean();

								default:
								return br.ReadBytes((int)(stream.Length - stream.Position));
							}
						}
					}
				}
			}

			return null;
		}

		public bool SaveFile(string fileName, object data)
		{
			if(cloudAPI == null || !cloudAPI.isAvailable)
				return false;

			if(data == null)
			{
				Debug.LogError("Failed to SaveToFile CloudSyncedPlayerPrefs - data == null");
				return false;
			}

			DataType dataType = DetermineDataType(data);

			byte[] serialized = null;

			switch(dataType)
			{
				case DataType.Integer:
					serialized = System.BitConverter.GetBytes((int)data);
				break;

				case DataType.Long:
					serialized = System.BitConverter.GetBytes((long)data);
				break;

				case DataType.Float:
					serialized = System.BitConverter.GetBytes((float)data);
				break;

				case DataType.Double:
					serialized = System.BitConverter.GetBytes((double)data);
				break;

				case DataType.String:
					serialized = System.Text.Encoding.UTF8.GetBytes(data.ToString());
				break;

				case DataType.Bool:
					serialized = System.BitConverter.GetBytes((bool)data);
				break;
			}

			//Debug.Log("Saved " + dataType + " in " + serialized);
					
			if(serialized == null)
			{
				Debug.LogError("Failed to serialize CloudAPI - data type " + data + " not recognized");
				return false;
			}

			serialized = ProcessData(dataType, serialized);

			return cloudAPI.SaveFile(fileName, serialized);	
		}

		//

		protected DataType DetermineDataType(int dataType)
		{
			try
			{
				return (DataType)dataType;
			}
			catch
			{
			}

			return DataType.Bytes;
		}

		protected DataType DetermineDataType(object data)
		{
			if(data is int)
			{
				return DataType.Integer;
			}
			else if(data is long)
			{
				return DataType.Long;
			}
			else if(data is float)
			{
				return DataType.Float;
			}
			else if(data is double)
			{
				return DataType.Double;
			}
			else if(data is string)
			{
				return DataType.String;
			}
			else if(data is bool)
			{
				return DataType.Bool;
			}

			return DataType.Bytes;
		}

		public byte[] ProcessData(DataType dataType, byte[] data)
		{
			if(data == null)
				return null;

			byte[] outputData = null;

			using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
			{
				using(System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms))
				{
					bw.Write((int)dataType);
					bw.Write(data);

					outputData = ms.ToArray();
				}
			}

			return outputData;
		}

		//

		public void DispatchAsyncQueue()
		{
			asyncQueue.Dispatch();
		}

		//

		private void OnApplicationQuit() 
		{
			DispatchAsyncQueue();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			DispatchAsyncQueue();
		}
	}
}
