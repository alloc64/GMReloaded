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
using GMReloaded.UI;
using TouchOrchestra;

namespace GMReloaded.API
{
	public class LoginObserver
	{
		private Login parent;

		public LoginObserver(Login parent)
		{
			this.parent = parent;
		}

		public void StartLogin()
		{
			parent.OnLoginStarted();

			var observer = new JSONAPIObserver
			(
				"login", 
				OnResponse, 
				OnError
			);

			observer.SetPayload(LocalClientRobotEmil.loginCredentials);
			observer.Run();
		}

		public void LogEvent(string type, string action)
		{
			var observer = new JSONAPIObserver
			(
				"acc_evt", 
				(r) => { Debug.Log("Logged account event " + type + " / " + action); }, 
				(r) => { Debug.Log("Failed to log account event " + type + " / " + action); }
			);

			observer.SetPayload
			(
				new Dictionary<string, string> 
				{ 
					{ "type", type }, 
					{ "action", action } 
				}
			);
			observer.Run();
		}

		private void OnResponse(JSONServerResponse response)
		{
			if(response.responseCode == 200)
			{
				var obj = response.jsonPayload.GetField("token");

				if(obj != null)
					JSONAPIObserver.SetSessionToken(obj.str);

				parent._OnLoggedIn(response);
			}
			else
			{
				Analytics.GAI.Instance.LogEvent("API", "Login", "Failed " + response, 1);
				parent._OnLoginFailed(response);
			}
		}

		private void OnError(JSONServerResponse response)
		{
			Analytics.GAI.Instance.LogEvent("API", "Login", "Failed " + response, 1);
			parent._OnLoginFailed(response);
		}
	}
}