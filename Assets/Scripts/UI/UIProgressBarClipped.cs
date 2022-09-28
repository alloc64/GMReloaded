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
	public class UIProgressBarClipped : MonoBehaviourTO
	{
		[SerializeField]
		private tk2dClippedSprite progressSprite;

		[SerializeField]
		private bool interpolate = false;

		[SerializeField]
		private bool invertProgressDirection = false;

		//

		private float lastProgress = 0f;

		private float destProgress = 0f;

		//

		private float lerpTimer = 0f;

		private bool initSetProgress = true;

		//
		/*
		private void Awake()
		{
			StartCoroutine(t());
		}

		private IEnumerator t()
		{
			yield return new WaitForSeconds(1f);

			SetProgress(0.1f);
			yield return new WaitForSeconds(1f);

			SetProgress(0.2f);
			yield return new WaitForSeconds(1f);

			SetProgress(0.4f);
			yield return new WaitForSeconds(1f);

			SetProgress(0.2f);
			yield return new WaitForSeconds(1f);

			SetProgress(0.8f);
		}
		*/

		private void Update()
		{
			if(!interpolate)
				return;

			if(!Mathf.Approximately(lastProgress, destProgress) && lerpTimer < 1f)
			{
				lerpTimer += Time.deltaTime * 3f;

				var p = Mathf.Lerp(lastProgress, destProgress, TouchOrchestra.Ease.EaseProcess(lerpTimer, TouchOrchestra.EaseType.InOut));

				SetProgressNotInterpolated(p);

				if(lerpTimer >= 0.99f)
					this.lastProgress = this.destProgress;
			}
		}

		public void SetProgress(float progress)
		{
			this.destProgress = progress;

			this.lerpTimer = 0f;

			if(initSetProgress || destProgress < lastProgress || !interpolate)
			{
				this.lastProgress = this.destProgress;

				SetProgressNotInterpolated(progress);
				initSetProgress = false;
			}
		}

		private void SetProgressNotInterpolated(float p)
		{
			if(progressSprite != null)
			{
				var d = progressSprite.ClipRect;

				if(invertProgressDirection)
				{
					d.width = 1f;
					d.x = 1f - p;
				}
				else
				{
					d.width = p;
				}

				progressSprite.ClipRect = d;
			}
		}
	}
	
}
