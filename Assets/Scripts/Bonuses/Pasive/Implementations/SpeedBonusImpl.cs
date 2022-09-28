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
	public class SpeedBonusImpl : IBonusPasiveDispatch
	{
		protected float speedUp = 1.0f;

		public override bool Dispatch(Bonus bonus, RobotEmilNetworked robotParent, bool permanent)
		{
			if(speedUp >= Config.Bonuses.SpeedBonus_Max)
			{
				// presahl jsem maximum a nepude sebrat
				return false;
			}

			bool ret = base.Dispatch(bonus, robotParent, permanent);

			SetSpeedMultiplier(1f);

			return ret;
		}

		private void SetSpeedMultiplier(float diff)
		{
			if(robotParent == null)
				return;
			
			speedUp = Mathf.Clamp(speedUp + Config.Bonuses.SpeedBonus_Progress * diff, Config.Bonuses.SpeedBonus_Min, Config.Bonuses.SpeedBonus_Max);

			robotParent.SetSpeedMultiplier(speedUp);
		}

		public override void StopDispatch()
		{
			if(useCount > 0)
				SetSpeedMultiplier(-1f);
			
			base.StopDispatch();
		}

		public override void Reset()
		{
			base.Reset();

			speedUp = 1f;
		}
	}
}
