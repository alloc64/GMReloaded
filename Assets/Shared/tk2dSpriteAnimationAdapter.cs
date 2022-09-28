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

namespace TouchOrchestra
{
	[ExecuteInEditMode]
	public class tk2dSpriteAnimationAdapter : MonoBehaviour 
	{
		Color color = Color.white;
		tk2dBaseSprite sprite = null;

		public Color spriteColor = Color.white;
		// Use this for initialization
		private void Awake() 
		{
			sprite = GetComponent<tk2dBaseSprite>();

			if (sprite != null) 
				spriteColor = sprite.color;
		}

		// Update is called once per frame
		void LateUpdate () 
		{
			if (sprite != null && sprite.color != spriteColor) 
			{
				sprite.color = spriteColor;
				sprite.Build();
			}
		}
	}
}