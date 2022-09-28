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
using GMReloaded.Entities;

namespace GMReloaded.AI.Bots
{
	public class WeaponProbability
	{
		public bool firstTimeWeapon;

		public float probability;

		public float probabilityMin;
		public float probabilityMax;

		public float minActivityTime;

		public float usedTime;
		public float maxUsageTime;

		public float shootTimeMin;
		public float shootTimeMax;
		public float shootTime;

		public void RefreshShootTime()
		{
			this.shootTime = Random.Range(shootTimeMin, shootTimeMax);
		}

		public WeaponProbability Setup()
		{
			this.probability = Random.Range(probabilityMin, probabilityMax);

			this.usedTime = 0f;
			RefreshShootTime();

			return this;
		}
	}
}