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
using System.Collections.Generic;
using System.Linq;
using System;
using TouchOrchestra;

namespace GMReloaded
{
	public class SoundCFG
	{
		public SoundCFG(string path, float volume)
		{
			this.path = path;
			this.volume = volume;
		}

		public string path;

		public float volume = 1f;

		public float pitch = 1f;

		public float spatialBlend = 1f;

		public float minDistance = 1f;

		public float spread = 0f;

		public float maxDistance = 500f;

		public bool loop = false;

		public bool mute = false;

		public SoundCFG SetVolume(float volume) { this.volume = volume; return this; }

		public SoundCFG SetPitch(float pitch) { this.pitch = pitch; return this; }

		public SoundCFG SetSpatialBlend(float spatialBlend) { this.spatialBlend = spatialBlend; return this; }

		public SoundCFG SetStereo(bool stereo) { SetSpatialBlend(stereo ? 0f : 1f); return this; }

		public SoundCFG SetMinDistance(float minDistance) { this.minDistance = minDistance; return this; }

		public SoundCFG SetSpread(float spread) { this.spread = spread; return this; }

		public SoundCFG SetMaxDistance(float maxDistance) { this.maxDistance = maxDistance; return this; }

		public SoundCFG SetLoop(bool loop) { this.loop = loop; return this; }

		public SoundCFG SetMute(bool mute) { this.mute = mute; return this; }
	}
	
}
