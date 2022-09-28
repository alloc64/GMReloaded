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
	public abstract class IHandWeaponFiringProjectilesWithBarrel : IHandWeaponWithBarrel, IProjectileOwner
	{
		#region Projectile Grabs

		protected virtual bool canGrabNewProjectile
		{
			get
			{
				var c = weaponConfig;

				if(c == null || robotParent == null)
					return true;

				if(c.use == Config.Weapons.WeaponConfig.Use.Forever)
					return true;

				int numBonusGrenades = robotParent.numBonusGrenades;

				if(robotParent.clientType == RobotEmil.ClientType.LocalClient)
				{
					//Debug.Log("canGrab " + c.usedCount + " < " + c.initProjectileCount + " + " + numBonusGrenades);

					return c.usedCount < Mathf.Clamp(c.initProjectileCount + numBonusGrenades, 0, c.maxProjectileCount);
				}
				else
					return true;
			}
		}

		protected virtual int projectileCount
		{
			get
			{ 
				var c = weaponConfig;

				if(c == null || robotParent == null)
					return 0;

				return  Mathf.Clamp(c.initProjectileCount + robotParent.numBonusGrenades, 0, c.maxProjectileCount);
			}
		}

		public virtual Vector3 fireForce
		{
			get
			{
				var controlDir = robotParent.controlDirection;

				var dir = robotParent.direction;
				var barrelDir = barrel.direction;
				var lerpedDir = Vector3.Lerp(dir, barrelDir, 0.20f);

				lerpedDir.y = Mathf.Lerp(dir.y, barrelDir.y, Mathf.Lerp(0.2f, 0.8f, Mathf.Clamp01(controlDir.z)));

				if(controlDir.z <= 0f)
				{
					float d = 1f - Mathf.Abs(controlDir.z);

					if(d < 0.5f)
						d = 0.5f;

					lerpedDir.x *= d;
					lerpedDir.z *= d;
				}

				return lerpedDir;
			}
		}

		#endregion

		#region Sound

		protected ISound noAmmoSound;

		protected ISound fireProjectileSound;

		#endregion

		#region Unity

		protected override void Start()
		{
			base.Start();
			OnLoadSounds();
		}

		protected virtual void Update()
		{
			UpdateWeaponDepeleted();
		}

		#endregion

		protected virtual void UpdateWeaponDepeleted()
		{
			UpdateWeaponDepeletedProgress(canGrabNewProjectile ? 1f : 0f);
		}

		#region Projectile events

		protected virtual void OnProjectileFired(IObjectWithPosition projectile)
		{
			if(robotParent.clientType == RobotEmil.ClientType.LocalClient)
				weaponConfig.usedCount++;
		}

		public virtual void OnProjectileExplode(IObjectWithPosition projectile)
		{
		}

		public virtual void OnProjectileExploded(IObjectWithPosition projectile)
		{
		}

		#endregion

		public virtual int Attack(RobotEmil robotParent, RobotEmil.AttackType attackType, double timestamp, int projectileHashId)
		{
			var projectile = AttackWithProjectile(robotParent, attackType, timestamp);

			if(projectile == null)
				return -1;
			
			int pid = base.Attack(robotParent, attackType, timestamp);

			if(pid >= 0)
			{
				OnFireProjectileSound();
			}

			return pid;
		}

		protected virtual IProjectileObjectWithExplosion AttackWithProjectile(RobotEmil robotParent, RobotEmil.AttackType attackType, double timestamp)
		{
			return null;
		}

		protected virtual int OnStartAttackAssignHashId(RobotEmil robotParent, RobotEmil.AttackType attackType, double timestamp, int projectileHashId)
		{			
			var projectile = AttackWithProjectile(robotParent, attackType, timestamp);

			if(projectile == null)
				return -1;

			if(projectileHashId > 0)
			{
				projectile.AssignHashId(projectileHashId);
			}
			else
				projectile.GenerateHashId();
			
			arenaEventDispatcher.RegisterSyncedProjectile(projectile);

			OnFireProjectileSound();

			return projectile.GetAssignedHashId();
		}

		#region Sound Events

		protected virtual void OnLoadSounds()
		{
			noAmmoSound = snd.Load(Config.Sounds.noAmmo);
		}

		protected virtual void OnNoAmmo()
		{
			if(noAmmoSound != null)
				noAmmoSound.Play(transform);
		}

		protected virtual void OnFireProjectileSound()
		{
			if(fireProjectileSound != null)
				fireProjectileSound.Play(transform);
		}

		#endregion
	}
}