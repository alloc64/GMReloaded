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
using TouchOrchestra;
using System;
using System.Collections.Generic;

namespace GMReloaded.API
{
	public class Leaderboards : MonoSingletonPersistent<Leaderboards> 
	{
		public interface ILeaderboardsResponse
		{
			void OnLeaderboardDataReceived(List<Item> items);
		}

		public class Item
		{
			public string nick;
			public int kills;
			public int deaths;
			public float kdratio;

			public Item(JSONObject json)
			{
				var obj = json.GetField("nick");

				if(obj != null)
					this.nick = obj.str;

				kills = json.GetFieldi("kills");

				deaths = json.GetFieldi("deaths");

				kdratio = json.GetFieldf("kdratio");
			}

			public override string ToString()
			{
				return string.Format("[Leaderboards.Item: nick={0}, kills={1}, deaths={2}, kdratio={3}]", nick, kills, deaths, kdratio);
			}
		}

		private LeaderboardsObserver _leaderboards = null;
		private LeaderboardsObserver leaderboards
		{
			get
			{ 
				if(_leaderboards == null)
					_leaderboards = new LeaderboardsObserver();

				return _leaderboards;
			}
		}

		[SerializeField]
		private float timeBetweenRequests = 120f;

		private List<Item> leaderboardItems = null;
		private long lastReceivedDataTimestamp = 0;

		private HashSet<ILeaderboardsResponse> listeners = new HashSet<ILeaderboardsResponse>();

		public bool RequestLeaderboard(ILeaderboardsResponse listener)
		{
			if(Utils.GetUnixTimestamp() - lastReceivedDataTimestamp > timeBetweenRequests)
			{
				leaderboards.Dispatch((items) =>
				{
					this.leaderboardItems = items;
					lastReceivedDataTimestamp = Utils.GetUnixTimestamp();

					if(listener != null)
						listener.OnLeaderboardDataReceived(leaderboardItems);
				});

				return true;
			}
			else
			{
				if(listener != null)
					listener.OnLeaderboardDataReceived(leaderboardItems);
			}

			return false;
		}
	}
}
