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

namespace GMReloaded
{
	public class MonoBehaviourWithAnimator : MonoBehaviourTO
	{
		[SerializeField]
		protected new Animator animator;
		/*
		protected void MixAnimations(string animId, Transform mixingTransform)
		{
			AnimationState a = animator[animId];

			if(a == null)
			{
				Debug.LogError("MixAnimations failed to find anim " + animId);
				return;
			}

			a.blendMode = AnimationBlendMode.Blend;
			a.layer = layer;
			a.weight = 1.0f;
			a.AddMixingTransform(mixingTransform);
		}

		public void PlayAnimation(string animId, float crossFadeDelay = 0.1f, float speed = Mathf.Infinity)
		{
			if(animator == null)
			{
				Debug.Log("Legacy animation null");
				return;
			}

			var a = animator[animId];

			//Debug.Log("PlayAnimation " + animId);

			if(speed < Mathf.Infinity && a != null)
			{
				a.speed = speed;
			}

			if(crossFadeDelay <= 0f)
				animator.Play(animId);
			else
				animator.CrossFade(animId, crossFadeDelay);
		}

		public string PlayAnimationRandom(string [] animIds, float crossFadeDelay = 0.1f)
		{
			if(animator == null || animIds == null)
			{
				Debug.Log("Legacy animation null");
				return null;
			}

			string currRandomAnimationId = animIds[Random.Range(0, animIds.Length)];

			//Debug.Log("PlayAnimation " + currRandomAnimationId + " - " + crossFadeDelay);

			if(crossFadeDelay <= 0f)
				animator.Play(currRandomAnimationId);
			else
				animator.CrossFade(currRandomAnimationId, crossFadeDelay);

			return currRandomAnimationId;
		}

		public void SetAnimationLayer(string animId, int layer)
		{
			if(animator == null || animId == null)
			{
				Debug.Log("Legacy animation null");
				return;
			}

			AnimationState a = animator[animId];

			a.blendMode = AnimationBlendMode.Blend;
			a.layer = layer;
		}*/
	}
}
