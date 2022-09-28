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
using CodeStage.AntiCheat.ObscuredTypes;
using System;

namespace GMReloaded.Cloud
{
	public class CloudAsyncWritingQueue
	{
		private Queue<KeyValuePair<string, object>> queue = new Queue<KeyValuePair<string, object>>();

		private CloudAPI cloud;

		private bool beingDispatched = false;

		//

		public CloudAsyncWritingQueue(CloudAPI cloudAPI)
		{
			this.cloud = cloudAPI;
		}

		//

		public void Enqueue(string key, object value)
		{
			queue.Enqueue(new KeyValuePair<string, object>(key, value));

			if(queue.Count >= 5)
			{
				Dispatch();
			}
		}

		public void Dispatch()
		{
			if(queue.Count < 1 || beingDispatched)
				return;

			new System.Threading.Thread(DoWork).Start();

			beingDispatched = true;
		}

		//

		private void DoWork()
		{
			if(cloud == null)
				return;

			Debug.Log("Trying to write " + queue.Count + " items");

			do 
			{
				var kvp = queue.Dequeue();

				Debug.Log("Writing file " + kvp.Key + " with value " + kvp.Value + " to cloud...");

				string key = kvp.Key;
				object value = kvp.Value;

				if(value is int)
				{
					SaveToCloud(key, (int)value);
				}
				else if(value is long)
				{
					SaveToCloud(key, (long)value);
				}
				else if(value is float)
				{
					SaveToCloud(key, (float)value);
				}
				else if(value is bool)
				{
					SaveToCloud(key, (bool)value);
				}
				else if(value is string)
				{
					SaveToCloud(key, (string)value);
				}
				else
				{
					SaveToCloudNoCheck(key, value);
				}
			}
			while (queue.Count > 0);

			beingDispatched = false;
		}

		//

		private T LoadFromCloud<T>(string fileName, out bool found)
		{
			return cloud.LoadFile<T>(fileName, out found);
		}

		private bool SaveToCloud(string fileName, int data)
		{
			bool found = false;
			var value = LoadFromCloud<int>(fileName, out found);

			if(found && data < value)
			{
				Debug.Log("Ignoring push to cloud. Value is smaller than in cloud: " + data + " < " + value);
				return false;
			}

			return SaveToCloudNoCheck(fileName, data);
		}

		private bool SaveToCloud(string fileName, long data)
		{
			bool found = false;
			var value = LoadFromCloud<long>(fileName, out found);

			if(found && data < value)
			{
				Debug.Log("Ignoring push to cloud. Value is smaller than in cloud: " + data + " < " + value);
				return false;
			}

			return SaveToCloudNoCheck(fileName, data);
		}

		private bool SaveToCloud(string fileName, float data)
		{
			bool found = false;
			var value = LoadFromCloud<float>(fileName, out found);

			if(found && data < value)
			{
				Debug.Log("Ignoring push to cloud. Value is smaller than in cloud: " + data + " < " + value);
				return false;
			}

			return SaveToCloudNoCheck(fileName, data);
		}

		//
	
		private bool SaveToCloud(string fileName, bool data) { return SaveToCloudNoCheck(fileName, data); }

		private bool SaveToCloud(string fileName, string data) { return SaveToCloudNoCheck(fileName, data); }

		private bool SaveToCloudNoCheck(string fileName, object data) { return cloud.SaveFile(fileName, data); }

	}
	
}
