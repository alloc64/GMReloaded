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

public static class tk2dExtensions 
{
	public static void SetAlpha(this tk2dBaseSprite _sprite, float _a)
	{
		if(_sprite == null)
			return;

		Color _c = _sprite.color;
		_c.a = Mathf.Clamp01(_a);

		_sprite.color = _c;
	}

	public static void SetAlpha(this tk2dSpriteAnimator _animation, float _a)
	{
		if(_animation == null)
			return;

		tk2dBaseSprite _sprite = _animation.Sprite;

		if(_sprite == null)
			return;

		Color _c = _sprite.color;
		_c.a = Mathf.Clamp01(_a);

		_sprite.color = _c;
	}

	public static void SetAlpha(this tk2dTextMesh _textmesh, float _a)
	{
		if(_textmesh == null)
			return;

		Color _c = _textmesh.color;
		_c.a = Mathf.Clamp01(_a);

		_textmesh.color = _c;
		_textmesh.ForceBuild();
	}

	public static void SetSpriteByID(this tk2dBaseSprite sprite, string spriteID, string defaultSpriteID = null)
	{
		if(sprite == null || string.IsNullOrEmpty(spriteID))
			return;

		int thumbID = sprite.GetSpriteIdByName(spriteID, -1);

		if(thumbID != -1)
		{
			sprite.spriteId = thumbID;
		}
		else
		{
			if(!string.IsNullOrEmpty(defaultSpriteID))
				thumbID = sprite.GetSpriteIdByName(defaultSpriteID, -1);

			if(thumbID != -1)
				sprite.spriteId = thumbID;
			else
				sprite.SetActive(false);
		}
	}

	public static void ScaleByDPI(this tk2dTextMesh text, float maxScale)
	{
		if(text == null)
			return;

		Vector3 scale = text.scale;
		//float dpiScale = Mathf.Min(DPI.scale, 1.5f);

		//scale *= dpiScale;
		//text.scale = scale;
	}
}
