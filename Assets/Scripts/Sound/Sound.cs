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
using TouchOrchestra;

namespace GMReloaded
{
	[RequireComponent(typeof(AudioSource))]
	public class Sound : SoundBase
	{
		public enum Tag
		{
			None,
			Ambient,
			UI,
			BackgroundMusic
		}

		public enum State
		{
			Enabled,
			Disabled
		}

		public Action<ISound> OnStop;

		public override string id { get; set; }

		[SerializeField]
		public new Tag tag;

		public State state { get; private set; }

		public bool persistent { get; private set; }
		public float initVolume { get; private set; }

		private bool initVolumeSet = false;

		protected SoundManager soundManager { get { return SoundManager.GetInstance(); } }

		public override AudioSource source
		{
			get
			{ 
				if(_source == null && this != null)
				{
					_source = GetComponent<AudioSource>();
					initVolume = 1f;
				}

				return _source;	
			} 

			set { _source = value; }
		}

		public override AudioClip clip { get { return source != null ? source.clip : null; } set { if(source != null) source.clip = value; } }

		public override float volume
		{
			get
			{
				if(source == null)
					return 0f;

				return source.volume;
			}

			set
			{ 
				if(source == null)
					return;

				source.volume = Mathf.Clamp01(value);
			}
		}

		public bool shouldUpdatePlayingSound { get { return _playStarted && !isPlaying; } }

		private AudioSource _source;

		private bool _playStarted = false;

		public void SetState(State _state)
		{
			this.state = _state;
		}

		public override bool isPlaying { get { return source != null && source.isPlaying; } set {  throw new NotSupportedException(); } }

		public override void Play(Transform parent = null)
		{
			if(source == null || (state == State.Disabled && tag != Sound.Tag.BackgroundMusic))
				return;

			SetParent(parent);

			if(!source.enabled)
				source.enabled = true;
			
			source.Play();
			_playStarted = true;

			_OnPlay();
		}

		public override void SetParent(Transform p)
		{
			transform.parent = p == null ? transform : p;

			transform.localPosition = Vector3.zero;
			transform.localScale = Vector3.one;

		}

		public override void Pause()
		{
			if(source == null)
				return;

			source.Pause();

			_OnPause();
		}

		public override void Stop()
		{
			if(source == null)
				return;

				source.Stop();
		}

		public void Mute(bool mute)
		{
			if(source == null)
				return;

			source.mute = mute;
		}

		public void SetLooping(bool looping)
		{
			if(source == null)
				return;

			source.loop = looping;
		}

		public override void SetVolume(float volume)
		{
			if(source == null)
				return;

			if(!initVolumeSet)
			{
				initVolume = volume;
				initVolumeSet = true;
			}

			source.volume = Mathf.Clamp01(volume);
		}

		public void PlayOnStart(bool _play)
		{
			if(_play)
				Play();
		}

		public void SetTag(Sound.Tag tag) { this.tag = tag; }

		public void SetPersistent(bool _persistent)
		{
			this.persistent = _persistent;
		}

		public void SetPlayOnGameStart()
		{
			if(soundManager == null)
				return;

			soundManager.SetPlayOnGameStart(this);
		}

		public void UpdatePlayingSound()
		{
			if(shouldUpdatePlayingSound && !soundManager.gamePaused)
			{
				_OnStop();
				_playStarted = false;
			}
		}


		#region Events

		private void _OnPlay()
		{
			
		}

		private void _OnPause()
		{
			
		}

		private void _OnStop()
		{
			if(OnStop != null)
				OnStop(this);

			soundManager.AddRecycledSound(id, this);
		}

		#endregion

		public override void Destroy()
		{
			if(this == null)
				return;

			soundManager.RemoveSoundInstance(this);
			Destroy(this.gameObject);
		}
	}
}
