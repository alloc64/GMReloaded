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
using System.Linq;

namespace GMReloaded.UI.Final.Equip
{	
	public class KBWeaponProperty : IRecyclablePrefab<KBWeaponProperty>
	{
		[SerializeField]
		private tk2dTextMesh propertyTitle;

		[SerializeField]
		private tk2dTextMesh propertyValue;

		[SerializeField]
		private UIProgressBarClipped progressBar;

		//

		public override void Reinstantiate()
		{
			SetActive(true);
		}

		//

		public void Setup(int idx, string titleLocalizationId, float value, float maxValue, bool inverse)
		{
			if(propertyTitle != null)
				propertyTitle.text = localization.GetValue(titleLocalizationId);
			
			if(propertyValue != null)
				propertyValue.text = (Mathf.Round(value * 10f) / 10f).ToString();

			//Debug.Log(progressBar + " - " + value + " / " + maxValue);

			if(progressBar != null)
			{
				float p = 1f;

				if(maxValue != 0.0f)
				{
					p = (value / maxValue);

					if(inverse)
						p = 1f - p;
				}

				progressBar.SetProgress(p);
			}

			//

			var lp = Vector3.zero;
			lp.y = idx * -0.09f;
			localPosition = lp;
		}
	}
	
}
