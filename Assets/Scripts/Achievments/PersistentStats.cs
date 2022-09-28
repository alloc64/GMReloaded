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

namespace GMReloaded.Achievements.Stats
{
	public class PersistentStats
	{
		public enum StatId
		{
			CollectAllBonuses,

			TryAllActiveBonusesStat,
			ArenaCompletionStat
		}

		private Dictionary<StatId, PersistentStat> stats = new Dictionary<StatId, PersistentStat>();
	
		//

		public PersistentStats()
		{
			RegisterStat(new ArenaCompletionStat());
			RegisterStat(new TryAllActiveBonusesStat());
			RegisterStat(new CollectAllBonuses());
		}

		//

		private void RegisterStat(PersistentStat stat)
		{
			if(stat == null)
				return;

			stats[stat.statId] = stat;
		}

		public T GetStat<T>(StatId id) where T : PersistentStat
		{
			PersistentStat stat = null;
			stats.TryGetValue(id, out stat);

			if(stat == null)
				Debug.LogError("Failed to GetStat " + id);

			return stat as T;
		}

		//

		#region Per Implementation Functions

		public void SetCompletedArena(string arenaId)
		{
			var stat = GetStat<ArenaCompletionStat>(StatId.ArenaCompletionStat);

			if(stat != null)
				stat.SetCompleted(arenaId);
		}

		public void SetActiveBonusUsed(Bonuses.Bonus.Behaviour bonusBehaviour)
		{
			var stat = GetStat<TryAllActiveBonusesStat>(StatId.TryAllActiveBonusesStat);

			if(stat != null)
				stat.SetActiveBonusUsed(bonusBehaviour);
		}

		public void SetBonusCollected(Bonuses.Bonus.Behaviour bonusBehaviour)
		{
			var stat = GetStat<CollectAllBonuses>(StatId.CollectAllBonuses);

			if(stat != null)
				stat.SetBonusCollected(bonusBehaviour);
		}

		#endregion
	}
	
}