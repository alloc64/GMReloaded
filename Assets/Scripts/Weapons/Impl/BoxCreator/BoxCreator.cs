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

using GMReloaded.Entities;

namespace GMReloaded
{
	public class BoxCreator : DestroyableEntityCreator
	{
		public override bool isSecondaryAttackAllowed { get { return secondaryAttackType.HasFlag(RobotEmil.SecondaryAttackType.GunUpgrade); } }

		protected override void Awake()
		{
			LoadIndicators(EntityType.BoxDestroyable, EntityType.ExplosiveBarrel);
			SetEntityType(EntityType.BoxDestroyable);

			base.Awake();
		}

		public override void PrepareForAttack(RobotEmil robotParent, RobotEmil.AttackType attackType)
		{
			UpdateEntityType(attackType);

			//

			base.PrepareForAttack(robotParent, attackType);
		}

		public override int Attack(RobotEmil robotParent, RobotEmil.AttackType attackType, double timestamp, int projectileHashId)
		{
			UpdateEntityType(attackType);

			int ret = base.Attack(robotParent, attackType, timestamp, projectileHashId);

			if(ret >= 0 && robotParent != null && robotParent.clientType == RobotEmil.ClientType.LocalClient && entityType == EntityType.BoxDestroyable)
			{
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_53, 1);
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_54, 1);
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_55, 1);
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_56, 1);
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_57, 1);
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_58, 1);
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_59, 1);

				upgradeTree.IncrementItem(Config.UpgradeTree.ItemIDs.Item_8, 1);
			}

			return ret;
		}

		private void UpdateEntityType(RobotEmil.AttackType attackType)
		{
			switch(attackType)
			{
				case RobotEmil.AttackType.Primary:
					SetEntityType(EntityType.BoxDestroyable);
				break;

				case RobotEmil.AttackType.Secondary:
					
					if(secondaryAttackType.HasFlag(RobotEmil.SecondaryAttackType.GunUpgrade))
						SetEntityType(EntityType.ExplosiveBarrel);
					
				break;
			}
		}
	}
}