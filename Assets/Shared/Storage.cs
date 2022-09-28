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
using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Collections.Generic;

public class Storage
{
	internal enum ContentType
	{
		Float,
		Int32,
		Bool,
		String
	}

	public static bool HasKey(string key)
	{
		return PlayerPrefs.HasKey(GetKey(key));
	}

	/// Bool

	public static bool GetBool(string key, bool defaultValue = false)
	{
		return GetValueTypeValue<int>(key, defaultValue ? 1 : 0) == 1;
	}

	public static bool SetBool(string key, bool value)
	{
		return SetValue(key, value ? 1 : 0);
	}

	/// Int

	public static int GetInt(string key, int defaultValue = 0)
	{
		return GetValueTypeValue<int>(key, defaultValue);
	}

	public static bool SetInt(string key, int value)
	{
		return SetValue(key, value);
	}

	/// Float

	public static float GetFloat(string key, float defaultValue = 0f)
	{
		return GetValueTypeValue<float>(key, defaultValue);
	}

	public static bool SetFloat(string key, float value)
	{
		return SetValue(key, value);
	}

	/// String

	public static string GetString(string key, string defaultValue = "")
	{
		return GetRefTypeValue<string>(key, defaultValue);
	}

	public static bool SetString(string key, string value)
	{
		return SetValue(key, value);
	}

	internal static bool SetValue(string key, object content)
	{
		byte[] contentBytes = null;
		ContentType contentType;

		if(content is bool)
		{
			contentBytes = BitConverter.GetBytes((bool)content);
			contentType = ContentType.Bool;
		}
		if(content is int)
		{
			contentBytes = BitConverter.GetBytes((int)content);
			contentType = ContentType.Int32;
		}
		else
		if(content is float || content is double)
		{
			contentBytes = BitConverter.GetBytes((float)content);
			contentType = ContentType.Float;
		}
		else
		if(content is string)
		{
			contentBytes = System.Text.Encoding.UTF8.GetBytes((string)content);
			contentType = ContentType.String;
		}
		else
		{
			Debug.Log("Unsupported storage SetValue type: " + content.GetType() + " / " + content);
			return false;
		}
	
		byte[] bytes = new byte[contentBytes.Length + 1];
		bytes[0] = System.Convert.ToByte(contentType);	

		contentBytes.CopyTo(bytes, 1);

		return SaveBytes(key, bytes);
	}

	internal static Dictionary<string, string> keyCache = new Dictionary<string, string>();

	internal static string GetKey(string key)
	{
		string encryptedKey = null;

		if(!keyCache.TryGetValue(key, out encryptedKey))
		{
			encryptedKey = MD5(key);
			keyCache[key] = encryptedKey;
		}

		return encryptedKey;
	}

	internal static T GetValueTypeValue<T>(string key, T defaultValue) where T : struct
	{		
		object obj = GetObject(key);

		if(obj == null)
			return defaultValue;

		return (T)obj;	
	}

	internal static T GetRefTypeValue<T>(string key, T defaultValue) where T : class
	{		
		object obj = GetObject(key);

		if(obj == null)
			return defaultValue;

		return (T)obj;	
	}

	internal static object GetObject(string key)
	{
		if(VIKey == VIKey2)
			throw new AccessViolationException();

		if(!PlayerPrefs.HasKey(GetKey(key)))
			return null;

		try
		{
			string str = PlayerPrefs.GetString(GetKey(key));

			byte[] bytes = Decrypt(str);

			if(bytes.Length < 2)
				return null;

			ContentType contentType = (ContentType)bytes[0];

			switch(contentType)
			{
				case ContentType.Float:
				return System.BitConverter.ToSingle(bytes, 1); 

				case ContentType.Int32:
				return System.BitConverter.ToInt32(bytes, 1); 

				case ContentType.Bool:
				return System.BitConverter.ToBoolean(bytes, 1); 

				case ContentType.String:
				return System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length); 

				default:
					Debug.Log("Unsupported storage GetValue type: " + contentType);

				return null;
			}

		}
		catch
		{
			return null;
		}
	}

	internal static bool SaveBytes(string key, byte[] bytes)
	{
		if(VIKey == VIKey2)
			throw new AccessViolationException();

		try
		{
			PlayerPrefs.SetString(GetKey(key), System.Convert.ToBase64String(Encrypt(bytes)));
		}
		catch(Exception e)
		{
			Debug.Log(e);

			return false;
		}
		return true;
	}

	public static string PasswordHash = "yourkey1";
	public static string SaltKey = "yourkey2";

	/// <summary>
	/// The VI key - 16Bytes
	/// </summary>
	public static string VIKey = "$ę€žčTREFě3wERžř";
	public static string VIKey2 = "$ę€žčTREFě3wERžř";

	internal static byte[] keyBytes;
#if !UNITY_WEBPLAYER
	internal static AesManaged symmetricKey;
#endif
#if !UNITY_WP8
	internal static MD5CryptoServiceProvider md5;
#endif

	public static void Init(string p, string s, string v)
	{
		PasswordHash = p;
		SaltKey = s;
		VIKey = v;

#if !UNITY_WEBPLAYER
		keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
		symmetricKey = new AesManaged();
#endif
#if !UNITY_WP8
		md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
#endif
	}

	internal static byte[] Encrypt(byte[] bytes)
	{
#if UNITY_WEBPLAYER
		return bytes;
#else
		byte[] cipherTextBytes;

		using(var memoryStream = new MemoryStream())
		{
			using(var cryptoStream = new CryptoStream(memoryStream, symmetricKey.CreateEncryptor(keyBytes, Encoding.UTF8.GetBytes(VIKey)), CryptoStreamMode.Write))
			{
				cryptoStream.Write(bytes, 0, bytes.Length);
				cryptoStream.FlushFinalBlock();
				cipherTextBytes = memoryStream.ToArray();
				cryptoStream.Close();
			}
			memoryStream.Close();
		}

		return cipherTextBytes;
#endif
	}

	internal static byte[] Decrypt(string encryptedText)
	{
		byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);
#if UNITY_WEBPLAYER
		return cipherTextBytes;
#else
		var memoryStream = new MemoryStream(cipherTextBytes);
		var cryptoStream = new CryptoStream(memoryStream, symmetricKey.CreateDecryptor(keyBytes, Encoding.UTF8.GetBytes(VIKey)), CryptoStreamMode.Read);
		byte[] plainTextBytes = new byte[cipherTextBytes.Length];

		cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
		memoryStream.Close();
		cryptoStream.Close();

		return plainTextBytes;
#endif
	}

	internal static string MD5(string str)
	{
		byte[] bytes = System.Text.Encoding.UTF8.GetBytes(SaltKey + str + VIKey);
#if UNITY_WP8
		byte[] hashBytes = UnityEngine.Windows.Crypto.ComputeMD5Hash(bytes);
#else
		byte[] hashBytes = md5.ComputeHash(bytes);
#endif
		string hashString = "";

		for(int i = 0; i < hashBytes.Length; i++)
			hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');

		return hashString.PadLeft(32, '0');
	}
}

