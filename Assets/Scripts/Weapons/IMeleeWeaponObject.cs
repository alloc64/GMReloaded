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
using CodeStage.AntiCheat.ObscuredTypes;

namespace GMReloaded
{	
	public abstract class IMeleeWeaponObject : IHandWeaponObject, IRagdollDirectionalInfluencer, IAttackerObject
	{
		public RobotEmil ParentRobot { get { return robotParent; } }

		public WeaponType WeaponType { get { return weaponType; } }

		public int ProjectileType { get {  return -1; } }

		protected float damage
		{
			get
			{
				var c = weaponConfig;

				if(c == null)
					return 0f;

				return c.damage;
			}
		}

		protected float damagePercentual
		{
			get
			{
				var c = weaponConfig;

				if(c == null)
					return 0f;

				return c.damagePercentual;
			}
		}

		#region IRagdollDirectionalInfluencer implementation

		[SerializeField]
		private float hitForceMultiplier = 1000f;

		private Vector3 _hitNormal;

		public Vector3 hitForce { get { return _hitNormal * hitForceMultiplier; } }

		#endregion

		[SerializeField]
		protected new Collider collider;

		private int meleeAnimationLayer = -1;

		protected ISound swingSound;

		private bool _isAttackActive = false;
		private bool isAttackActive { get { return _isAttackActive; } set { _isAttackActive = value; if(!_isAttackActive) { hitColliders.Clear(); hitRobots.Clear(); } } }

		protected override void Awake()
		{
			base.Awake();
		}

		private HashSet<Collider> hitColliders = new HashSet<Collider>();
		private HashSet<RobotEmilNetworked> hitRobots = new HashSet<RobotEmilNetworked>();


		protected virtual void OnCollisionEnter(Collision collision)
		{
			if(isAttackActive)
			{
				_hitNormal = collision.contacts[0].normal;
			}
		}

		protected virtual void OnTriggerStay(Collider c)
		{
			if(isMeleeTriggered)
			{
				if(hitColliders.Add(c))
				{
					var otherRobotEmil = c.GetComponent<RobotEmilNetworked>();

					if(otherRobotEmil == null)
					{
						var childCollision = c.GetComponent<ChildCollision>();

						if(childCollision != null)
							otherRobotEmil = childCollision.GetParentComponent<RobotEmilNetworked>();
					}

					if(otherRobotEmil != robotParent)
					{
						if(otherRobotEmil != null)
						{
							if(robotParent.clientType == RobotEmil.ClientType.LocalClient && hitRobots.Add(otherRobotEmil))
								OnRobotHit(otherRobotEmil);
						}
						else
						{
							OnHitObject(c);
						}
					}
				}
			}
		}

		protected virtual void Update()
		{
			if(isAttackActive)
			{
				var robotAnimator = robotParent.animator;
				AnimatorStateInfo currentState = robotAnimator.GetCurrentAnimatorStateInfo(meleeAnimationLayer);

				var normalizedTime = currentState.normalizedTime;

				if(!isMeleeTriggered && normalizedTime >= 0.3f && normalizedTime < 0.75f)
				{
					SetMeleeTriggered(true);
				}

				if(isMeleeTriggered && normalizedTime >= 0.75f)
				{
					SetMeleeTriggered(false);
				}
			}
		}

		#region Hit Events

		protected virtual void OnHitObject(Collider collider)
		{
		}

		protected virtual void OnRobotHit(RobotEmilNetworked otherRobotEmil)
		{
			otherRobotEmil.HitByMeleeWeapon(this, damagePercentual * (robotParent == null ? ((ObscuredFloat)1f): robotParent.meleeDamageMultiplier), damage);
		}


		#endregion

		protected virtual void SetMeleeAnimationLayer(int layer)
		{
			this.meleeAnimationLayer = layer;
		}

		private bool isMeleeTriggered = false;
		protected virtual void SetMeleeTriggered(bool active)
		{
			if(collider != null)
			{
				collider.isTrigger = isMeleeTriggered = active;

				if(isMeleeTriggered && swingSound != null)
					swingSound.Play(transform);
			}
		}

		#region Robot Control interface

		public override void PrepareForAttack(RobotEmil robotParent, RobotEmil.AttackType attackType)
		{
		}

		public override int Attack(RobotEmil robotParent, RobotEmil.AttackType attackType, double timestamp)
		{
			if(robotParent == null && !isAttackActive)
				return -1;
			
			TriggerAttackAnimation();

			isAttackActive = true;

			if(meleeAnimationLayer < 0)
				Debug.LogError("meleeAnimationLayer not set");

			return 0;
		}

		public virtual void OnEndOfStabAnimation(RobotEmil robotParent)
		{
			isAttackActive = false;
		}

		public override void GrabToHand(IAnimatorMonoBehaviour animatorBehaviour, Transform hand)
		{
			base.GrabToHand(animatorBehaviour, hand);

			isAttackActive = false;
			SetMeleeTriggered(false);
		}

		protected abstract void TriggerAttackAnimation();

		#endregion
	}
	
}
