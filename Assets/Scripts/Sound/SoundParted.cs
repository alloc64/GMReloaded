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

namespace GMReloaded
{
	public class SoundParted : ISound
	{
		public string id { get; private set; }

		public float volume
		{
			get
			{
				if(currPlayingSound == null)
					return 0f;

				return currPlayingSound.volume;
			}

			set
			{
				if(sounds == null)
					return;

				foreach(var sound in sounds)
				{
					if(sound != null)
						sound.volume = value;
				}
			}
		}

		public int numSounds
		{
			get
			{
				if(sounds == null)
					return 0;

				return sounds.Count;
			}
		}

		public List<Sound> sounds { get; private set; }

		public Sound currPlayingSound { get; private set; }

		protected int randomSoundID
		{
			get
			{
				return UnityEngine.Random.Range(0, numSounds);
			}
		}

		protected SoundManager soundManager
		{
			get
			{
				return SoundManager.GetInstance();
			}
		}

		public SoundParted(string _containerID)
		{
			if(string.IsNullOrEmpty(_containerID))
				throw new NullReferenceException("_containerID");

			this.id = _containerID;
		}

		public void AddSoundPart(string _soundID, Sound.Tag _tag = Sound.Tag.None)
		{
			if(string.IsNullOrEmpty(_soundID))
			{
				return;
			}

			//Debug.Log("AddSoundPart " + _soundID);

			Sound _sound = soundManager.Load(_soundID);

			if(_sound == null)
				return;

			_sound.SetTag(_tag);

			if(sounds == null)
				sounds = new List<Sound>();

			sounds.Add(_sound);
		}

		private void OnStop(ISound _sound)
		{
			if(currPlayingSound == null)
				return;

			//Debug.LogWarning("OnStop part of sound " + currPlayingSound.id);

			Play();
		}

		private int _lastSoundIndex = -1;

		public void Play(Transform parent = null)
		{
			if(currPlayingSound != null && currPlayingSound.isPlaying)
				return;

			int _soundIndex = randomSoundID;

			while(_soundIndex == _lastSoundIndex && numSounds > 1)
			{
				_soundIndex = randomSoundID;
			}

			_lastSoundIndex = _soundIndex;

			if(sounds == null || _soundIndex < 0 || _soundIndex >= sounds.Count)
				return;

			currPlayingSound = sounds[_soundIndex];

			if(currPlayingSound == null)
				return;

			currPlayingSound.OnStop = OnStop;
			currPlayingSound.Play(parent);

			//Debug.LogWarning("Playing part of sound " + currPlayingSound.id);
		}

		public void Pause()
		{
			if(currPlayingSound == null)
				return;

			currPlayingSound.Pause();
		}

		public void Stop()
		{
			if(currPlayingSound == null)
				return;

			Sound _snd = currPlayingSound;

			currPlayingSound = null;

			_snd.Stop();
		}

		public bool isPlaying
		{
			get
			{
				if(currPlayingSound == null)
					return false;

				return currPlayingSound.isPlaying;
			}

			set
			{ 
				throw new NotImplementedException();
			}
		}

		public void Destroy()
		{
			if(sounds == null)
				return;

			foreach(var _sound in sounds)
			{
				if(_sound == null)
					continue;

				_sound.Destroy();
			}
		}
	}
}
