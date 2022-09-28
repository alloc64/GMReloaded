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
using TouchOrchestra.Net;
using System.Collections.Generic;
using System;

namespace GMReloaded.API
{
	public class JSONAPIObserver
	{
		public enum Method
		{
			GET,
			POST
		}

		private GMHTTP http = new GMHTTP();

		private Dictionary<string, string> payload = new Dictionary<string, string>();

		private string entryPoint;

		private Action<JSONServerResponse> OnSuccess, OnError;

		private bool requiresLogin;

		protected static long lastRequestTimestamp;

		private static string token;

		private Method requestMethod = Method.GET;

		protected string url
		{
			get
			{
				string _url = Config.apiURL + "?c=" + entryPoint;

				if(!string.IsNullOrEmpty(token))
					payload["token"] = token;

				if(requestMethod == Method.GET)
				{
					int i = 0;
					foreach(var kvp in payload)
					{
						if(i < payload.Count)
							_url += "&";

						_url += WWW.EscapeURL(kvp.Key) + "=" + WWW.EscapeURL(kvp.Value);

						i++;
					}
				}

				return _url;
			}
		}

		public JSONAPIObserver(string entryPoint, Action<JSONServerResponse> OnSuccess, Action<JSONServerResponse> OnError)
		{
			this.entryPoint = entryPoint;
			this.OnSuccess = OnSuccess;
			this.OnError = OnError;
		}

		public void SetRequestMethod(Method requestMethod)
		{
			this.requestMethod = requestMethod;
		}

		public void SetPayload(Dictionary<string, string> payload)
		{
			this.payload = payload;
		}

		public virtual void Run()
		{
			ProcessRequest(url);
		}

		protected virtual void ProcessRequest(string url)
		{
			#if UNITY_EDITOR
			Debug.Log("Executing request " + url);
			#endif

			switch(requestMethod)
			{
				case Method.POST:
					http.POST
					(
						url, 
						payload,
						(www) => _OnSuccess(new JSONServerResponse(www.text)), 
						(www) => _OnError(new JSONServerResponse(www.error))
					);
				break;
					
				case Method.GET:
				default:
					http.GET
					(
						url, 
						(www) => _OnSuccess(new JSONServerResponse(www.text)), 
						(www) => _OnError(new JSONServerResponse(www.error))
					);
				break;
			}
		}

		protected void _OnSuccess(JSONServerResponse response)
		{
			if(OnSuccess != null) 
				OnSuccess(response);

			lastRequestTimestamp = Utils.GetUnixTimestamp();
		}

		protected void _OnError(JSONServerResponse response)
		{
			if(OnError != null) 
				OnError(response); 
		}

		public static void SetSessionToken(string _token)
		{
			token = _token;
		}
	}
	
}