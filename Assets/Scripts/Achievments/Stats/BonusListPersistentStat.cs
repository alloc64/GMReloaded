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
	public abstract class BonusListPersistentStat : PersistentStat
	{
		protected HashSet<Bonus.Behaviour> usedBonuses = new HashSet<Bonus.Behaviour>();

		#region implemented abstract members of PersistentStat

		public override void OnSerializeStruct(System.IO.BinaryWriter bw)
		{
			bw.Write(usedBonuses.Count);

			foreach(Bonus.Behaviour behaviour in usedBonuses)
			{
				bw.Write((int)behaviour);
			}
		}

		public override bool OnDeserializeStruct(System.IO.BinaryReader br)
		{
			usedBonuses.Clear();

			int cnt = br.ReadInt32();

			for(int i = 0; i < cnt; i++)
			{
				usedBonuses.Add((Bonus.Behaviour)br.ReadInt32());
			}

			return usedBonuses.Count > 0;
		}

		#endregion

	}
	
}
