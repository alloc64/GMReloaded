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

using System;
using System.Collections;

namespace Independent
{
	public static class AnimationExtensions
	{
		public static void StartCoroutine(IEnumerator cor)
		{
			Coroutine.GetInstance().ProcessCoroutine(cor);
		}

		public static void Play(this UnityEngine.Animation animation, Action onComplete)
		{
			if(animation == null)
				return;

			UnityEngine.AnimationClip _clip = animation.clip;

			if(_clip == null)
				return;

			StartCoroutine(_PlayCoroutine(animation, _clip.name, onComplete));
		}

		public static void Play(this UnityEngine.Animation animation, string clipName, Action onComplete)
		{
			StartCoroutine(_PlayCoroutine(animation, clipName, onComplete));
		}

		private static IEnumerator _PlayCoroutine(this UnityEngine.Animation animation, string clipName, Action onComplete)
		{
			if(animation == null)
				yield break;

			UnityEngine.AnimationState _currState = animation[clipName];

			if(_currState == null)
				yield break;

			bool isPlaying = true;
			float _progressTime = 0f;
			float _timeAtLastFrame = 0f;
			float _timeAtCurrentFrame = 0f;
			float deltaTime = 0f;

			animation.Play(clipName);

			_timeAtLastFrame = UnityEngine.Time.realtimeSinceStartup;

			while(isPlaying)
			{
				if(_currState == null)
					yield break;

				_timeAtCurrentFrame = UnityEngine.Time.realtimeSinceStartup;
				deltaTime = _timeAtCurrentFrame - _timeAtLastFrame;
				_timeAtLastFrame = _timeAtCurrentFrame; 

				_progressTime += deltaTime;

				_currState.normalizedTime = _progressTime / _currState.length; 

				if(animation != null)
					animation.Sample();

				if(_progressTime >= _currState.length)
				{
					if(_currState.wrapMode != UnityEngine.WrapMode.Loop)
					{
						isPlaying = false;
					} 
					else
					{
						_progressTime = 0.0f;
					}
				}

				yield return new UnityEngine.WaitForEndOfFrame();
			}

			yield return null;



			if(onComplete != null)
			{
				onComplete();
			} 
		}
	}
}
