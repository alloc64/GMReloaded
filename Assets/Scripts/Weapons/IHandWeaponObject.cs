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

namespace GMReloaded
{	
	public abstract class IHandWeaponObject : IRecyclablePrefab<IHandWeaponObject>
	{
		private Transform parentInitial;

		protected RobotEmil robotParent;

		//

		protected Achievements.MissionsController missions { get { return Achievements.MissionsController.Instance; } }

		protected UpgradeTree.UpgradeTreeController upgradeTree { get { return UpgradeTree.UpgradeTreeController.Instance; } }

		protected ArenaEventDispatcher arenaEventDispatcher { get { return ArenaEventDispatcher.Instance; } }

		protected ProjectileRecycler projectileRecycler { get { return ProjectileRecycler.Instance; } }

		//

		private WeaponType _weaponType;
		private bool _weaponTypeGet = false;
		public WeaponType weaponType 
		{
			get 
			{ 
				if(!_weaponTypeGet)
				{
					WeaponRecycler.GetPrefabWeaponType(this, out _weaponType);
					_weaponTypeGet = true;
				}

				return _weaponType;
			}
		}

		public virtual ProjectileType projectileType { get { return EnumsExtensions.UnknownEnum<ProjectileType>(); } set { throw new NotImplementedException(); } }

		//

		private Config.Weapons.WeaponConfig _weaponConfig;
		public Config.Weapons.WeaponConfig weaponConfig
		{
			get
			{
				if(_weaponConfig == null)
				{
					var wt = weaponType;

					Config.Weapons.weaponConfig.TryGetValue(wt, out _weaponConfig);

					if(_weaponConfig == null)
						Debug.LogError("Unable find configuration of weapon " + wt);
				}

				return _weaponConfig;
			}
		}

		//

		public float weightSpeedMultiplier
		{
			get
			{
				var c = weaponConfig;

				if(c == null)
					return 1f;

				return Mathf.Clamp(1f / (c.weight / Config.Weapons.defaultWeaponWeight), Config.Weapons.minWeaponSpeedMultiplier, Config.Weapons.maxWeaponSpeedMultiplier);
			}
		}

		//

		public virtual RobotEmil.SecondaryAttackType secondaryAttackType { get { return robotParent != null ? robotParent.secondaryAttackType : RobotEmil.SecondaryAttackType.None; } }

		public virtual bool isSecondaryAttackAllowed { get { return false; } }

		//

		#region Unity

		protected virtual void Awake()
		{
			
		}

		protected virtual void Start()
		{
			
		}

		#endregion

		public override void Reinstantiate()
		{
			SetActive(true);
		}

		protected void ResetParent()
		{
			parent = parentInitial;
		}

		public virtual void GrabToHand(IAnimatorMonoBehaviour animatorBehaviour, Transform hand)
		{
			this.robotParent = animatorBehaviour as RobotEmil;
			parentInitial = parent;
			parent = hand;

			localPosition = Vector3.zero;
			localRotation = Quaternion.identity;
			localScale = Vector3.one;
		}

		public abstract void PrepareForAttack(RobotEmil robotParent, RobotEmil.AttackType attackType);

		public virtual int Attack(RobotEmil robotParent, RobotEmil.AttackType attackType, double timestamp)
		{
			return 0;
		}

		public abstract void OnRemoveFromHand(IAnimatorMonoBehaviour animatorBehaviour);

		protected virtual void UpdateWeaponDepeletedProgress(float p)
		{
			if(robotParent != null)
				robotParent.OnUpdateWeaponDepeletedProgress(p);
		}

		// vola se kdyz robot zdechne - idealni pro schovani indikatoru
		public virtual void OnRobotDeath()
		{
			
		}
	}
	
}
