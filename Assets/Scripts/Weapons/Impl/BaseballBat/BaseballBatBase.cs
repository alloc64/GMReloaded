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
	public class BaseballBatBase : IMeleeWeaponObject 
	{
		public override void GrabToHand(IAnimatorMonoBehaviour robotParent, Transform hand)
		{
			base.GrabToHand(robotParent, hand);

			//TODO:
			robotParent.SetAnimatorLayerWeight(AnimatorLayer.BaseballBat_Hands, 1f);
		}

		public override void OnRemoveFromHand(IAnimatorMonoBehaviour robotParent)
		{
			robotParent.SetAnimatorLayerWeight(AnimatorLayer.BaseballBat_Hands, 0f);
		}

		protected override void OnHitObject(Collider collider)
		{
			base.OnHitObject(collider);


		}

		protected override void OnRobotHit(RobotEmilNetworked otherRobotEmil)
		{
			base.OnRobotHit(otherRobotEmil);

			//otherRobotEmil.Hit(robotParent, Config.Weapons.knifeToRobotDamage, damage);
			//robotParent.SendRemoteHit(WeaponType.Knife, otherRobotEmil, Config.Weapons.knifeToRobotDamage, damage);
		}

		protected override void TriggerAttackAnimation()
		{
			if(robotParent == null)
				return;

			robotParent.TriggerBaseballBatAttackAnimation();
		}
	}
}
