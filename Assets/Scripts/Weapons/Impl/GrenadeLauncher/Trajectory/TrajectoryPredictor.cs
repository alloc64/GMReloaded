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
using UnityEngine.Internal;
using System.Collections.Generic;

namespace GMReloaded.Weapons.Impl.GrenadeLauncher
{
	public class TrajectoryPredictor
	{
		public static void PredictTrajectory(ref List<Vector3> segments, Vector3 position, Vector3 segVelocity, float bounciness, int segmentCount = 20, float segmentScale = 1f, int layerMask = Physics.DefaultRaycastLayers, int maxBounceCount = -1)
		{
			if(segments == null)
				segments = new List<Vector3>();
			else
				segments.Clear();

			// The first line point is wherever the player's cannon, etc is
			segments.Insert(0, position);

			// The initial velocity
			//Vector3 segVelocity = playerFire.transform.up * playerFire.fireStrength * Time.deltaTime;

			// reset our hit object
			//_hitObject = null;

			int bounceCount = 0;

			for (int i = 1; i < segmentCount; i++)
			{
				if(maxBounceCount != -1 && bounceCount >= maxBounceCount)
				{
					break;
				}
				// Time it takes to traverse one segment of length segScale (careful if velocity is zero)
				float segTime = (segVelocity.sqrMagnitude != 0) ? segmentScale / segVelocity.magnitude : 0;

				// Add velocity from gravity for this segment's timestep
				segVelocity = segVelocity + Physics.gravity * segTime;

				// Check to see if we're going to hit a physics object
				RaycastHit hit;
				if (Physics.Raycast(segments[i - 1], segVelocity, out hit, segmentScale, layerMask))
				{
					bounceCount++;

					// set next position to the position where we hit the physics object
					segments.Insert(i, segments[i - 1] + segVelocity.normalized * hit.distance);

					// correct ending velocity, since we didn't actually travel an entire segment
					segVelocity = (segVelocity - Physics.gravity * (segmentScale - hit.distance) / segVelocity.magnitude);

					//
					segVelocity *= bounciness;

					// flip the velocity to simulate a bounce
					segVelocity = Vector3.Reflect(segVelocity, hit.normal);
				}
				// If our raycast hit no objects, then set the next position to the last one plus v*t
				else
				{
					segments.Insert(i, segments[i - 1] + segVelocity * segTime);
				}
			}
		}
	}
}
