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
	public class PingIndicator : MonoBehaviour 
	{
		public tk2dSprite [] pointer;

		public tk2dTextMesh pingTextMesh;

		public void SetPing(int ping)
		{
			int count;
			Color c;

			if(pingTextMesh != null)
				pingTextMesh.text = ping.ToString();

			if(ping < 50)
			{
				// zelenej
				count = 3;
				c = Color.green;
			}
			else if(ping >= 50 && ping < 100)
			{
				// zlutej
				count = 2;
				c = Color.yellow;
			}
			else
			{
				//cervenej
				count = 1;
				c = Color.red;
			}

			for(int i = 0; i < 3; i++)
			{
				if(i < count)
				{
					SetColor(i, c);
				}
				else
				{
					SetColor(i, new Color(0f, 0f, 0f, 0.3f));
				}
			}
		}

		private void SetColor(int idx, Color c)
		{
			if(idx < 0 || idx >= pointer.Length)
				return;

			tk2dSprite s = pointer[idx];

			if(s != null)
			{
				s.color = c;
			}
		}
	}
}
