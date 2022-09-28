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

namespace GMReloaded.UI
{
	public class ChatMessage : IRecyclablePrefab<ChatMessage>
	{
		[SerializeField]
		private tk2dTextMesh textMesh;

		public void SetText(string text, float wrap = -1f)
		{
			if(textMesh != null)
			{
				textMesh.text = text;
				textMesh.Commit();

				if(wrap >= 0)
				{
					while(textMesh.GetComponent<Renderer>().bounds.extents.x * 2 > wrap)
					{
						text = text.Substring(1, text.Length - 1);
						textMesh.text = text;
						textMesh.Commit();
					}
				}
			}
		}

		public void SetLocalPositonY(float y)
		{
			var lp = localPosition;
			lp.y = y;
			localPosition = lp;
		}

		public override void Reinstantiate() 
		{ 
			SetActive(true);
		}
	}
	
}
