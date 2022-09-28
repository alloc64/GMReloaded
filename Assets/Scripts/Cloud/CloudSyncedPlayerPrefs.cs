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
	public class CloudSyncedPlayerPrefs
	{
		private static CloudAPI cloud { get { return CloudAPI.Instance; } }

		//

		public static bool HasKey(string key) { return ObscuredPrefs.HasKey(key); }

		//

		public static void SetInt(string key, int value)
		{
			SaveToCloudAsync(key, value);
			ObscuredPrefs.SetInt(key, value);
		}

		public static int GetInt(string key, int defaultValue = 0)
		{
			return ResolveValidNumericValue(key, ObscuredPrefs.GetInt(key, defaultValue), (resolvedValue) => ObscuredPrefs.SetInt(key, resolvedValue));
		}

		//

		public static void SetLong(string key, long value)
		{
			SaveToCloudAsync(key, value);
			ObscuredPrefs.SetLong(key, value);
		}

		public static long GetLong(string key, long defaultValue = 0)
		{
			return ResolveValidNumericValue(key, ObscuredPrefs.GetLong(key, defaultValue), (resolvedValue) => ObscuredPrefs.SetLong(key, resolvedValue));
		}

		//

		public static void SetFloat(string key, float value)
		{
			SaveToCloudAsync(key, value);
			ObscuredPrefs.SetFloat(key, value);
		}

		public static float GetFloat(string key, float defaultValue = 0f)
		{
			return ResolveValidNumericValue(key, ObscuredPrefs.GetFloat(key, defaultValue), (resolvedValue) => ObscuredPrefs.SetFloat(key, resolvedValue));
		}

		//

		public static void SetString(string key, string value)
		{
			SaveToCloudAsync(key, value);
			ObscuredPrefs.SetString(key, value);
		}

		public static string GetString(string key, string defaultValue = "")
		{
			return ResolveValidNonComparableValue(key, ObscuredPrefs.GetString(key, defaultValue), (resolvedValue) => ObscuredPrefs.SetString(key, resolvedValue));
		}

		//

		public static void SetBool(string key, bool value)
		{
			SaveToCloudAsync(key, value);
			ObscuredPrefs.SetBool(key, value);
		}

		public static bool GetBool(string key, bool defaultValue = false)
		{
			return ResolveValidNumericValue(key, ObscuredPrefs.GetBool(key, defaultValue), (resolvedValue) => ObscuredPrefs.SetBool(key, resolvedValue));
		}

		//

		private static T ResolveValidNumericValue<T>(string key, T localValue, System.Action<T> OnSolved) where T : IComparable<T>
		{
			bool found = false;
			var cloudValue = LoadFromCloud<T>(key, out found);

			// existuje hodnota v cloudu?
			if(found)
			{
				Debug.Log("ResolveValidNumericValue " + key + " - " + localValue + " / " + cloudValue);

				if(localValue.CompareTo(cloudValue) != 0)
				{
					// pokud je lokalni hodnota vetsi nez v cloudu, provedu update do cloudu
					if(localValue.CompareTo(cloudValue) > 0)
					{
						SaveToCloudSync(key, localValue);
					}
					else
					{
						// pokud je hodnota v cloudu vetsi nez lokalni, ulozim ji do playerprefs a pouzivam jako svoji vlastni
						localValue = cloudValue;
					}

					if(OnSolved != null)
						OnSolved(localValue);
				}
			}

			return localValue;
		}

		//

		private static T ResolveValidNonComparableValue<T>(string key, T localValue, System.Action<T> OnSolved)
		{
			bool found = false;
			var cloudValue = LoadFromCloud<T>(key, out found);

			if(found)
			{
				localValue = cloudValue;

				if(OnSolved != null)
					OnSolved(localValue);
			}

			return localValue;
		}

		//

		private static T LoadFromCloud<T>(string fileName, out bool found)
		{
			return cloud.LoadFile<T>(fileName, out found);
		}

		private static void SaveToCloudAsync(string key, object value)
		{
			cloud.asyncQueue.Enqueue(key, value);
		}

		private static void SaveToCloudSync(string key, object localValue)
		{
			cloud.SaveFile(key, localValue);
		}

		//

		#if UNITY_EDITOR
		public static void UnitTest()
		{

			string intKey = "test_int1";
			string longKey = "test_long1";
			string floatKey = "test_float1";
			string boolKey = "test_bool1";


			ObscuredPrefs.SetInt(intKey, -999);
			ObscuredPrefs.SetLong(longKey, -333333343333);
			ObscuredPrefs.SetFloat(floatKey, -9876.3323f);
			ObscuredPrefs.SetBool(boolKey, false);


			//

			Debug.Log(intKey + " value before: " + ObscuredPrefs.GetInt(intKey));

			Cloud.CloudSyncedPlayerPrefs.SetInt(intKey, 111);

			Debug.Log(intKey + " value after: " + Cloud.CloudSyncedPlayerPrefs.GetInt(intKey));

			//

			Debug.Log(longKey + " value before: " + ObscuredPrefs.GetLong(longKey));

			Cloud.CloudSyncedPlayerPrefs.SetLong(longKey, 999999);

			Debug.Log(longKey + " value after: " + Cloud.CloudSyncedPlayerPrefs.GetLong(longKey));

			//

			Debug.Log(floatKey + " value before: " + ObscuredPrefs.GetFloat(floatKey));

			Cloud.CloudSyncedPlayerPrefs.SetFloat(floatKey, 1234.3322f);

			Debug.Log(floatKey + " value after: " + Cloud.CloudSyncedPlayerPrefs.GetFloat(floatKey));

			//

			Debug.Log(boolKey + " value before: " + ObscuredPrefs.GetBool(boolKey));

			Cloud.CloudSyncedPlayerPrefs.SetBool(boolKey, true);

			Debug.Log(boolKey + " value after: " + Cloud.CloudSyncedPlayerPrefs.GetBool(boolKey));


		}
		#endif
	}
	
}
