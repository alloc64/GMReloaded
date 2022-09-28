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

namespace GMReloaded.Traps
{
	public struct TrapConfig
	{
		public bool valid;

		public TrapType trapType;

		public float duration;

		public int repeats;
		public int repeatsMaxCount;

		public float timeBetweenTraps;

		public float probability;

		public TrapConfig(TrapType trapType, float duration, int repeatsMaxCount, float timeBetweenTraps, float probability)
		{
			this.valid = true;

			this.trapType = trapType;

			this.duration = duration;

			this.repeats = 0;

			this.repeatsMaxCount = repeatsMaxCount;

			this.timeBetweenTraps = timeBetweenTraps;

			this.probability = probability;
		}
	}
	
}