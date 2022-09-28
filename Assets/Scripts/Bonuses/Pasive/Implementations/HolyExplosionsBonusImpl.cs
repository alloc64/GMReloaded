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
	public class HolyExplosionsBonusImpl : IBonusPasiveDispatch
	{
		public override bool Dispatch(Bonus bonus, RobotEmilNetworked robotParent, bool permanent)
		{
			if(useCount > 0 && !permanent)
			{
				// maximum 1
				return false;
			}

			bool ret = base.Dispatch(bonus, robotParent, permanent);

			robotParent.AddSecondaryAttackType(RobotEmil.SecondaryAttackType.HolyExplosions);

			return ret;
		}

		public override void StopDispatch()
		{
			base.StopDispatch();

			if(useCount < 1 && robotParent != null)
			{
				robotParent.RemoveSecondaryAttackType(RobotEmil.SecondaryAttackType.HolyExplosions);
			}
		}
	}
}
