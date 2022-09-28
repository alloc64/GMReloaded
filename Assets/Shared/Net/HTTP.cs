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

using System;
using System.Collections;
using System.Collections.Generic;
using Independent;
using UnityEngine;
using System.Text;

namespace TouchOrchestra.Net
{

	public class HTTP
	{
		public void GET(string url, Action<WWW> OnResponse, Action<WWW> OnError)
		{
			WWW www = new WWW(url);
			Independent.Coroutine.GetInstance().ProcessCoroutine(WaitForRequest(www, OnResponse, OnError));
		}

		public void POST(string url, Dictionary<string, string> post, Action<WWW> OnResponse, Action<WWW> OnError)
		{
			WWWForm form = new WWWForm();
			foreach(KeyValuePair<string,string> postArg in post)
			{
				form.AddField(postArg.Key, postArg.Value);
			}

			WWW www = new WWW(url, form);

			Independent.Coroutine.GetInstance().ProcessCoroutine(WaitForRequest(www, OnResponse, OnError));
		}

		protected virtual IEnumerator WaitForRequest(WWW www, Action<WWW> OnResponse, Action<WWW> OnError)
		{
			yield return www;

			if(www.error == null)
			{
				if(OnResponse != null)
				{
					OnResponse(www);
				}
			}
			else
			{
				Debug.Log(www.url + "\n" + www.error);

				if(OnError != null)
				{
					OnError(www);
				}
			}

		}
	}
}