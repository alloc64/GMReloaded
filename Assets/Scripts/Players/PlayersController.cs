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

namespace GMReloaded
{
	public class PlayersController : MonoSingleton<PlayersController>
	{
		private Dictionary<int, RobotEmilNetworked> players = new Dictionary<int, RobotEmilNetworked>();

		public virtual bool Register(RobotEmilNetworked obj)
		{
			if(obj == null)
				return false;

			Debug.LogWarning("Register player " + obj + " - photonPlayerId " + obj.photonPlayerId);

			return players[obj.photonPlayerId] = obj;
		}

		public virtual bool Unregister(RobotEmilNetworked obj)
		{
			if(obj == null)
				return false;

			return players.Remove(obj.photonPlayerId);
		}

		public virtual RobotEmilNetworked Get(int photonPlayerId)
		{
			RobotEmilNetworked player = null;
			players.TryGetValue(photonPlayerId, out player);

			return player;
		}

		public virtual Dictionary<int, RobotEmilNetworked> Objects { get { return players; } }

		public int numAlive
		{
			get
			{
				int numAlive = 0;
				foreach(var kvp in Objects)
				{
					var p = kvp.Value;

					if(p != null)
					{
						if(p.state != RobotEmil.State.Dead && p.state != RobotEmil.State.Unspawned)
						{
							numAlive++;
						}
					}
				}

				return numAlive;
			}
		}

		public RobotEmil bestBoxDestroyer
		{
			get
			{
				RobotEmil robot = null;
				int lastBoxes = 0;

				foreach(var kvp in Objects)
				{
					var p = kvp.Value;

					if(p != null)
					{
						if(p.boxes > lastBoxes)
						{
							lastBoxes = p.boxes;
							robot = p;
						}
					}
				}

				return robot;
			}
		}
	}
	
}
