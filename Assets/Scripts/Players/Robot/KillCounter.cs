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
using System;

namespace GMReloaded
{
	public class KillCounter
	{
		//

		private int killCount;

		private float lastKillTimestamp;

		//

		public KillCounter()
		{
			Reset();
		}

		//

		public void Reset()
		{
			killCount = 0;
			lastKillTimestamp = -1f;
		}

		public void OnKilledEnemy(RobotEmilNetworked robotParent)
		{
			int diff = (int)(Time.realtimeSinceStartup - lastKillTimestamp);

			int timeBetweenKills = 7;

			if(lastKillTimestamp > 0 && diff > timeBetweenKills)
			{
				killCount -= (int)(diff / timeBetweenKills);

				if(killCount < 0)
					killCount = 0;
			}
			else
			{
				killCount++;
			}

			if(robotParent != null)
				robotParent.OnMultikillHappened(killCount);

			lastKillTimestamp = Time.realtimeSinceStartup;
		}
	}
	
}
