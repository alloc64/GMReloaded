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

namespace GMReloaded.UI.Final
{
	public class KBFocusableTextButton : KBFocusableGUIItem
	{
		[SerializeField]
		private tk2dTextMesh focusedTextMesh;

		[SerializeField]
		private Color activeColor = Color.white;

		[SerializeField]
		private Color inactiveColor1 = new Color32(0xF1, 0xF1, 0xF1, 0xFF);

		public override void SetFocused(bool active)
		{
			base.SetFocused(active);

			if(focusedTextMesh != null)
				focusedTextMesh.color = active ? activeColor : inactiveColor1;
		}
	}
}