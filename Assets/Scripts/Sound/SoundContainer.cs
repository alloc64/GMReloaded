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
using System;

namespace GMReloaded
{
	public class SoundContainer : ISound
	{
		public string id { get; private set; }

		public float volume
		{
			get
			{
				if(currPlayedSound == null)
					return 0f;

				return currPlayedSound.volume;
			}
			set
			{
				if(currPlayedSound == null || sounds == null)
					return;

				foreach(var sound in sounds)
				{
					if(sound != null)
						sound.volume = value;
				}
			}
		}

		public List<Sound> sounds { get; private set; }

		public Sound currPlayedSound { get; private set; } 

		public int numSounds
		{
			get
			{ 
				if(sounds == null)
					return 0;

				return sounds.Count;
			}
		}

		public Sound firstSound
		{
			get
			{ 
				if(numSounds < 1)
					return null;

				return sounds[0];
			}
		}

		public bool isPlaying
		{
			get
			{
				if(currPlayedSound == null)
					return false;

				return currPlayedSound.isPlaying;
			}

			set
			{ 
				throw new NotImplementedException();
			}
		}

		protected SoundManager soundManager
		{
			get
			{
				return SoundManager.GetInstance();
			}
		}

		public SoundContainer(string _containerID)
		{
			this.id = _containerID;
		}

		public SoundContainer(string _containerID, Sound _defaultSound) : this(_containerID)
		{
			AddSound(_defaultSound);
		}

		public void AddSound(Sound _sound)
		{
			if(_sound == null)
				return;

			if(sounds == null)
				sounds = new List<Sound>();

			//Debug.Log(id + " - " + _sound.id);

			sounds.Add(_sound);
		}

		public void AddSound(string _soundID)
		{
			AddSound(soundManager.Load(_soundID));
		}

		public void RemoveSound(Sound _sound)
		{
			if(_sound == null || sounds == null)
				return;

			sounds.Remove(_sound);
		}

		public void RemoveSound(string _soundID)
		{
			sounds.RemoveAll(_s => _s != null && _s.name == _soundID);
		}

		public void Play(Transform parent)
		{
			if(firstSound == null)
				return;

			currPlayedSound = firstSound;

			firstSound.Play(parent);
		}

		public void Pause()
		{
			Stop();
		}

		public void Stop()
		{
			if(currPlayedSound == null)
				return;

			currPlayedSound.Stop();

			currPlayedSound = null; 
		}

		public void PlayRandomRange(Transform parent, int _start, int _end)
		{
			if(numSounds < 1)
				return;

			if(_start < 0)
				_start = 0;

			if(_end >= numSounds)
				_end = numSounds;

			int _randSndIndex = UnityEngine.Random.Range(_start, _end);

			if(_randSndIndex < 0 || _randSndIndex >= numSounds)
				return;

			Sound _sound = sounds[_randSndIndex];

			//Debug.Log (_randSndIndex + " -- " + _sound.id);

			if(_sound == null)
				return;

			currPlayedSound = _sound;

			_sound.Play(parent);
		}

		public void PlayRandom(Transform parent)
		{
			PlayRandomRange(parent, 0, numSounds);
		}

		public void Destroy()
		{
			foreach(var _sound in sounds)
			{
				if(_sound != null)
					_sound.Destroy();
			}
		}
	}
}
