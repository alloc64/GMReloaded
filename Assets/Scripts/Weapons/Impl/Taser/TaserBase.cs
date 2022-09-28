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
	public class TaserBase : IMeleeWeaponObject
	{
		[SerializeField]
		private ParticleSystem hitParticle;

		protected override void Awake()
		{
			base.Awake();

			swingSound = snd.Load(Config.Sounds.taserSwing);
			SetMeleeAnimationLayer(AnimatorLayer.Knife_Hands);
		}

		protected override void OnHitObject(Collider collider)
		{
			base.OnHitObject(collider);

			snd.Play(Config.Sounds.taserHit, transform);

			if(hitParticle != null)
				hitParticle.Play();
		}

		protected override void TriggerAttackAnimation()
		{
			if(robotParent == null)
				return;

			robotParent.TriggerTaserStabAnimation();
		}

		public override void GrabToHand(IAnimatorMonoBehaviour animatorBehaviour, Transform hand)
		{
			base.GrabToHand(animatorBehaviour, hand);

			animatorBehaviour.SetAnimatorLayerWeight(AnimatorLayer.Knife_Hands, 1f);

			if(robotParent != null)
				robotParent.IgnoreCollision(collider, true);
		}

		public override void OnRemoveFromHand(IAnimatorMonoBehaviour animatorBehaviour)
		{
			animatorBehaviour.SetAnimatorLayerWeight(AnimatorLayer.Knife_Hands, 0f);

			if(robotParent != null)
				robotParent.IgnoreCollision(collider, false);
		}
	}
}
