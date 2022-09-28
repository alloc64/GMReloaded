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

namespace GMReloaded.UI.Final.Charts.SteamCharts
{
	public class KBSteamCharts : GMReloaded.UI.Final.Tabs.KBTab
	{
		[SerializeField]
		private KBChartsTableRow chartsTableRowTemplate;

		[SerializeField]
		private Transform chartsTableRowsContainer;

		[SerializeField]
		private UIInfiniteLoadingIndicator loadingIndicator;

		//

		private List<KBChartsTableRow> chartsTableRows = new List<KBChartsTableRow>();


		#if STEAM_ENABLED

		private PrefabsRecyclerBase<KBChartsTableRow> _recycler;
		private PrefabsRecyclerBase<KBChartsTableRow> recycler
		{
			get
			{ 
				if(_recycler == null)
				{
					_recycler = new PrefabsRecyclerBase<KBChartsTableRow>(chartsTableRowTemplate, chartsTableRowsContainer);
					_recycler.Preinstantiate(20);
				}

				return _recycler;
			}
		}

		//

		protected override void Awake()
		{
			base.Awake();

			if(chartsTableRowTemplate != null)
				chartsTableRowTemplate.SetActive(false);
		}

		public override void Show()
		{
			base.Show();

			GMReloaded.Steam.Steamworks.Instance.FindLeaderboard(Config.Steam.leaderboardId, (bool result, Steamworks.SteamLeaderboard_t leaderboard, HashSet<Steamworks.LeaderboardEntry_t> entries) => 
			{
				if(result)
				{
					foreach(var item in chartsTableRows)
						if(item != null)
							recycler.Enqueue(item);

					chartsTableRows.Clear();

					float offset = 0;
					int pos = 1;

					foreach(var entry in entries)
					{
						if(pos < 10)
						{
							var chartTableRow = recycler.Dequeue();
							chartTableRow.ResetTransforms();
							chartTableRow.SetYOffset(offset);
							chartTableRow.Setup(pos, entry);

							offset -= 0.095f;
							pos++;

							chartsTableRows.Add(chartTableRow);
						}

					}

					loadingIndicator.SetActive(false);
				}
			});
		}

		#endif
	}
}
