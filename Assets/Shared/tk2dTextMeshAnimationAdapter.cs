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
	public class tk2dTextMeshAnimationAdapter : MonoBehaviour 
	{
		Color color = Color.white;
		tk2dTextMesh _textMesh = null;
		tk2dTextMesh textMesh
		{
			get
			{
				if(_textMesh == null)
					_textMesh = GetComponent<tk2dTextMesh>();
				
				return _textMesh;
			}
		}

		public Color textColor = Color.white;
		// Use this for initialization
		private void Awake() 
		{
			if (textMesh != null) 
				textColor = textMesh.color;
		}

		// Update is called once per frame
		void LateUpdate () 
		{
			if (textMesh != null && textMesh.color != textColor) 
			{
				textMesh.color = textColor;
				textMesh.Commit();
			}
		}

		public void SetColor(Color c)
		{
			if (textMesh != null) 
			{
				textMesh.color = c;
				textMesh.Commit();
			}

			textColor = c;
		}

		public void SetText(string text)
		{
			if(textMesh != null)
				textMesh.text = text;
		}
	}
}