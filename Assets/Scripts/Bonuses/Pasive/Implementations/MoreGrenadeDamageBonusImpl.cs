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

namespace GMReloaded.Bonuses.Pasive
{
	public class MoreGrenadeDamageBonusImpl : IBonusPasiveDispatch
	{
		private float damageMultiplier = 1f;
		private float radiusMultiplier = 1f;

		private float damageMultiplierMin = Config.Bonuses.MoreGrenadeDamage_DamageMultiplier_Min;
		private float damageMultiplierMax = Config.Bonuses.MoreGrenadeDamage_DamageMultiplier_Max;

		private float radiusMultiplierMin = Config.Bonuses.MoreGrenadeDamage_RadiusMultiplier_Min;
		private float radiusMultiplierMax = Config.Bonuses.MoreGrenadeDamage_RadiusMultiplier_Max;

		//

		public override bool Dispatch(Bonus bonus, RobotEmilNetworked robotParent, bool permanent)
		{
			if(damageMultiplier >= damageMultiplierMax && radiusMultiplier >= radiusMultiplierMax)
			{
				// presahl jsem maxima u obojiho, nejde sebrat
				return false;
			}

			bool ret = base.Dispatch(bonus, robotParent, permanent);

			SetMultipliers(1f, 1f);

			return ret;
		}

		private void SetMultipliers(float dmDiff, float rmDiff)
		{
			if(robotParent == null)
				return;

			damageMultiplier = Mathf.Clamp(damageMultiplier + Config.Bonuses.MoreGrenadeDamage_DamageMultiplier_Progress * dmDiff, damageMultiplierMin, damageMultiplierMax);
			radiusMultiplier = Mathf.Clamp(radiusMultiplier + Config.Bonuses.MoreGrenadeDamage_RadiusMultiplier_Progress * rmDiff, radiusMultiplierMin, radiusMultiplierMax);

			robotParent.SetGrenadeDamageMultiplier_Bonus(damageMultiplier);
			robotParent.SetGrenadeRadiusMultiplier_Bonus(radiusMultiplier);
		}

		public override void StopDispatch()
		{
			if(useCount > 0)
				SetMultipliers(-1f, -1f);
			
			base.StopDispatch();
		}

		public override void Reset()
		{
			base.Reset();

			damageMultiplier = 1f;
			radiusMultiplier = 1f;
		}
	}
}
