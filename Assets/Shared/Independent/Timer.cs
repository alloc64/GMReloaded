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

namespace Independent
{
	public interface ITimer
	{

	}

	public class Timer : MonoSingletonPersistent<Timer>, ITimer
	{
		#region Independent Timer
		private const float _timeScale = 1.0f;
		private static float _deltaTime;
		private static float _timeAtLastFrame; 

		public static float deltaTime
		{
			get
			{
				return _deltaTime;
			}
		}
		#endregion

		private void Update()
		{
			float _timeAtCurrentFrame = Time.realtimeSinceStartup;
            _deltaTime = (_timeAtCurrentFrame - _timeAtLastFrame) * _timeScale;
			_timeAtLastFrame = _timeAtCurrentFrame; 
		}
	}
}
