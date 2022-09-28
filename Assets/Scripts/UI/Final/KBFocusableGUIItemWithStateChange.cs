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
	public class KBFocusableGUIItemWithStateChange : KBFocusableGUIItem
	{
		[SerializeField]
		private tk2dBaseSprite activeSprite;

		[SerializeField]
		private tk2dBaseSprite inactiveSprite;

		public override void SetFocused(bool active)
		{
			base.SetFocused(active);

			if(activeSprite != null)
				activeSprite.SetActive(active);
			
			if(inactiveSprite != null)
				inactiveSprite.SetActive(!active);
		}
	}
	
}