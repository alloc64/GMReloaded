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
	public struct RobotEmilInterpolatorState : IInterpolationState
	{
		private double _timetstamp;
		public double timestamp { get { return _timetstamp; } set { _timetstamp = value; } }

		internal WeaponType currWeaponType;

		internal RobotEmil.State state;
		internal Vector3 position;
		internal float angleY;
		internal float speedMultiplier;

		internal Vector3 controlDirection;
		internal RobotEmilViewObserver.Direction directionState;
		internal bool running;

		internal int numBonusGrenades;
	}
	
}
