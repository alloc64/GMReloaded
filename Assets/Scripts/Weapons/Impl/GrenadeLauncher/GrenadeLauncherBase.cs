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

namespace GMReloaded
{

	public class GrenadeLauncherBase : IHandWeaponFiringProjectilesWithBarrelReloadableSequientially
	{
		[SerializeField]
		private ProjectileType _projectileType = ProjectileType.StickyGrenade;

		public override ProjectileType projectileType { get { return _projectileType; } set { _projectileType = value; } }

		private ProjectileType? initProjectileType;

		[SerializeField]
		private ParticleSystem fireEffect;

		[SerializeField]
		private float forceMultiplier = 1f;

		//

		[SerializeField]
		private Color glowColor = Color.white;

		[SerializeField]
		private Renderer [] glowingObjects;

		//

		[SerializeField]
		private GMReloaded.Weapons.Impl.GrenadeLauncher.TrajectoryVisualization trajectoryVisualization;

		private bool laserSightActive = false;

		//

		private bool grenadesMasked;

		//

		private static int grenadeIdCounter = 0;

		public override Vector3 fireForce 
		{ 
			get 
			{ 
				var force = base.fireForce * forceMultiplier * Config.Weapons.grenadeLauncherMinForce; 

				float maxForce = 15f;

				force.x = Mathf.Clamp(force.x, -maxForce, maxForce);
				force.y = Mathf.Clamp(force.y, -maxForce, maxForce);

				force.z = Mathf.Clamp(force.z, -maxForce, maxForce);

				return force;
			} 
		}

		protected override float reloadTimeMultiplier 
		{ 
			get 
			{ 	
				int _projectileCount = projectileCount;

				if(_projectileCount < 1)
					return base.reloadTimeMultiplier;  

				return _projectileCount; 
			} 
		}

		public override bool isSecondaryAttackAllowed { get { return secondaryAttackType.HasFlag(RobotEmil.SecondaryAttackType.HolyExplosions); } }

		//

		#region Unity

		protected override void Awake()
		{
			base.Awake();

			SetDefaultGlowColor();
		}

		protected virtual void LateUpdate()
		{
			if(trajectoryVisualization != null
			   && secondaryAttackType.HasFlag(RobotEmil.SecondaryAttackType.GunUpgrade)
			   && robotParent != null
			   && robotParent.clientType == RobotEmil.ClientType.LocalClient
			   && robotParent.state != RobotEmil.State.Dead)
			{
				int maxBounceCount = 1;

				switch(projectileType)
				{
					case ProjectileType.BouncyGrenade:
						maxBounceCount = 5;
					break;
						
					case ProjectileType.FlashGrenade:
						maxBounceCount = -1;
					break;
				}

				float _trajectorySegmentSize = 0.1f;

				int _trajectorySegmentCount = 90;

				//Bounciness je hardcoded z materialu granatu, blbe se k tomu pristupuje

				trajectoryVisualization.PredictTrajectory(fireForce, 0.44f, _trajectorySegmentCount, _trajectorySegmentSize, ((1 << Layer.DestroyableEntity) | (1 << Layer.Default)), maxBounceCount);

				laserSightActive = true;
			}
			else
			{
				if(laserSightActive)
				{
					trajectoryVisualization.ClearTrajectory();
				}
			}
		}

		#endregion

		public override void Reinstantiate()
		{
			base.Reinstantiate();

			SetGrenadesMasked(false);

			SetDefaultGlowColor();

			SetDefaultProjectileType();
		}

		public override void GrabToHand(IAnimatorMonoBehaviour robotParent, Transform hand)
		{
			base.GrabToHand(robotParent, hand);

			robotParent.SetAnimatorLayerWeight(AnimatorLayer.GrenadeLauncher_Hands, 1f);
			robotParent.SetAnimatorLayerWeight(AnimatorLayer.SpineBend, 1f);
		}

		public override void OnRemoveFromHand(IAnimatorMonoBehaviour robotParent)
		{
			robotParent.SetAnimatorLayerWeight(AnimatorLayer.GrenadeLauncher_Hands, 0f);
			robotParent.SetAnimatorLayerWeight(AnimatorLayer.SpineBend, 0f);
		}

		#region Robot Control interface

		public override void PrepareForAttack(RobotEmil robotParent, RobotEmil.AttackType attackType)
		{
		}

		protected override IProjectileObjectWithExplosion AttackWithProjectile(RobotEmil robotParent, RobotEmil.AttackType attackType, double timestamp)
		{
			if(robotParent == null)
				return null;

			bool canGrab = canGrabNewProjectile;

			if(!canGrab)
			{
				OnNoAmmo();

				Debug.LogWarning("Unable dequeue new projectile -- probably depeleted - " + projectileType);
				return null;
			}

			if(!initProjectileType.HasValue)
			{
				initProjectileType = projectileType;
			}

			if(secondaryAttackType.HasFlag(RobotEmil.SecondaryAttackType.HolyExplosions))
			{
				switch(attackType)
				{
					case RobotEmil.AttackType.Primary:

						SetDefaultProjectileType();
						SetDefaultGlowColor();

					break;
				
					case RobotEmil.AttackType.Secondary:
					
						projectileType = ProjectileType.HolyGrenade;
						SetGlowColor(Config.Bonuses.HolyExplosionsGlowColor);

						Debug.Log("holyExplosions active - forcing Holy Grenade");

					break;
				}
			}

			var currGrenade = projectileRecycler.GetPrefab<GrenadeBase>(projectileType);

			if(currGrenade == null)
			{
				Debug.LogWarning("GrenadeLauncher :: unable get grenade " + projectileType);
				return null;
			}
			else
			{
				robotParent.TriggerShootingAnimation();

				OnProjectileFired(currGrenade);

				currGrenade.SetOnPosition(barrel.transform);
				currGrenade.SetMasked(grenadesMasked);
				currGrenade.Fire(this, fireForce, robotParent, robotParent.grenadeDamageMultiplier, robotParent.grenadeRadiusMultiplier, robotParent.grenadeExplosionSpeedUp, timestamp);

				if(fireEffect != null)
					fireEffect.PlayWithClear();

				grenadeIdCounter++; 

				if(grenadeIdCounter >= Config.Grenades.maxSerializedCount)
					grenadeIdCounter = 0;
				
				return currGrenade;
			}
		}

		#endregion

		#region Glow

		private void SetDefaultGlowColor()
		{
			SetGlowColor(glowColor);
		}

		public void SetGlowColor(Color color)
		{
			if(glowingObjects != null)
			{
				foreach(var r in glowingObjects)
				{
					if(r != null)
					{
						var mat = r.material;

						mat.color = color;

						mat.SetColor("_EmissionColorUI", color);
						mat.SetColor("_EmissionColor", mat.GetColor("_EmissionColorUI") * mat.GetFloat("_EmissionScaleUI"));

						mat.EnableKeyword("_EMISSION");

						r.material = mat;
					}
				}
			}
		}

		#endregion

		#region Bonuses

		private void SetDefaultProjectileType()
		{
			if(initProjectileType.HasValue)
				projectileType = initProjectileType.Value; // restornu na default granat
		}

		public void SetGrenadesMasked(bool grenadesMasked)
		{
			this.grenadesMasked = grenadesMasked;
		}

		#endregion

		#region Sounds

		protected override void OnLoadSounds()
		{
			base.OnLoadSounds();

			fireProjectileSound = snd.Load(Config.Sounds.grenadeLauncherFireProjectile);
		}

		#endregion
	}
}