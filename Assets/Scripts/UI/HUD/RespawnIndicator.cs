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
	public class RespawnIndicator : MonoBehaviourTO
	{
		[SerializeField]
		private tk2dBaseSprite circleSprite;

		[SerializeField]
		private tk2dTextMesh textMesh;

		private float timer = 0f;

		private bool isActive = false;

		public void SetTime(int seconds)
		{
			if(textMesh != null)
				textMesh.text = seconds + "s";
		}

		public void Show()
		{
			if(isActive)
				return;

			isActive = true;

			base.SetActive(true);
		}

		public void Hide()
		{
			if(!isActive)
				return;

			isActive = false;

			base.SetActive(false);
		}

		private void Update()
		{
			if(circleSprite != null)
			{
				var lea = circleSprite.localEulerAngles;
				lea.z = -timer * 360f;
				circleSprite.localEulerAngles = lea;

				timer += Time.deltaTime * 0.4f;

				if(timer >= 1f)
					timer = 0f;
			}
		}

		public override void SetActive(bool active)
		{
			if(isActive)
				base.SetActive(active);
		}
	}
}