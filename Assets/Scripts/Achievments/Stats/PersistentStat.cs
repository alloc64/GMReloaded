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
	public abstract class PersistentStat : ISerializableStruct
	{
		public abstract PersistentStats.StatId statId { get; }

		private string _statPrefsKey = null;
		private string statPrefsKey 
		{
			get
			{
				if(_statPrefsKey == null)
					_statPrefsKey = "Ach_Stat_" + statId;

				return _statPrefsKey;
			}
		}

		//

		protected Achievements.MissionsController missions { get { return Achievements.MissionsController.Instance; } }

		//

		#region ISerializableStruct implementation

		public abstract void OnSerializeStruct(System.IO.BinaryWriter bw);

		public abstract bool OnDeserializeStruct(System.IO.BinaryReader br);

		#endregion

		//

		protected bool isLoaded { get; private set; }

		//

		public PersistentStat()
		{
			
		}

		//

		protected void Serialize()
		{
			if(!isLoaded)
			{
				Debug.LogError("Failed to serialize PersistentStat - isLoaded == false");
				return;
			}

			byte[] bytes = StructSerializer.Serialize<PersistentStat>(this);

			if(bytes == null)
			{
				Debug.LogError("Failed to serialize PersistentStat");
				return;
			}

			ObscuredPrefs.SetByteArray(statPrefsKey, bytes);
		}

		protected void Deserialize()
		{
			if(isLoaded)
				return;

			byte[] bytes = ObscuredPrefs.GetByteArray(statPrefsKey);

			if(bytes == null)
			{
				Debug.LogError("Failed to de-serialize PersistentStat");
				return;
			}

			StructSerializer.Deserialize<PersistentStat>(this, bytes);

			isLoaded = true;
		}
	}

}
