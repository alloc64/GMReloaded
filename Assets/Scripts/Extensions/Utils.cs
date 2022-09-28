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

public class Utils  
{
	private static System.Security.Cryptography.MD5 md5;

	static Utils()
	{
		md5 = System.Security.Cryptography.MD5.Create();
	}

	public static void Initialize()
	{

	}

	public static void OpenURL(string url)
	{
		#if (UNITY_WEBPLAYER || UNITY_WEBGL) && !UNITY_EDITOR

		Application.ExternalEval("window.parent.top.location = '" + url + "';");

		#else

		Application.OpenURL(url);

		#endif
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		if (min < 0 && max > 0 && (angle > max || angle < min))
		{
			angle -= 360;
			if (angle > max || angle < min)
			{
				if (Mathf.Abs(Mathf.DeltaAngle(angle, min)) < Mathf.Abs(Mathf.DeltaAngle(angle, max))) return min;
				else return max;
			}
		}
		else if(min > 0 && (angle > max || angle < min))
		{
			angle += 360;
			if (angle > max || angle < min)
			{
				if (Mathf.Abs(Mathf.DeltaAngle(angle, min)) < Mathf.Abs(Mathf.DeltaAngle(angle, max))) return min;
				else return max;
			}
		}

		if (angle < min) return min;
		else if (angle > max) return max;
		else return angle;
	}

	public static string MD5(string s)
	{
		byte[] data = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(s));

		System.Text.StringBuilder sb = new System.Text.StringBuilder();

		for (int i = 0; i < data.Length; ++i)
			sb.Append(data[i].ToString("x2"));
		
		return sb.ToString();
	}

	public static long GetUnixTimestamp()
	{
		var timeSpan = (System.DateTime.UtcNow - new System.DateTime(1970, 1, 1, 0, 0, 0));
		return (long)timeSpan.TotalSeconds;
	}


	public static T CloneItem<T>(MonoBehaviour @object, Transform parent) where T : MonoBehaviourTO
	{
		var go = GameObject.Instantiate(@object);

		if(go == null)
			return null;

		T c = go.GetComponent<T>();

		if(c == null)
			return null;

		c.name = c.name.Replace("(Clone)", "");
		c.transform.parent = parent;
		c.transform.localPosition = Vector3.zero;
		c.transform.localRotation = Quaternion.identity;
		c.transform.localScale = Vector3.one;

		c.SetActive(true);

		return c;
	}
}
