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

namespace GMReloaded.UI.Final.Loading
{
	public class KBLoadingMenu : KBFocusableSuccessorsGUI 
	{
		[SerializeField]
		private tk2dTextMesh hintText;

		[SerializeField]
		private tk2dBaseSprite loaderSprite;

		[SerializeField]
		private tk2dBaseSprite loaderOpositeSprite;

		[SerializeField]
		private float loaderSpriteRotationSpeed = 2f;

		protected override void Update()
		{
			base.Update();

			RotateSprite(loaderSprite, 1f);
			RotateSprite(loaderOpositeSprite, -1f);
		}

		private void RotateSprite(tk2dBaseSprite sprite, float d)
		{
			if(sprite != null)
			{
				var lea = sprite.localEulerAngles;
				lea.z += loaderSpriteRotationSpeed * Time.deltaTime * d;
				sprite.localEulerAngles = lea;
			}
		}

		public void SetHint(string id)
		{
			if(hintText != null)
				hintText.text = (id == null) ? "" : localization.GetValue(id);
		}
	}
}
