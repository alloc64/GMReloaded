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
	public class SessionAPIObserver : JSONAPIObserver
	{
		private bool ignoreSession = false;

		public SessionAPIObserver(string entryPoint, Action<JSONServerResponse> OnSuccess, Action<JSONServerResponse> OnError) : base(entryPoint, OnSuccess, OnError)
		{
			
		}

		public override void Run()
		{
			if(ignoreSession || Utils.GetUnixTimestamp() - lastRequestTimestamp < 600)
			{
				ProcessRequest(url);
				IgnoreSessionTemporary(false);
			}
			else
			{
				var validatorObserver = new JSONAPIObserver
				(
					"validSession", 
					(response) =>
					{
						if(response.responseCode == 200)
							ProcessRequest(url);
						else
							OnSessionExpired(response);
					}, 
					_OnError
				);

				validatorObserver.Run();
			}
		}

		private void OnSessionExpired(JSONServerResponse response)
		{
			API.Login.Instance.StartLogin();
		}

		public void IgnoreSessionTemporary(bool ignored)
		{
			this.ignoreSession = ignored;
		}
	}
	
}