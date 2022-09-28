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

namespace GMReloaded.UI.Final.Charts.InternalAPI
{
	public class KBChartsTableRow : IRecyclablePrefab<KBChartsTableRow>
	{
		[SerializeField]
		private tk2dTextMesh posTextMesh;

		[SerializeField]
		private tk2dTextMesh playerTextMesh;

		[SerializeField]
		private tk2dTextMesh killsTextMesh;

		[SerializeField]
		private tk2dTextMesh deathsTextMesh;

		[SerializeField]
		private tk2dTextMesh kdRatioTextMesh;

		[SerializeField]
		private tk2dBaseSprite backgroundSprite;

		//

		[SerializeField]
		private Color32 activeColor = Color.white;

		[SerializeField]
		private Color32 inactiveColor = new Color32(0x59, 0xd6, 0xe4, 0xFF);

		public void Setup(int pos, GMReloaded.API.Leaderboards.Item item)
		{
			if(posTextMesh != null)
				posTextMesh.text = pos.ToString();

			if(playerTextMesh != null)
				playerTextMesh.text = item.nick;

			if(killsTextMesh != null)
				killsTextMesh.text = item.kills.ToString();

			if(deathsTextMesh != null)
				deathsTextMesh.text = item.deaths.ToString();

			if(kdRatioTextMesh != null)
				kdRatioTextMesh.text = item.kdratio.ToString("F");

			bool odd = (pos - 1) % 2 == 0;

			backgroundSprite.SetActive(odd);

			SetAllTextColor(odd ? activeColor : inactiveColor);
		}

		public void SetYOffset(float offset)
		{
			var lp = localPosition;
			lp.y = offset;
			localPosition = lp;
		}

		public override void Reinstantiate()
		{
			SetActive(true);
		}

		private void SetAllTextColor(Color c)
		{
			if(posTextMesh != null)
				posTextMesh.color = c;

			if(playerTextMesh != null)
				playerTextMesh.color = c;

			if(killsTextMesh != null)
				killsTextMesh.color = c;

			if(deathsTextMesh != null)
				deathsTextMesh.color = c;

			if(kdRatioTextMesh != null)
				kdRatioTextMesh.color = c;
		}
	}
}
