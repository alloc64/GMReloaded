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
using Independent;

namespace GMReloaded
{
	[ExecuteInEditMode]
	public class Infobox : MonoBehaviourTO 
	{
		[SerializeField]
		private tk2dBaseSprite backgroundSprite;

		[SerializeField]
		private tk2dTextMesh titleTextMesh;

		[SerializeField]
		private tk2dTextMesh descTextMesh;

		[SerializeField]
		public Color color;

		[SerializeField]
		private new Animation animation;

		private Color lastColor = Color.white;

		private void LateUpdate() 
		{
			if (lastColor != color) 
			{
				SetAlpha(color.a);
				lastColor = color;
			}
		}
		public void SetMessage(GameMessage.Message msg)
		{
			if(backgroundSprite != null)
				backgroundSprite.SetSpriteByID("GameMessage_" + msg.type);

			if(titleTextMesh != null)
				titleTextMesh.text = localization.GetValue(msg.title);

			if(descTextMesh != null)
				descTextMesh.text = localization.GetValue(msg.description);
			
			if(animation != null)
				animation.Play();
		}

		private void SetAlpha(float a)
		{
			if(backgroundSprite != null)
				backgroundSprite.SetAlpha(a);
			
			if(titleTextMesh != null)
			{
				titleTextMesh.SetAlpha(a);
				titleTextMesh.Commit();
			}
		
			if(descTextMesh != null)
			{
				descTextMesh.SetAlpha(a);
				descTextMesh.Commit();
			}
		}
	}
}
