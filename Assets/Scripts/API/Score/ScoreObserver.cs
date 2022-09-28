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
using TouchOrchestra;

namespace GMReloaded.API
{
	public class ScoreObserver 
	{
		private Dictionary<string, float> properties = new Dictionary<string, float>();

		private long lastUpdateTimestamp;

		public void Init()
		{
			lastUpdateTimestamp = Utils.GetUnixTimestamp();
		}

		public void Dispatch(bool ignoreSession = false)
		{
			if(properties.Count < 1)
				return;

			LogAbsoluteValue("playTime", Utils.GetUnixTimestamp() - lastUpdateTimestamp);
			lastUpdateTimestamp = Utils.GetUnixTimestamp();
				
			JSONObject json = new JSONObject(properties);

			properties.Clear();

			SessionAPIObserver observer = new SessionAPIObserver
			(
				"score", 
				OnResponse,
				OnError
			);

			var payload = new Dictionary<string, string>();
			payload.Add("json", json.ToString());

			observer.SetPayload(payload);
			observer.IgnoreSessionTemporary(ignoreSession);
			observer.Run();
		}

		private void OnResponse(JSONServerResponse response)
		{
			if(response.responseCode == 200)
				OnScoreSaved();
			else
				OnScoreSaveFailed(response);
		}

		private void OnError(JSONServerResponse response)
		{ 
			OnScoreSaveFailed(response);
		}

		private void OnScoreSaved()
		{
			Debug.Log("Score successfully saved...");
		}

		private void OnScoreSaveFailed(JSONServerResponse response)
		{
			Debug.Log("Score save failed:" + response);
		}


		public void LogAbsoluteValue(string id, float value)
		{
			if(string.IsNullOrEmpty(id))
			{
				Debug.LogError("LogAbsoluteValue id NULL");
				return;
			}

			properties[id] = value;
		}

		public void LogRelativeValue(string id, float value)
		{
			float previousVal = 0f;
			properties.TryGetValue(id, out previousVal);

			LogAbsoluteValue(id, previousVal + value);
		}
	}
}
