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

	public class SoundManager : SoundManagerBase<Sound>
	{
		private static SoundManager _instance;

		public static SoundManager GetInstance() 
		{
			if(_instance == null) 
			{
				_instance = FindObjectOfType(typeof(SoundManager)) as SoundManager;

				if(_instance == null) 
				{
					string _objName = "#" + typeof(SoundManager).ToString();
					
					var _obj = new GameObject(_objName);
					_instance = _obj.AddComponent<SoundManager>();
				}
			}

			return _instance;
		}

		public enum State
		{
			None,
			OnSoundEnabled,
			OnSoundDisabled,

			OnMusicEnabled,
			OnMusicDisabled
		}

		private const string sndVolumeKey = "SND_Volume";
		private float _volume = -1f;
		public float volume
		{
			get 
			{ 
				if(_volume < 0f)
					volume = PlayerPrefs.GetFloat(sndVolumeKey, 1f); 

				return _volume;
			}
			set 
			{ 
				if(_volume == value)
					return;

				AudioListener.volume = _volume = value; 

				PlayerPrefs.SetFloat(sndVolumeKey, _volume);
			}
		}

		private SpeechManager _speech;
		public SpeechManager speech
		{
			get
			{
				if(_speech == null)
					_speech = new SpeechManager(this);

				return _speech;
			}
		}

		public bool gamePaused { get; private set; }
     
		#region Unity Stuff

		protected override void Awake()
		{
			base.Awake();

			DontDestroyOnLoad(this.gameObject);

			// preset volume
			 
			AudioListener.volume = volume;
		}

		protected override void Update()
		{
			base.Update();

			for(var i = soundInstances.GetEnumerator(); i.MoveNext();)
			{
				Sound sound = i.Current as Sound;

				if(sound != null && sound.shouldUpdatePlayingSound)
				{
					sound.UpdatePlayingSound();
				}
			}
		}

		private void OnApplicationPause(bool paused) 
		{
			gamePaused = paused;
		}

		#endregion

		#region SoundManagerBase Reimplementation

		public Sound Load(string soundID, Sound.Tag tag)
		{
			Sound _sound = Load(soundID);

			if(_sound == null)
				return null;

			SetSoundTag(_sound, tag);

			return _sound;
		}

		public Sound Load(string soundID)
		{		
			Sound sound = Load<Sound>(soundID);

			if(sound == null)
				return null;

			return sound;
		}

		private Sound GetInstance(string soundID)
		{
            Sound _sound = PeekSoundFromRecycler(soundID) as Sound;

			if(_sound == null)
				_sound = Load(soundID);

			return _sound;
		}

		public Sound Play(string soundID, Sound.Tag tag, Transform parent)
		{
			Sound sound = GetInstance(soundID);

			if(sound == null)
				return null;

			if(sound.tag == Sound.Tag.None || tag != Sound.Tag.None)
			{
				SetSoundTag(sound, tag);
			}

			if(sound.state != Sound.State.Disabled || sound.tag == Sound.Tag.BackgroundMusic)
			{
				DequeueSoundFromRecycler(soundID);

				sound.Play(parent);
			}

			return sound;
		}


		public override Sound Play(string soundID)
		{
			return Play(soundID, Sound.Tag.None, null);
		}

		public virtual Sound Play(string soundID, Transform parent)
		{
			return Play(soundID, Sound.Tag.None, parent);
		}

		public override bool IsPlaying(string soundID)
		{
			Sound _sound = GetInstance(soundID);

			if(_sound != null)
			{
				return _sound.isPlaying;
			}

			return false;
		}

		public override Sound Stop(string soundID)
		{
			Sound _sound = GetInstance(soundID);

			if(_sound != null)
			{
				_sound.Stop();

				AddRecycledSound(soundID, _sound);
			}

			return _sound;
		}

		public void SetSoundTag(Sound sound, Sound.Tag tag)
		{	
			if(sound == null)
				return;

			sound.tag = tag;

			switch(sound.tag)
			{
				default:
				case Sound.Tag.None:
				case Sound.Tag.Ambient:

					sound.SetPersistent(false);

				break;

				case Sound.Tag.BackgroundMusic:

					sound.SetPersistent(false);
					sound.SetLooping(true);

				break;

				case Sound.Tag.UI:

					sound.SetPersistent(true);

				break;
			}
		}


		#endregion

		public void EnableSounds()
		{
			ForeachSound(s =>
			{
				if(s == null)
					return;

				s.SetState(Sound.State.Enabled);
			});
		}

		public void DisableSounds()
		{
			ForeachSound(s =>
			{
				if(s == null || (s != null && s.tag == Sound.Tag.BackgroundMusic))
					return;

				s.SetState(Sound.State.Disabled);
				s.Stop();
			});
		}

		public void SetState(State _state)
		{
			OnStateChanged(_state);
		}

		protected void OnStateChanged(State _state)
		{
			switch(_state)
			{
				default:
				break;

				case State.OnSoundEnabled:
					EnableSounds();
				break;

				case State.OnSoundDisabled:
					DisableSounds();
				break;

				case State.OnMusicEnabled:
				case State.OnMusicDisabled:
					
				break;
			}
		}

		public void DestroyNotPersistentSounds()
		{
			List<Sound> _soundsMarkedToRemove = new List<Sound>();

			ForeachSound(s =>
			{
				if(s == null || s.persistent)
					return;

				_soundsMarkedToRemove.Add(s);
			});

			foreach(var s in _soundsMarkedToRemove)
			{
				//Debug.LogWarning("Remove " + _sound.name + " - " + _sound.persistent, _sound.transform);
				DestroySound(s);
			}
		}

		public void PauseAllAmbientSounds(bool _paused)
		{
			ForeachSound(_sound =>
			{
				if(_sound == null || _sound.tag != Sound.Tag.Ambient)
					return;

				if(_sound.isPlaying)
				{
					if(_paused)
						_sound.Pause();
				}
				else
				{
					if(!_paused)
						_sound.Play();
				}
			});
		}

		#region Ambients


		#endregion

		public void ForeachSound(Action<Sound> _OnSound)
		{
			foreach(var _sound in soundInstances.Cast<Sound>())
			{
				if(_sound != null && _OnSound != null)
					_OnSound(_sound);
			}
		}
	}
}
