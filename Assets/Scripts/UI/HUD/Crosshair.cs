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
	public class Crosshair : MonoBehaviourTO
	{
		[SerializeField]
		private Renderer spriteRenderer;

		private Material _spriteMaterial;
		private Material spriteMaterial
		{
			get
			{ 
				if(spriteRenderer != null)
					_spriteMaterial = spriteRenderer.material;
			
				return _spriteMaterial;
			}
		}

		private void Awake()
		{
			SetDepeletedProgress(1f);
		}


		public void SetDepeletedProgress(float p)
		{
			if(spriteMaterial != null)
				spriteMaterial.SetFloat("_Range", p);
		}

		public void Show()
		{
			SetActive(true);
		}

		public void Hide()
		{
			SetActive(false);
		}
	}
}
