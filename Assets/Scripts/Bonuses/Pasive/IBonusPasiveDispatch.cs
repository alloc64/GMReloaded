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

namespace GMReloaded.Bonuses.Pasive
{
	public abstract class IBonusPasiveDispatch : IBonusDispatchable
	{
		public int useCount { get; protected set; }

		public override bool Active { get { return false; } } // toto musi byt false jinak se nedispatchnou bonusy

		private bool _permanent = false;
		public override bool Permanent { get { return _permanent; } }

		//

		protected RobotEmilNetworked robotParent;
		protected Bonus bonus;

		//

		private HUD hud { get { return HUD.Instance; }}

		//

		public override bool Dispatch(Bonus bonus, RobotEmilNetworked robotParent, bool permanent)
		{
			this.bonus = bonus;
			this.robotParent = robotParent;
			this._permanent = permanent;

			useCount++;

			if(robotParent.clientType == RobotEmil.ClientType.LocalClient)
			{
				hud.OnLocalPlayerPasiveBonusPickedUp(bonus, useCount);
			}

			return true;
		}

		public virtual void StopDispatch()
		{
			if(useCount < 1)
				return;

			if(robotParent == null)
			{
				Debug.LogError("Failed to StopDispatch " + this + " - robotParent == null");
				return;
			}

			useCount--;

			if(useCount <= 0)
			{
				useCount = 0;
			}

			if(robotParent != null && robotParent.clientType == RobotEmil.ClientType.LocalClient && bonus != null)
			{
				hud.OnLocalPlayerPasiveBonusDispatchStopped(bonus, useCount);
			}
		}

		public virtual void Reset()
		{
			useCount = 0;
		}
	}
}
