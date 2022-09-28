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

namespace GMReloaded.Achievements
{
	public class Mission : AchievableItemBase
	{
		public int id { get; private set; }

		public int experience { get; private set; }

		public int madnessPoints { get; private set; }

		public bool isIgnoredInProgressBar { get; private set; }

		public bool isAchievement { get; private set; }

		//

		public Mission() : base() {	}

		public Mission(string key, int threshold, int reportStep, int experience, int madnessPoints, bool isIgnoredInProgressBar, bool isAchievement)
		{
			Setup(key, threshold, reportStep);

			this.experience = experience;
			this.madnessPoints = madnessPoints;
			this.isIgnoredInProgressBar = isIgnoredInProgressBar;
			this.isAchievement = isAchievement;
		}

		//

		public void SetId(int id) { this.id = id; }
	}
}