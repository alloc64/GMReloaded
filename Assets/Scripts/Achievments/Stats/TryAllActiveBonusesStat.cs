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
	public class TryAllActiveBonusesStat : BonusListPersistentStat
	{
		private static HashSet<Bonus.Behaviour> _activeBonuses;
		private static HashSet<Bonus.Behaviour> activeBonuses
		{
			get
			{
				if(_activeBonuses == null)
				{
					_activeBonuses = new HashSet<Bonus.Behaviour>();
					
					foreach(var bcfg in Config.Bonuses.bonusesDistribution)
					{
						if(bcfg != null)
						{
							if(Bonus.GetBonusType(bcfg.bonusBehaviour) == Bonus.Type.Active)
								_activeBonuses.Add(bcfg.bonusBehaviour);
						}
					}
				}

				return _activeBonuses;
			}
		}

		//

		#region implemented abstract members of PersistentStat

		public override PersistentStats.StatId statId { get { return PersistentStats.StatId.TryAllActiveBonusesStat; } }

		#endregion

		public void SetActiveBonusUsed(Bonus.Behaviour bonusBehaviour)
		{
			Deserialize();

			if(usedBonuses.Add(bonusBehaviour))
			{
				bool missing = false;

				//

				//foreach(var activeBonus in activeBonuses)
				//	Debug.Log("activeBonus " + activeBonus);

				//foreach(var usedBonus in usedBonuses)
				//	Debug.Log("usedBonus " + usedBonus);

				//

				foreach(var activeBonus in activeBonuses)
				{
					if(!usedBonuses.Contains(activeBonus))
					{
						missing = true;
						break;
					}
				}

				//Debug.Log("SetActiveBonusUsed " + bonusBehaviour + " missing " + missing);

				if(!missing)
				{
					missions.IncrementMission(Config.Missions.MissionIDs.Mission_83, 1);
				}

				Serialize();
			}
		}
	}
	
}
