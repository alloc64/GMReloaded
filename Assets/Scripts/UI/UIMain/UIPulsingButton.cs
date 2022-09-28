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

namespace GMReloaded
{
	public class UIPulsingButton : MonoBehaviourTO
	{
		private Timer easeTimer = new Timer();

		protected void Update()
		{
			SetUniformScale(easeTimer.Loop(0.93f, 1.0f, 0.15f));
		}

		public virtual void SetUniformScale(float _scale)
		{
			localScale = new Vector3(_scale, _scale, localScale.z);
		}
	}
}
