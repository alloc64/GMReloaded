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
using System;
using System.Collections.Generic;
using TouchOrchestra;

namespace GMReloaded.Tutorial.HUD
{
	public class TutorialFadeInOut : MonoBehaviourTO
	{
		[SerializeField]
		private tk2dBaseSprite fadeInOutSprite;	

		//

		public void FadeInOut(Action OnBetweenFadeInOut)
		{
			float t = 1.5f;

			fadeInOutSprite.SetAlpha(0f);
			fadeInOutSprite.SetActive(true);

			Ease.Instance.Alpha(0f, 1.2f, t, EaseType.In, SetAlpha, () =>
			{
				if(OnBetweenFadeInOut != null)
					OnBetweenFadeInOut();

				Ease.Instance.Alpha(1.2f, 0f, t, EaseType.Out, SetAlpha, () =>
				{
					fadeInOutSprite.SetActive(false);
				});
			});
		}

		public void FadeInOut(float delay, Action OnBetweenFadeInOut)
		{
			Timer.DelayAsyncIndependent(delay, () => FadeInOut(OnBetweenFadeInOut));
		}

		//

		private void SetAlpha(float a)
		{
			if(fadeInOutSprite != null)
				fadeInOutSprite.SetAlpha(Mathf.Clamp01(a));
		}
	}
}