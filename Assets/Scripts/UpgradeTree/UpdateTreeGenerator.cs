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

namespace GMReloaded.UpgradeTree
{
	public class UpdateTreeGenerator
	{
		private float[] ranges = new float[] { 0.01f, 0.52f, 0.79f, 0.95f, 0.99f, 1.0f };

		public enum Tier : int
		{
			Tier1 = 1,
			Tier2 = 2,
			Tier3 = 3,
			Tier4 = 4,
			Tier5 = 5
		}

		private List<int> generated;

		public List<int> Process()
		{
			generated = new List<int>();

			var tierRange = Random.Range(15, 30);

			for(int i = 0; i < tierRange; i++)
				GenerateTier();

			#if UNITY_EDITOR
			//Dump();
			#endif

			return generated;
		}

		private void GenerateTier()
		{
			float r = Random.Range(1f, 100f) * 0.01f;

			if(r > ranges[0] && r <= ranges[1])
			{
				generated.Add((int)Tier.Tier1);
			}
			else if(r > ranges[1] && r <= ranges[2])
			{
				generated.Add((int)Tier.Tier2);
			}
			else if(r > ranges[2] && r <= ranges[3])
			{
				generated.Add((int)Tier.Tier3);
			}
			else if(r > ranges[3] && r <= ranges[4])
			{
				generated.Add((int)Tier.Tier4);
			}
			else if(r > ranges[4] && r <= ranges[5])
			{
				generated.Add((int)Tier.Tier5);
			}
		}

		//

		#if UNITY_EDITOR

		private void Dump()
		{
			Debug.Log("Dumping UpdateTreeTiers....");
			for(int i = 0; i < generated.Count; i++)
			{
				Debug.Log(i + " => " + generated[i]);
			}

			DumpGroupedTiers();
		}

		private void DumpGroupedTiers()
		{
			Dictionary<int, int> valueCount = new Dictionary<int, int>();

			foreach(int obj in generated)
			{
				if (valueCount.ContainsKey(obj))
					valueCount[obj]++;
				else
					valueCount[obj] = 1;
			}

			Debug.Log("Dumping grouped tiers....");

			foreach(var kvp in valueCount)
			{
				Debug.Log(kvp.Key + " => " + kvp.Value);
			}
		}

		#endif
	}
	
}
