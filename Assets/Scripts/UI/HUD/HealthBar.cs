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
	public class HealthBar : MonoBehaviourTO
	{
		[SerializeField]
		public tk2dTextMesh hpTextMesh;

		[SerializeField]
		private TouchOrchestra.tk2dTextMeshAnimationAdapter hpMessage;

		[SerializeField]
		private Color hpMessagePositiveColor = Color.green;

		[SerializeField]
		private Color hpMessageNegativeColor = Color.red;

		//

		private float lastHPPercents = -1f;

		//

		public void SetHP(float p)
		{
			float hpDiff = 0f;

			if(lastHPPercents > 0.0f)
				hpDiff = Mathf.Clamp(p - lastHPPercents, -1f, 1f);

			lastHPPercents = p;

			hpTextMesh.text = Mathf.Ceil(Mathf.Clamp(p, 0f, 1f) * 100f).ToString();

			if(p <= 0.0f && hpDiff < 0f)
				hpDiff -= 0.01f;
			
			int roundedDiff = Mathf.CeilToInt(hpDiff * 100f);

			if(hpMessage != null && roundedDiff != 0)
			{
				string value = Mathf.Clamp(roundedDiff, -100, 100) + " " + localization.GetValue("HP");

				hpMessage.SetColor(hpDiff > 0 ? hpMessagePositiveColor : hpMessageNegativeColor);
				hpMessage.SetText(value);

				var anim = hpMessage.GetComponent<Animation>();

				if(anim != null)
				{
					anim[anim.clip.name].time = 0.0f;
					anim.Play();
				}
			}
		}
	}
}
