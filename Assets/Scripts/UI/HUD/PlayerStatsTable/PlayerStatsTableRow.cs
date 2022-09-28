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

namespace GMReloaded
{
	public class PlayerStatsTableRow : IRecyclablePrefab<PlayerStatsTableRow>
	{
		[SerializeField]
		public tk2dTextMesh nickTextMesh;

		[SerializeField]
		public tk2dTextMesh scoreTextMesh;

		[SerializeField]
		public tk2dTextMesh killsTextMesh;

		[SerializeField]
		public tk2dTextMesh deathsTextMesh;

		[SerializeField]
		public tk2dTextMesh pingIndicator;

		//

		[SerializeField]
		private tk2dSprite backgroundSprite;

		//

		[SerializeField]
		public int kills;

		[SerializeField]
		private Color localClientColor = new Color(0.545f, 1f, 0.62f, 1f);

		public void SetYOffset(float offset)
		{
			var lp = localPosition;
			lp.y = offset;
			localPosition = lp;
		}

		public void SetLocalPlayer(bool isLocalClient)
		{
			if(backgroundSprite != null)
				backgroundSprite.SetSprite(isLocalClient ? "scoretable_line_active" : "scoretable_line");
		}

		public void SetNick(string nick)
		{
			if(nickTextMesh != null)
				nickTextMesh.text = nick;
		}

		public void SetScore(int score)
		{
			if(scoreTextMesh != null)
				scoreTextMesh.text = score.ToString();
		}

		public void SetKills(int kills)
		{
			this.kills = kills;

			if(killsTextMesh != null)
				killsTextMesh.text = kills.ToString();
		}

		public void SetDeaths(int deaths)
		{
			if(deathsTextMesh != null)
				deathsTextMesh.text = deaths.ToString();
		}

		public void SetPing(int ping)
		{
			if(pingIndicator != null)
				pingIndicator.text = ping.ToString();
		}

		public override void Reinstantiate()
		{
			SetActive(true);
		}
	}
}
