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
	public class JSONServerResponse
	{
		//

		private int _responseCode = 500;
		public int responseCode { get { return _responseCode; } private set { _responseCode = value; } }

		public String payload { get; private set; }

		public JSONObject jsonPayload { get; private set; }

		//

		public JSONServerResponse(string response)
		{
			if(!string.IsNullOrEmpty(response))
			{
				JSONObject json = new JSONObject(response);

				var code = json.GetFieldi("code");

				if(code > 0)
					responseCode = code;

				var obj = json.GetField("payload");

				if(obj != null)
					jsonPayload = obj;
				else
					payload = response;
			}
		}

		public override string ToString()
		{
			return string.Format("[ServerResponse: payload={0}, jsonPayload={1}, responseCode={2}]", payload, jsonPayload, responseCode);
		}
	}
	
}