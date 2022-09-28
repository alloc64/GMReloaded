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
using System.Linq;
using TouchOrchestra;

namespace GMReloaded
{
	public abstract class SoundManagerBase<U> : MonoBehaviour where U : ISound
	{
		public AudioListener listener { get; set; }

		protected HashSet<SoundBase> soundInstances = new HashSet<SoundBase>();

		private Queue<SoundBase> _soundQueue = new Queue<SoundBase>();

		#region Recycler

	    private Dictionary<string, Queue<SoundBase>> _recyclableSounds = new Dictionary<string, Queue<SoundBase>>();

		#endregion

		#region Unity Stuff

		protected virtual void Awake()
		{
			TryAddListener();
		}

		protected virtual void Update()
		{
			if(listener == null)
				TryAddListener();
		}

		#endregion

		private void TryAddListener()
		{
			if(GetComponent<AudioListener>() == null)
				SetListener(gameObject.AddComponent<AudioListener>());
		}

		public void SetListener(AudioListener listener)
		{
			if(this.listener != null)
				Destroy(this.listener);

			this.listener = listener;
		}

		public virtual T Load<T>(string soundID) where T : SoundBase
		{
			T snd = PeekSoundFromRecycler(soundID) as T;

			if(snd != null)
				return snd;

			var cfg = SoundConfig.GetSound(soundID);

			if(cfg == null)
			{
				Debug.LogError("Error, sound ID " + soundID + " not found in cfg!");
				return null;
			}

			string clipPath = cfg.path;

			if(clipPath == null)
				return null;

			AudioClip clip = Resources.Load(clipPath) as AudioClip;

			if(clip == null)
			{
				Debug.LogError("Error, invalid path to sound " + clipPath);
				return null;
			}

			snd = new GameObject("Sound " + soundID).AddComponent<T>();

			if(snd == null)
				return null;

			snd.SetParent(transform);

			snd.clip = clip;
			snd.id = soundID;

			AudioSource src = snd.source;

			if(src == null)
			{
				Debug.LogError("Error, sound source invalid " + clipPath);
				return null;
			}

			src.playOnAwake = false;
			src.mute = cfg.mute;
			src.loop = cfg.loop;
			src.volume = cfg.volume;
			src.spatialBlend = cfg.spatialBlend;
			src.pitch = cfg.pitch;
			src.minDistance = cfg.minDistance;
			src.spread = cfg.spread;
			src.maxDistance = cfg.maxDistance;

		    AddRecycledSound(soundID, snd);

			RegisterSound(snd);

			return snd;
		}

		#region Sound Controller Methods

		/// <summary>
		/// Gets sound instance by soundID
		/// </summary>
		/// <returns>The sound instance</returns>
		/// <param name="_soundID">Sound ID</param>
		public T GetInstance<T>(string _soundID) where T : SoundBase
		{
			if(_soundID == null)
			{
				Debug.Log("Error, null soundID is not valid ID!");
				return null;
			}

			return PeekSoundFromRecycler(_soundID) as T;
		}

		public abstract U Play(string _soundID);
		public abstract bool IsPlaying(string _soundID);
		public abstract U Stop(string _soundID);

		#endregion

		#region Manager

		/// <summary>
		/// Sets the listener volume
		/// </summary>
		/// <param name="_volume">_volume.</param>
		public void SetListenerVolume(float _volume)
		{
			AudioListener.volume = Mathf.Clamp01(_volume);
		}

		/// <summary>
		/// Sets sound muted
		/// </summary>
		/// <param name="_status">If set to <c>true</c> sound is muted</param>
		public void Mute(bool _status)
		{
			SetListenerVolume(_status ? 0.0f : 1.0f);
		}
        
		#region Play On Start

		public void SetPlayOnGameStart(SoundBase _sound)
		{
			if(_soundQueue == null)
				return;

			if(_sound == null)
			{
				Debug.LogError("Error, unable to SetPlayOnGameStart null Sound!");
				return;
			}

			_soundQueue.Enqueue(_sound);
		}

		public void PlayQueuedSounds()
		{
			if(_soundQueue == null)
				return;

			while(_soundQueue.Count > 0)
			{
				var _sound = _soundQueue.Dequeue();

				if(_sound == null)
					continue;

				_sound.Play();
			}
		}

		#endregion

		#endregion

		#region Sound Factory Manager

		public virtual void RegisterSound(SoundBase _sound)
		{
			if(_sound == null)
				return;

			soundInstances.Add(_sound);
		}

		public virtual void RemoveSoundInstance(SoundBase _sound)
		{
			if(_sound == null)
				return;

			soundInstances.Remove(_sound);
			RemoveSoundFromRecycler(_sound.id);
		}

		public virtual void DestroySound(SoundBase _sound)
		{
			if(_sound == null)
				return;

			RemoveSoundInstance(_sound);
			_sound.Destroy();
		}

		#endregion

		#region Recycler

		public virtual void AddRecycledSound(string _soundID, SoundBase _sound)
		{

			if(!_recyclableSounds.ContainsKey(_soundID))
			{
				_recyclableSounds[_soundID] = new Queue<SoundBase>();
			}

			_recyclableSounds[_soundID].Enqueue(_sound);

			_sound.SetParent(transform);

			//Debug.Log("AddRecycledSound " + _soundID + " - " + _recyclableSounds[_soundID].Count, _sound);
		}

		public virtual SoundBase PeekSoundFromRecycler(string _soundID)
		{
			SoundBase _sound = null;

			if(!_recyclableSounds.TryGetValue(_soundID, out _soundQueue))
				return null;

			if(_soundQueue != null)
			{
				int _queueCount = _soundQueue.Count;

				while(_queueCount > 0 && (_sound == null || (_sound != null && _sound.isPlaying)))
				{
					_sound = _soundQueue.Peek();
					_queueCount--;
				}
			}

			return _sound;
		}

		public virtual SoundBase DequeueSoundFromRecycler(string _soundID)
        {
			SoundBase _sound = null;


			if(!_recyclableSounds.TryGetValue(_soundID, out _soundQueue))
				return null;

			if(_soundQueue != null)
			{
				do
				{
					_sound = _soundQueue.Dequeue();
				}
				while(_soundQueue.Count > 0 && (_sound == null || (_sound != null && _sound.isPlaying)));
			}

			return _sound;
		}

		public virtual void RemoveSoundFromRecycler(string _soundID)
        {
			_recyclableSounds.Remove(_soundID);
		}

		#endregion
	}
}
