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

namespace GMReloaded
{

	public class RocketLauncher : IHandWeaponFiringProjectilesWithBarrelReloadableSequientially
	{
		[System.Serializable]
		public class RocketContainer
		{
			public Transform container;
			public Rocket projectile;
		}

		[SerializeField]
		private RocketContainer [] rocketContainers;

		[SerializeField]
		private Transform barrelModel;

		private bool rotateBarrel = false;

		[SerializeField]
		private float rotateBarrelTime = 0.5f;

		private float rotateBarrelTimer = 0f;

		[SerializeField]
		private float destBarrelRotation = 0f;

		[SerializeField]
		private int currRocketContainerId = 0;

		private float forceMultiplier = 1f;

		//protected override bool canGrabNewProjectile { get { return !rotateBarrel && base.canGrabNewProjectile; } }

		public override Vector3 fireForce
		{ 
			get { return base.fireForce * forceMultiplier;  } 
		}

		protected override float reloadTime
		{
			get
			{
				if(robotParent == null)
					return base.reloadTime;

				return base.reloadTime - robotParent.grenadeExplosionSpeedUp;
			}
		}

		public override ProjectileType projectileType { get { return ProjectileType.Rocket; } }

		private void OnDrawGizmos()
		{
			foreach(var c in rocketContainers)
			{
				if(c.container != null)
					Gizmos.DrawWireSphere(c.container.position, 0.02f);
			}
		}

		protected override void Update()
		{
			base.Update();

			if(rotateBarrel)
			{
				if(rotateBarrelTimer < rotateBarrelTime)
					rotateBarrelTimer += Time.deltaTime;

				if(rotateBarrelTimer >= rotateBarrelTime)
				{
					rotateBarrelTimer = 0f;
					rotateBarrel = false;
				}

				if(barrelModel != null)
				{
					barrelModel.localRotation = Quaternion.Slerp(barrelModel.localRotation, Quaternion.Euler(destBarrelRotation, 0f, 0f), Mathf.Clamp01(rotateBarrelTimer / rotateBarrelTime));
				}
			}
		}

		private void RefillBarrel()
		{
			bool isLocalClient = robotParent.clientType == RobotEmil.ClientType.LocalClient;

			int size = rocketContainers.Length;
			int idx = (int)(((int)destBarrelRotation) / 90);
			idx %= size;

			//Debug.Log(destBarrelRotation + " - " + idx + " - " + size);

			int limit = 0;
			while(limit++ < 10)
			{
				var c = rocketContainers[idx];

				if(c != null && c.projectile == null)
				{
					if(isLocalClient && weaponConfig.usedCount >= projectileCount)
						break;

					var currRocket = projectileRecycler.GetPrefab<Rocket>(ProjectileType.Rocket);

					if(currRocket != null)
					{
						currRocket.SetOnPosition(c.container);
						c.projectile = currRocket;

						if(isLocalClient)
							weaponConfig.usedCount++;
					}
					else
					{
						Debug.LogWarning("RocketLauncher :: unable get Rocket");
					}
				}

				idx++;
				idx %= size;
			}
		}

		#region implemented abstract members of IWeaponObject

		public override void PrepareForAttack(RobotEmil robotParent, RobotEmil.AttackType attackType)
		{
		}

		public override int Attack(RobotEmil robotParent, RobotEmil.AttackType attackType, double timestamp, int projectileHashId)
		{
			return OnStartAttackAssignHashId(robotParent, attackType, timestamp, projectileHashId);
		}

		protected override IProjectileObjectWithExplosion AttackWithProjectile(RobotEmil robotParent, RobotEmil.AttackType attackType, double timestamp)
		{
			if(reloadTimer < reloadTime)
			{
				Debug.LogWarning("Waiting for cooldown...");
				return null;
			}

			Rocket currRocket = null;
			RocketContainer currRocketContainer = TryDequeueLoadedRocketContainer();

			if(currRocketContainer != null)
				currRocket = currRocketContainer.projectile;

			if(currRocket == null)
			{
				OnNoAmmo();

				Debug.LogWarning("rocketContainers not filled - depeleted?");
				return null;
			}

			robotParent.TriggerShootingAnimation();

			OnProjectileFired(currRocket);

			currRocket.Fire(this, robotParent, fireForce);

			rotateBarrel = true;

			destBarrelRotation += 90f;

			if(destBarrelRotation >= 360f)
				destBarrelRotation = 0f;

			currRocketContainer.projectile = null;

			return currRocket;
		}

		#endregion

		private RocketContainer TryDequeueLoadedRocketContainer(bool simulationOnly = false)
		{
			RocketContainer currRocketContainer = null;
			Rocket currRocket = null;

			int i = 0;
			while(i++ < 10 && currRocket == null)
			{
				int idx = currRocketContainerId;

				if(simulationOnly)
				{
					idx++;
					idx %= rocketContainers.Length;
				}
				else
				{
					currRocketContainerId++;
					currRocketContainerId %= rocketContainers.Length;

					idx = currRocketContainerId;
				}

				currRocketContainer = rocketContainers[idx];

				if(currRocketContainer != null && currRocketContainer.projectile != null)
					return currRocketContainer;
			}

			return null;
		}


		protected override void UpdateReloadTimer()
		{
			if(robotParent == null)
				return;

			if(reloadTimer < reloadTime)
			{
				reloadTimer += Time.deltaTime * reloadTimeMultiplier;
			}
		}


		protected override void OnProjectileFired(IObjectWithPosition projectile)
		{
			reloadTimer = 0f;
		}

		public override void OnProjectileExploded(IObjectWithPosition projectile)
		{
			base.OnProjectileExploded(projectile);

			var rc = TryDequeueLoadedRocketContainer(true);

			if(rc == null)
			{
				Debug.Log("No rocket to shoot, trying recharge barrel....");

				RefillBarrel();
			}
		}

		//

		public override void GrabToHand(IAnimatorMonoBehaviour robotParent, Transform hand)
		{
			base.GrabToHand(robotParent, hand);

			RefillBarrel();

			robotParent.SetAnimatorLayerWeight(AnimatorLayer.GrenadeLauncher_Hands, 1f);
			robotParent.SetAnimatorLayerWeight(AnimatorLayer.SpineBend, 1f);
		}

		public override void OnRemoveFromHand(IAnimatorMonoBehaviour robotParent)
		{
			robotParent.SetAnimatorLayerWeight(AnimatorLayer.GrenadeLauncher_Hands, 0f);
			robotParent.SetAnimatorLayerWeight(AnimatorLayer.SpineBend, 0f);
		}

		#region Sounds

		protected override void OnLoadSounds()
		{
			base.OnLoadSounds();

			fireProjectileSound = snd.Load(Config.Sounds.rocketLauncherFireProjectile);
		}

		#endregion
	}
}