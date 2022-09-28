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
	public class RobotEmilRemoteClientInterpolator : PositionInterpolator<RobotEmilInterpolatorState>
	{
		public RobotEmilRemoteClientInterpolator(IInterpolationCallback<RobotEmilInterpolatorState> callback) : base(callback)
		{
			
		}

		public void ForcePosition(Vector3 position)
		{
			Debug.Log("ForcePosition " + position);

			var zeroState = bufferedInterpState[0];
			zeroState.position = position;

			ResetData();
			bufferedInterpState[0] = zeroState;
		}
	}

	public class RobotEmilRemoteClientPhotonObserver : IRobotEmilPhotonObserver
	{
		private RobotEmilRemoteClientInterpolator _interpolator;
		public RobotEmilRemoteClientInterpolator interpolator
		{
			get 
			{
				if(_interpolator == null)
					_interpolator = new RobotEmilRemoteClientInterpolator(parentRobot);

				return _interpolator;
			}
		}

		protected override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if(!stream.isWriting)
			{
				OnPhotonViewRead(stream, info);
			}
		}

		protected virtual void OnPhotonViewRead(PhotonStream stream, PhotonMessageInfo info)
		{
			if(parentRobot == null)
				return;

			RobotEmilNetworked.NetworkProperties np = StructSerializer.Deserialize<RobotEmilNetworked.NetworkProperties>((byte[])stream.ReceiveNext());

			// interpolace

			RobotEmilInterpolatorState interpState = new RobotEmilInterpolatorState();
			interpState.timestamp = info.timestamp;

			interpState.currWeaponType = np.currWeaponType;

			interpState.state = np.state;
			interpState.position = np.position;
			interpState.angleY = np.angleY;
			interpState.speedMultiplier = np.speedMultiplier;
			interpState.controlDirection = np.controlDirection;
			interpState.directionState = np.directionState;
			interpState.running = np.running;

			interpState.numBonusGrenades = np.numBonusGrenades;

			interpolator.ReadData(interpState);

			parentRobot.OnNetworkPropertiesReceived(np);
		}

		private void Update()
		{
			interpolator.Interpolate(Time.deltaTime);
		}
	}
}
