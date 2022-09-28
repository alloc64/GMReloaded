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

namespace TouchOrchestra
{
	public class NativeAppOpen : MonoSingleton<NativeAppOpen>
	{
		private bool appLeft = false;

		public void OpenURL(string url)
		{
			if(string.IsNullOrEmpty(url))
				return;

			string [] split = url.Split('/');

			if(split == null || split.Length < 1)
				return;

			string id = split[split.Length-1];

			if(url.StartsWith("fb://"))
			{
				StartCoroutine(OpenFacebook(id));
			}
			else if(url.StartsWith("twitter:///"))
			{
				StartCoroutine(OpenTwitter(id));
			}
			else
			{
				Application.OpenURL(url);
			}
		}

		void OnApplicationPause(bool inIsPause)
		{
			this.appLeft = true;
		}

		IEnumerator OpenFacebook(string id)
		{
			if(!CheckPackageAppIsPresent("com.facebook.katana"))
			{
				Application.OpenURL("fb://profile/" + id);//replace #'s w/ fb profile id
				yield return new WaitForSeconds(1f);
			}

			if (this.appLeft)
				this.appLeft = false;
			else
				Application.OpenURL("http://www.facebook.com/" + id);//replace mypage
		}

		IEnumerator OpenTwitter(string id)
		{
			//Application.OpenURL("twitter://" + id);//repace username
			//yield return new WaitForSeconds(1f);

			//if (this.appLeft)
			//	this.appLeft = false;
			//else

			// nejspis unsupported, protoze nefunguje ani na jedno device

			Application.OpenURL("http://www.twitter.com/" + id);//replace username
			yield return null;
		}

		private bool CheckPackageAppIsPresent(string package)
		{
#if UNITY_ANDROID && !UNITY_EDITOR
			AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject ca = up.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager");

			//take the list of all packages on the device
			AndroidJavaObject appList = packageManager.Call<AndroidJavaObject>("getInstalledPackages",0);
			int num = appList.Call<int>("size");
			for(int i = 0; i < num; i++)
			{
				AndroidJavaObject appInfo = appList.Call<AndroidJavaObject>("get", i);
				string packageNew = appInfo.Get<string>("packageName");
				if(packageNew.CompareTo(package) == 0)
				{
					return true;
				}
			}
			return false;
#else
			return true;
#endif
		}
	}
}
