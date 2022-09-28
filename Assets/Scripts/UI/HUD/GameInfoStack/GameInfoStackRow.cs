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
	public class GameInfoStackRow : IRecyclablePrefab<GameInfoStackRow> 
	{
		public enum Type
		{
			DeathInfo,
			Info
		}

		[SerializeField]
		public tk2dTextMesh textMesh;

		//

		public float timestamp { get; private set; }

		public bool isOutdated { get { return Time.realtimeSinceStartup - timestamp >= 10f; } }

		//

		public override void Reinstantiate()
		{
			SetActive(true);

			timestamp = Time.realtimeSinceStartup;
		}

		public void SetLocalPositionY(float pos)
		{
			var lp = localPosition;
			lp.y = pos;
			localPosition = lp;
		}

		public void SetType(Type type)
		{
			//TODO:
		}

		public void SetDeathText(Color plColor, string playerLeft, Color prColor, string playerRight)
		{
			if(textMesh == null)
				return;

			SetText(plColor.ToTK2DColor() + playerLeft + Color.white.ToTK2DColor() + " killed " + prColor.ToTK2DColor() + playerRight);
		}

		public void SetSuicideText(Color color, string player)
		{
			if(textMesh == null)
				return;

			SetText(color.ToTK2DColor() + player + Color.white.ToTK2DColor() + " killed himself");
		}

		public void SetText(string text)
		{
			if(textMesh != null)
				textMesh.text = text;
		}
	}
}
