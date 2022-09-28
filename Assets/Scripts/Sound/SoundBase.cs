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

namespace GMReloaded
{
	public abstract class SoundBase : MonoBehaviour, ISound
	{
		public abstract string id { get; set; }
		public abstract float volume { get; set; }
		public abstract bool isPlaying { get; set; }

		public abstract AudioSource source { get; set; }
		public abstract AudioClip clip { get; set; }

		public abstract void Play(Transform parent = null);
		public abstract void Pause();

		public abstract void Stop();

		public abstract void SetVolume(float volume);

		public abstract void SetParent(Transform parent);

		public abstract void Destroy();
	}
}
