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
using CodeStage.AntiCheat.ObscuredTypes;
using GMReloaded.Bonuses;

namespace GMReloaded.Achievements.Stats
{
	public class CollectAllBonuses : BonusListPersistentStat
	{
		#region implemented abstract members of PersistentStat

		public override PersistentStats.StatId statId { get { return PersistentStats.StatId.CollectAllBonuses; } }

		#endregion

		public void SetBonusCollected(Bonus.Behaviour bonusBehaviour)
		{
			Deserialize();

			//

			bool missing = false;

			foreach(var distribution in Config.Bonuses.bonusesDistribution)
			{
				if(!usedBonuses.Contains(distribution.bonusBehaviour))
				{
					missing = true;
					break;
				}
			}

			Debug.Log("SetBonusCollected " + bonusBehaviour + " missing " + missing);

			if(!missing)
			{
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_84, 1);
			}

			//

			Serialize();
		}
	}
	
}
