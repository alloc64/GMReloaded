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
	public class KBFocusableButton : KBFocusableTextButton
	{
		[SerializeField]
		private tk2dBaseSprite sprite;

		[SerializeField]
		private Color hoverOverSpriteColor = new Color32(62, 202, 218, 255);

		[SerializeField]
		private Color hoverOutSpriteColor = new Color32(74, 186, 199, 255);

		protected override void Awake()
		{
			base.Awake();
			SetHoverColor(false);
		}

		protected override void OnHover(bool over)
		{
			base.OnHover(over);

			SetHoverColor(over);
		}

		private void SetHoverColor(bool over)
		{
			if(sprite != null)
			{
				sprite.color = over ? hoverOverSpriteColor : hoverOutSpriteColor;
			}
		}
	}
}