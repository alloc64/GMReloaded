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

namespace GMReloaded.Traps
{
	public class FogTrapSequence : TrapSequence
	{
		private Color defaultFogColor;

		protected UnityStandardAssets.ImageEffects.GlobalFog globalFog { get { return RobotEmilImageEffects.IsNull ? null : RobotEmilImageEffects.Instance.globalFog; } }

		public override bool Dispatch(float dispatchTime)
		{
			bool dispatch = base.Dispatch(dispatchTime);

			if(dispatch)
			{
				//if(globalFog != null)
				//	globalFog.enabled = true;

				RenderSettings.fog = true;
				defaultFogColor = RenderSettings.fogColor;

				RenderSettings.fogColor = new Color32(128, 128, 128, 255);
			}

			return dispatch;
		}

		public override void OnDispatchProgress(float t)
		{
			base.OnDispatchProgress(t);

			float dt = 1f - Mathf.Abs(((2f * t) - 1f));

			RenderSettings.fogDensity = 0.35f * dt;

			//if(globalFog != null)
			//	globalFog.heightDensity = 1.4f * dt;
		}

		public override void OnDispatched()
		{
			base.OnDispatched();

			//if(globalFog != null)
			//	globalFog.enabled = false;

			RenderSettings.fog = false;
			RenderSettings.fogColor = defaultFogColor;
		}
	}
}