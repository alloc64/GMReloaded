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

namespace GMReloaded.Traps
{
	public class TrapDispatcher : MonoSingleton<TrapDispatcher>
	{
		private Dictionary<TrapType, ITrapSequence> _trapSequenceInstances;
		private Dictionary<TrapType, ITrapSequence> trapSequenceInstances
		{
			get
			{
				if(_trapSequenceInstances == null)
				{
					_trapSequenceInstances = new Dictionary<TrapType, ITrapSequence>();

					var trapSequences = FindObjectsOfType<TrapSequenceMonoBehaviour>();

					if(trapSequences.Length > 0)
					{
						foreach(var ts in trapSequences)
						{
							if(ts != null)
							{
								Debug.Log("Found trap sequence " + ts.trapType, ts);

								_trapSequenceInstances[ts.trapType] = ts;
							}
						}
					}

					LoadPredefinedTrapSequences();
				}

				return _trapSequenceInstances;
			}
		}

		private void LoadPredefinedTrapSequences()
		{
			_trapSequenceInstances[TrapType.Fog] = new FogTrapSequence();
		}

		public bool Dispatch(TrapConfig trapConfig)
		{
			if(!trapConfig.valid)
				return false;

			ITrapSequence trapSequence = null;

			trapSequenceInstances.TryGetValue(trapConfig.trapType, out trapSequence);

			if(trapSequence == null)
			{
				Debug.LogError("Failed to dispatch trap " + trapConfig.trapType + ", sequence not found");
				return false;
			}

			return trapSequence.Dispatch(trapConfig.duration);
		}
	}

}