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
using UnityEngine;

namespace Independent
{
	public static class tk2dAnimatorExtension
	{
		public static void StartCoroutine(IEnumerator cor)
		{
			Coroutine.GetInstance().ProcessCoroutine(cor);
		}

		public static void PlayIndependent(this tk2dSpriteAnimator animation)
		{
			if(animation == null)
				return;

			StartCoroutine(_PlayCoroutine(animation));
		}


		private static IEnumerator _PlayCoroutine(this tk2dSpriteAnimator animation)
		{
			if(animation == null)
				yield break;

			bool isPlaying = true;
			float _progressTime = 0f;
			float _timeAtLastFrame = 0f;
			float _timeAtCurrentFrame = 0f;
			float deltaTime = 0f;

			animation.Play();

			_timeAtLastFrame = UnityEngine.Time.realtimeSinceStartup;

			tk2dSpriteAnimationClip _currClip = animation.CurrentClip;

			if(_currClip == null)
				yield break;

			while(isPlaying)
			{
				_timeAtCurrentFrame = UnityEngine.Time.realtimeSinceStartup;
				deltaTime = _timeAtCurrentFrame - _timeAtLastFrame;
				_timeAtLastFrame = _timeAtCurrentFrame; 

				_progressTime += deltaTime;

				animation.UpdateAnimation(deltaTime);

				if(animation.CurrentFrame >= _currClip.frames.Length)
				{
					if(_currClip.wrapMode != tk2dSpriteAnimationClip.WrapMode.Loop)
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
		}
	}
}
