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


#if UNITY_XBOXONE

using System;
using System.Collections.Generic;
using GMReloaded.Achievements;
using System.Collections;

namespace GMReloaded.Xbox.Linq
{
	public static class XboxLINQ_Generated
	{
		public static Dictionary<string, GMReloaded.UI.Final.ServerList.KBServerListItem> ServerList_OrderBy_AOT(this Dictionary<string, GMReloaded.UI.Final.ServerList.KBServerListItem> source, Comparison<KeyValuePair<string, GMReloaded.UI.Final.ServerList.KBServerListItem>> comparison)
		{
			var l = source.ToList().SortAOT(comparison);
			var ret = new Dictionary<string, GMReloaded.UI.Final.ServerList.KBServerListItem>();
			
			foreach(var kvp in l)
				ret.Add(kvp.Key, kvp.Value);
			
			return ret;
		}
		
		public static Dictionary<string, Mission> Missions_OrderBy_AOT(this Dictionary<string, Mission> source, Comparison<KeyValuePair<string, Mission>> comparison)
		{
			var l = source.ToList().SortAOT(comparison);
			var ret = new Dictionary<string, Mission>();
			
			foreach(var kvp in l)
				ret.Add(kvp.Key, kvp.Value);
			
			return ret;
		}
		
		public static Dictionary<MadnessStepType, Config.MadnessMode.MadnessStep> MadnessSteps_OrderBy_AOT(this Dictionary<MadnessStepType, Config.MadnessMode.MadnessStep> source, Comparison<KeyValuePair<MadnessStepType, Config.MadnessMode.MadnessStep>> comparison)
		{
			var l = source.ToList().SortAOT(comparison);
			var ret = new Dictionary<MadnessStepType, Config.MadnessMode.MadnessStep>();
			
			foreach(var kvp in l)
				ret.Add(kvp.Key, kvp.Value);
			
			return ret;
		}

		public static Dictionary<PhotonPlayer, PlayerStatsTableRow> PlayerStatsTable_OrderBy_AOT(this Dictionary<PhotonPlayer, PlayerStatsTableRow> source, Comparison<KeyValuePair<PhotonPlayer, PlayerStatsTableRow>> comparison)
		{
			var l = source.ToList().SortAOT(comparison);
			var ret = new Dictionary<PhotonPlayer, PlayerStatsTableRow>();
			
			foreach(var kvp in l)
				ret.Add(kvp.Key, kvp.Value);
			
			return ret;
		}
	}
	
}

#endif
