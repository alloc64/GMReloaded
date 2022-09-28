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
using System;
using TouchOrchestra;

namespace GMReloaded
{

	public class RobotEmilDeathCamEffect : MonoBehaviour
	{
		private GMReloaded.Shaders.PostProcess.GrayScreen grayScreen { get { return RobotEmilImageEffects.Instance.grayScreen; } }

		private enum State
		{
			Hidden,
			Shown,
		}

		private State state;

		private float initGrayScreenIntensity = 0f;

		private void Awake()
		{
			if(grayScreen != null)
				initGrayScreenIntensity = grayScreen.intensity;
		}

		public void Show()
		{
			if(state == State.Shown || grayScreen == null)
				return;

			state = State.Shown;

			grayScreen.SetActive(true);
			Ease.Instance.Alpha(0f, initGrayScreenIntensity, 0.5f, EaseType.In, (t) => 
			{
				if(grayScreen != null) 
					grayScreen.intensity = t;
			});
		}

		public void Hide()
		{
			if(state == State.Hidden || grayScreen == null)
				return;

			float currIntensity = grayScreen.intensity;
			
			Ease.Instance.Alpha(currIntensity, 0f, 0.3f, EaseType.Out, (t) => 
			{
				if(grayScreen != null) 
					grayScreen.intensity = t;
			}, () => 
			{
				if(grayScreen != null)
					grayScreen.SetActive(false);
				
				state = State.Hidden;
			});
		}
	}
	
}
