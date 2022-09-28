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
#if UNITY_XBOXONE
using GMReloaded.Xbox.Linq;
#else
using System.Linq;
#endif

namespace GMReloaded
{
	public class PlayerStatsTableRowRecycler : PrefabsRecyclerBase<PlayerStatsTableRow>
	{
		public PlayerStatsTableRowRecycler(PlayerStatsTableRow basePrefab, Transform parent) : base(basePrefab, parent)
		{
			Preinstantiate(20);
			basePrefab.SetActive(false);
		}
	}

	public class PlayerStatsTable : MonoSingleton<PlayerStatsTable> 
	{
		[SerializeField]
		public PlayerStatsTableRow baseRow;

		[SerializeField]
		public Transform rowsContainer;

		[SerializeField]
		public GameObject content;

		[SerializeField]
		private tk2dTextMesh arenaTimeTextMesh;

		[SerializeField]
		private tk2dTextMesh serverNameTextMesh;

		[SerializeField]
		private tk2dTextMesh arenaNameTextMesh;

		[SerializeField]
		private TopGameTime topGameTime;

		[SerializeField]
		private MonoBehaviourTO [] deactivatedObjectsOnVisible;

		private PlayerStatsTableRowRecycler _recycler;
		private PlayerStatsTableRowRecycler recycler
		{
			get
			{ 
				if(_recycler == null)
				{
					_recycler = new PlayerStatsTableRowRecycler(baseRow, rowsContainer);
					baseRow.SetActive(false);
				}

				return _recycler;
			}
		}

		private Dictionary<PhotonPlayer, PlayerStatsTableRow> rows = new Dictionary<PhotonPlayer, PlayerStatsTableRow>();

		private void Awake()
		{
			var r = recycler;

			if(tutorial.isActive && serverNameTextMesh != null)
				serverNameTextMesh.SetActive(false);
		}

		public void AddPlayer(PhotonPlayer player)
		{
			//Debug.LogWarning("AddPlayer " + player);

			var row = recycler.Dequeue();

			if(row != null)
			{
				row.ResetTransforms();
				row.SetNick(player.name);

				rows[player] = row;
				SortRows();
			}
		}

		public void RemovePlayer(PhotonPlayer player)
		{
			var row = GetRow(player);

			if(row != null)
			{
				recycler.Enqueue(row);
				rows.Remove(player);

				SortRows();

				//Debug.LogWarning("RemovePlayer " + player);
			}
		}

		private PlayerStatsTableRow GetRow(PhotonPlayer player)
		{
			if(player == null)
				return null;

			PlayerStatsTableRow row = null;
			rows.TryGetValue(player, out row);

			return row;
		}

		public void UpdatePlayer(RobotEmilNetworked player)
		{
			if(player == null)
				return;
			
			var row = GetRow(player.photonPlayer);

			//Debug.LogWarning("UpdatePlayer " + player + " / " + row);

			if(row != null)
			{
				row.SetLocalPlayer(player.clientType == RobotEmil.ClientType.LocalClient);
				row.SetScore(player.score);
				row.SetKills(player.kills);
				row.SetDeaths(player.deaths);
				row.SetPing(player.ping);
			}

			SortRows();
		}

		public void UpdateChangedPlayerNick(PhotonPlayer player)
		{
			if(player == null)
				return;

			var row = GetRow(player);

			if(row != null)
			{
				row.SetNick(player.name);
			}
		}

		private void SortRows()
		{
			int counter = 1;
#if UNITY_XBOXONE
			foreach (var k in rows.PlayerStatsTable_OrderBy_AOT((i0, i1) => i1.Value.kills.CompareTo(i0.Value.kills)))
#else
			foreach (var k in rows.OrderByDescending(i => i.Value.kills))
#endif
			{
				var row = k.Value;

				if(row != null)
				{
					row.SetYOffset((counter - 1) * -0.112f);
					counter++;
				}
			}
		}
		public void SetArenaTime(string time)
		{
			if(arenaTimeTextMesh == null)
				return;
			
			if(topGameTime != null)
				topGameTime.SetTime(time);
			
			arenaTimeTextMesh.text = time;
		}

		public void SetServerName(string name)
		{
			if(serverNameTextMesh != null)
				serverNameTextMesh.text = name;
		}

		public void SetArena(string arenaId)
		{
			if(arenaNameTextMesh != null)
				arenaNameTextMesh.text = localization.GetValue(arenaId);
		}

		private bool permanentlyVisible = false;

		public void ShowOnClick(bool show)
		{
			if(!permanentlyVisible)
				SetActive(show);
		}

		public void SetPermanentlyVisible(bool perma)
		{
			permanentlyVisible = perma;
			SetActive(permanentlyVisible);
		}

		private bool isActive = false;

		public override void SetActive(bool active)
		{
			if(isActive == active)
				return;

			isActive = active;

			if(content != null)
				content.SetActive(active);
			
			if(deactivatedObjectsOnVisible != null)
			{
				foreach(var go in deactivatedObjectsOnVisible)
				{
					if(go != null)
						go.SetActive(!active);
				}
			}

			if(tutorial.isActive && topGameTime != null)
				topGameTime.SetActive(false);
		}
	}
}
