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
	public abstract class TrapSequence : ITrapSequence
	{
		[SerializeField]
		public TrapType _trapType;
		public TrapType trapType { get { return _trapType; } }

		private TrapSequenceDispatcher trapDispatcher = new TrapSequenceDispatcher();

		#region Dispatch methods

		public virtual bool Dispatch(float dispatchTime)
		{
			return trapDispatcher.Dispatch(this, dispatchTime);
		}

		public virtual void OnDispatchProgress(float t)
		{

		}

		public virtual void OnDispatched()
		{

		}

		#endregion
	}
}