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
using System.Linq;

namespace GMReloaded
{
	public class GameInfoStack : MonoBehaviourTO
	{
		[SerializeField]
		private GameInfoStackRow basePrefab;

		[SerializeField]
		private float rowMargin = 0.08f;

		private float rowOffset = 0f;

		private PrefabsRecyclerBase<GameInfoStackRow> _recycler;
		private PrefabsRecyclerBase<GameInfoStackRow> recycler
		{
			get
			{
				if(_recycler == null)
				{
					_recycler = new PrefabsRecyclerBase<GameInfoStackRow>(basePrefab, transform);
					_recycler.Preinstantiate(20);
					basePrefab.SetActive(false);
				}

				return _recycler;
			}
		}

		private List<GameInfoStackRow> gameInfoRows = new List<GameInfoStackRow>();

		//

		/*
		//test only

		private void Awake()
		{
			PlayerSuicide("YOLO1");
			PlayerSuicide("YOLO2");
			PlayerSuicide("YOLO3");
			PlayerSuicide("YOL04");
			PlayerSuicide("YOLO5");
			PlayerSuicide("YOLO6");
			PlayerSuicide("YOLO7");

			StartCoroutine(cor());
		}

		private System.Collections.IEnumerator cor()
		{
			yield return new WaitForSeconds(1f);
			PlayerSuicide("YOLO8");
			yield return new WaitForSeconds(1f);
			PlayerSuicide("YOLO9");
			yield return new WaitForSeconds(1f);
			PlayerSuicide("YOLO10");
			yield return new WaitForSeconds(1f);
			PlayerSuicide("YOLO11");
		}
		*/
		

		private void Update()
		{
			bool updateOffsets = RemoveOldRows();

			if(updateOffsets)
			{
				UpdateOffsets();
			}
		}

		private void UpdateOffsets()
		{
			rowOffset = 0f;

			foreach(var r in gameInfoRows.OrderBy((r) => r.timestamp))
			{
				if(r != null)
				{
					r.SetLocalPositionY(rowOffset);
					rowOffset -= rowMargin;
				}
			}
		}

		private bool RemoveOldRows()
		{
			bool updateOffsets = false;
			int k = 0;
			for(int i = 0; i < gameInfoRows.Count; i++)
			{
				var r = gameInfoRows[i];
				k++;

				if(r != null && (r.isOutdated || (gameInfoRows.Count >= 6 && k <= 5)))
				{
					recycler.Enqueue(r);

					gameInfoRows.RemoveAt(i);

					updateOffsets = true;
				}
			}

			return updateOffsets;
		}

		#region Public state methods

		public void PlayerInfo(string info)
		{
			var r = AddRow(GameInfoStackRow.Type.Info);
			r.SetText(info);
		}

		public void PlayerSuicide(string player)
		{
			PlayerSuicide(Color.white, player);
		}

		public void PlayerSuicide(Color color, string player)
		{
			var r = AddRow(GameInfoStackRow.Type.DeathInfo);
			r.SetSuicideText(color, player);
		}

		public void PlayerKilled(string playerLeft, string playerRight)
		{
			PlayerKilled(Color.white, playerLeft, Color.white, playerRight);
		}

		public void PlayerKilled(Color plColor, string playerLeft, Color prColor, string playerRight)
		{
			var r = AddRow(GameInfoStackRow.Type.DeathInfo);
			r.SetDeathText(plColor, playerLeft, prColor, playerRight);
		}

		#endregion

		private GameInfoStackRow AddRow(GameInfoStackRow.Type type)
		{
			var gameInfoRow = recycler.Dequeue();

			if(gameInfoRow == null)
				return null;

			gameInfoRow.SetType(type);
			gameInfoRow.ResetTransforms();

			if(!gameInfoRows.Contains(gameInfoRow))
				gameInfoRows.Add(gameInfoRow);

			RemoveOldRows();

			UpdateOffsets();

			return gameInfoRow;
		}
	}
}
