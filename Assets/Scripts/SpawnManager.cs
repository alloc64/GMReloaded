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
using System.Linq;
using System.Collections.Generic;

namespace GMReloaded
{
	public class SpawnManager : MonoSingleton<SpawnManager>
	{
		[SerializeField]
		private List<SpawnPoint> spawnInstances = new List<SpawnPoint>();

		private PlayersController playersController { get { return PlayersController.Instance; } }

		public bool Register(SpawnPoint spawn)
		{
			if(spawn != null && !spawnInstances.Contains(spawn))
			{
				spawnInstances.Add(spawn);
				return true;
			}

			return false;
		}


		private SpawnPoint GetUnusedSpawns()
		{
			List<SpawnPoint> unusedSpawns = spawnInstances.FindAll((s) => s != null && s.state == SpawnPoint.State.Unused);

			return unusedSpawns == null || unusedSpawns.Count < 1 ? null : unusedSpawns[Random.Range(0, unusedSpawns.Count)];
		}

		private void UnuseSpawns()
		{
			foreach(var s in spawnInstances)
			{
				if(s != null && s.state == SpawnPoint.State.Used)
				{
					bool spawnFree = true;

					foreach(var kvp in playersController.Objects)
					{
						var p = kvp.Value;
						if(p != null)
						{
							float d = Vector3.Distance(p.position, s.position);
							//Debug.Log("d " + d);

							float usedTime = Time.realtimeSinceStartup - s.lastUsedTimestamp;

							Debug.Log("usedTime " + usedTime);

							if(d < 3f && usedTime < 20f)
							{
								spawnFree = false;
								break;
							}
						}
					}

					if(spawnFree)
					{
						s.SetUsed(false);
					}
				}
			}
		}

		public SpawnPoint GetFreeSpawn()
		{
			SpawnPoint spawn = GetUnusedSpawns();

			if(spawn == null)
			{
				UnuseSpawns();

				spawn = GetUnusedSpawns();
			}

			if(spawn != null)
				spawn.SetUsed(true);

			return spawn;
		}

		public void ValidateSpawn(ref Vector3 spawnPosition, ref float spawnPositionAngleY)
		{
			foreach(var s in spawnInstances)
			{
				if(s != null)
				{
					float m = (s.position - spawnPosition).magnitude;
					float a = s.localEulerAngles.y - spawnPositionAngleY;

					if(m < 1 && Mathf.Approximately(a, 0f))
						return;
				}
			}

			var tempSpawn = GetFreeSpawn();

			Debug.LogWarning("Valid spawn not found, trying recover by getting free spawn: " + tempSpawn);

			if(tempSpawn != null)
			{
				spawnPosition = tempSpawn.position;
				spawnPositionAngleY = tempSpawn.localEulerAngles.y;
			}
		}
	}
}
