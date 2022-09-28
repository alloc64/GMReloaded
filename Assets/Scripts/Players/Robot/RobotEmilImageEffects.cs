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
using GMReloaded.Shaders.PostProcess;

namespace GMReloaded
{
	public class RobotEmilImageEffects : MonoSingleton<RobotEmilImageEffects>
	{
		public new Camera camera;

		//

		public GrayScreen grayScreen;

		public GrainScreen grain;

		public Vignetting vignetting;

		public RadialBlur radialBlur;

		public Pixelize pixelize;

		public UnityStandardAssets.ImageEffects.GlobalFog globalFog;

		public HolyGrenadeSignalization holyGrenadeSignalization;

		public UnityStandardAssets.ImageEffects.DepthOfField dof;

		public Kino.Bloom bloom;

		public UnityStandardAssets.ImageEffects.Antialiasing antialiasing;

		//

		public void SetHDREnabled(bool enabled)
		{
			if(bloom != null)
				bloom.enabled = enabled;

			if(camera != null)
				camera.hdr = enabled;
		}
	}
	
}
