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
using System;
using System.Collections.Generic;

namespace TouchOrchestra
{
	public class Dispatcher : MonoSingleton<Dispatcher> 
	{
		private Queue<Action> _queuedActions;

		public void RunOnMainThread(Action action)
		{
			if(action == null)
				return;

            if (_queuedActions == null)
                _queuedActions = new Queue<Action>();

			lock(_queuedActions)
			{
				_queuedActions.Enqueue(action);
			}
		}

		private void Update()
        {
            if (_queuedActions == null)
                return;

			lock(_queuedActions)
			{
				while(_queuedActions.Count > 0)
				{
					Action dequeuedAction = _queuedActions.Dequeue();

					if(dequeuedAction != null)
						dequeuedAction();
				}
			}
		}
	}
}
