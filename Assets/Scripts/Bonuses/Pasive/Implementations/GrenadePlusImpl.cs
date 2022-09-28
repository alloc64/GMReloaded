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
	public class GrenadePlusImpl : IBonusPasiveDispatch
	{
		public override bool Dispatch(Bonus bonus, RobotEmilNetworked robotParent, bool permanent)
		{
			bool grenadeAdded = SetBonusGrenades(1, robotParent);

			if(!grenadeAdded)
				return false;

			return base.Dispatch(bonus, robotParent, permanent);
		}

		private bool SetBonusGrenades(int diff, RobotEmilNetworked robotParent)
		{
			if(robotParent == null)
				return false;

			return robotParent.SetBonusGrenades(diff);
		}

		public override void StopDispatch()
		{
			if(useCount > 0)
				SetBonusGrenades(-1, robotParent);
			
			base.StopDispatch();
		}
	}
}
