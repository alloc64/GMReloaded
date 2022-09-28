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
using System.Collections.Generic;
#if UNITY_XBOXONE
using GMReloaded.Xbox.Linq;
#else
using System.Linq;
#endif

using System;
using TouchOrchestra;

namespace GMReloaded.API
{
	public class LeaderboardsObserver 
	{
		public void Dispatch(Action<List<Leaderboards.Item>> OnDispatched)
		{
			SessionAPIObserver observer = new SessionAPIObserver
			(
				"leaderboards", 
				(r) => OnResponse(r, OnDispatched),
				(r) => OnError(r, OnDispatched)
			);

			observer.Run();
		}

		private void OnResponse(JSONServerResponse response, Action<List<Leaderboards.Item>> OnDispatched)
		{
			List<Leaderboards.Item> items = null;

			if(response.responseCode == 200)
			{
				items = new List<Leaderboards.Item>();

				foreach(var i in response.jsonPayload.list)
				{
					JSONObject json = i as JSONObject;

					if(json != null)
					{
						items.Add(new Leaderboards.Item(json));
					}
				}

				#if UNITY_XBOXONE
				items.Sort((i0, i1) => i1.kdratio.CompareTo(i0.kdratio));
				#else
				items = items.OrderByDescending((i) => i.kdratio).ToList();
				#endif
			}

			if(OnDispatched != null)
				OnDispatched(items);
		}

		private void OnError(JSONServerResponse response, Action<List<Leaderboards.Item>> OnDispatched)
		{
			Debug.LogWarning("An error ocured while getting leaderboards data " + response.payload);

			if(OnDispatched != null)
				OnDispatched(null);
		}
	}
}