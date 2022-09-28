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
using System;
using System.Threading;
using System.Linq;

namespace GMReloaded
{
	public class Loom : MonoSingletonPersistent<Loom>
	{
		private List<Action> _actions = new List<Action>();

		public static void QueueOnMainThread(Action action)
		{
			lock(Instance._actions)
			{
				Instance._actions.Add(action);
			}
		}

		private List<Action> actions = new List<Action>();

		// Update is called once per frame
		private void Update()
		{
			if(_actions.Count > 0)
			{
				actions.Clear();

				lock(_actions)
				{
					actions.AddRange(_actions);
					_actions.Clear();
				}

				foreach(var a in actions)
				{
					a();
				}
			}
		}
	}
}
