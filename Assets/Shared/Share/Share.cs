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
using System.Runtime.InteropServices;

namespace Railee
{
	public class Sharer
	{

#if UNITY_ANDROID && !UNITY_EDITOR
		private const string shareClasspath = "com.touchorchestra.android.Share";

		private static AndroidJavaObject _shareInstance;
		private static AndroidJavaObject shareInstance
		{
			get
			{
				if(_shareInstance == null)
					_shareInstance = new AndroidJavaObject(shareClasspath);

				return _shareInstance;
			}
		}
#endif

#if UNITY_IPHONE
		[DllImport("__Internal")]
		private static extern void _Share (string text, System.IntPtr imageBytePtr, int length);
#endif
		public static void Share(string text, byte [] imageByte = null)
		{
			string defaultFBURL = "https://www.facebook.com/sharer/sharer.php?u=" + WWW.EscapeURL(text);

#if UNITY_IPHONE && !UNITY_EDITOR

			if(imageByte == null)
			{
				_Share(text, System.IntPtr.Zero, 0);
			}
			else
			{
				GCHandle handle = GCHandle.Alloc(imageByte, GCHandleType.Pinned);

			_Share(text, handle.AddrOfPinnedObject(), imageByte.Length);

				handle.Free();
			}

#elif UNITY_ANDROID && !UNITY_EDITOR
			if(shareInstance != null)
			{
				shareInstance.Call("share", new object[] 
				{
					"",
					text,
					"Share",
					imageByte
				});
			}
#elif UNITY_WP8 && !UNITY_EDITOR

			TouchOrchestra.Sharer.Share(imageByte);

#elif UNITY_WEBPLAYER && !UNITY_EDITOR
			Application.ExternalCall("window.open('" + defaultFBURL + "', '_parent')");
#else 
			Application.OpenURL(defaultFBURL);
#endif
		}
	}
}
