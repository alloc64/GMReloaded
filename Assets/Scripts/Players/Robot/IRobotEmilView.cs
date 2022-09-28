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
using System;

using Input = TeamUtility.IO.InputManager;

namespace GMReloaded
{
	public abstract class IRobotEmilView
	{
		protected RobotEmilViewObserver o;

		public virtual Ray viewRay 
		{
			get
			{
				var t = o.cameraTransform;

				if(t == null)
					return new Ray();

				var lp = t.localPosition;
				return new Ray(t.TransformPoint(lp), o.direction);
			}
		}

		protected RobotEmilViewObserver.Type viewType;

		//

		public IRobotEmilView(RobotEmilViewObserver observer)
		{
			this.o = observer;

			Awake();
		}

		protected abstract void Awake();

		public abstract void Update();

		public abstract void SyncStates(ref RobotEmilViewObserver.Direction directionState, ref Vector3 controlDirection, ref bool running);

		public virtual void SetViewType(RobotEmilViewObserver.Type viewType)
		{
			this.viewType = viewType;
		}

		public abstract void Shake();
	}
	
}
