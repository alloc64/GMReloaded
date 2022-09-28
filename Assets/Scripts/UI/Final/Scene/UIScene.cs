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

namespace GMReloaded.UI.Final.Scene
{
	public class UIScene : MonoBehaviourTO
	{
		[SerializeField]
		private Animation camAnimation;

		[SerializeField]
		private Material[] smallGlowingLights;

		//

		//private Timer lightLooper = new Timer();

		//

		public static UIScene Instance
		{
			get
			{ 
				return FindObjectOfType<UIScene>();
			}
		}

		//

		public void OnStateChanged(GMReloaded.UI.Final.KBMenuRenderer.State state)
		{
			//TODO:
		}

		//

		public void PlayAnimation()
		{
			if(camAnimation != null)
				camAnimation.Play();
		}
		/*
		private void Update()
		{
			if(smallGlowingLights.Length > 0)
			{
				float a = lightLooper.LoopIndependent(1.43f, 0.5f, 0.5f);

				for(int i = 0; i < smallGlowingLights.Length; i++)
				{
					var mat = smallGlowingLights[i];

					if(mat != null)
					{
						mat.SetColor("_EmissionColor", mat.GetColor("_EmissionColorUI") * a);
						mat.EnableKeyword("_EMISSION");
					}
				}
			}
		}
		*/
	}
	
}
