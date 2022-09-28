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

namespace GMReloaded.Achievements.Stats
{
	public class ArenaCompletionStat : PersistentStat
	{
		#region implemented abstract members of PersistentStat

		public override PersistentStats.StatId statId { get { return PersistentStats.StatId.ArenaCompletionStat; } }

		#endregion

		private List<string> completedArenaIds = new List<string>();

		#region implemented abstract members of PersistentStat

		public override void OnSerializeStruct(System.IO.BinaryWriter bw)
		{
			bw.Write(completedArenaIds.Count);

			foreach(var arenaId in completedArenaIds)
			{
				if(arenaId != null)
					bw.Write(arenaId);
			}
		}

		public override bool OnDeserializeStruct(System.IO.BinaryReader br)
		{
			completedArenaIds.Clear();

			int cnt = br.ReadInt32();

			for(int i = 0; i < cnt; i++)
			{
				string arenaId = br.ReadString();

				if(arenaId != null)
					completedArenaIds.Add(arenaId);
			}

			return completedArenaIds.Count > 0;
		}

		#endregion

		public void SetCompleted(string arenaId)
		{
			Deserialize();

			//foreach(var caid in completedArenaIds)
			//	Debug.Log("completed arenaId " + caid);

			if(!completedArenaIds.Contains(arenaId))
				completedArenaIds.Add(arenaId);

			bool missing = false;

			foreach(var cfg in Config.Arenas.arenaConfig)
			{
				if(!completedArenaIds.Contains(cfg.Key))
				{
					missing = true;
					break;
				}
			}

			//Debug.Log("SetArenaCompleted " + arenaId + " missing " + missing);

			if(!missing)
			{
				missions.IncrementMission(Config.Missions.MissionIDs.Mission_82, 1);
			}

			Serialize();
		}
	}
}
