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

namespace GMReloaded.AI.Bots
{

	public class RobotEmilViewBotClient : IRobotEmilView
	{
		private RobotEmilBotClient botClient;

		public RobotEmilViewBotClient(RobotEmilViewObserver observer) : base(observer)
		{
		}

		#region implemented abstract members of IRobotEmilView

		protected override void Awake()
		{

		}

		public override void Update()
		{

		}

		public void SetBotClient(RobotEmilBotClient botClient)
		{
			this.botClient = botClient;
		}

		public override void SyncStates(ref RobotEmilViewObserver.Direction directionState, ref Vector3 controlDirection, ref bool running)
		{
			if(botClient != null)
			{
				directionState = botClient.directionState;
				controlDirection = botClient.controlDirection;
				running = botClient.running;
			}
		}

		public override void Shake()
		{

		}

		#endregion
	}
}