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

public class PhysicsExtensions 
{
	#region Ballistic trajectory comp

	public static Vector3 GetTrajectoryVelocity(Vector3 startingPosition, Vector3 targetPosition, float lob, Vector3 gravity)
	{
		float physicsTimestep = Time.fixedDeltaTime;
		float timestepsPerSecond = Mathf.Ceil(1f / physicsTimestep);

		float n = lob * timestepsPerSecond;

		Vector3 a = physicsTimestep * physicsTimestep * gravity;
		Vector3 p = targetPosition;
		Vector3 s = startingPosition;

		Vector3 velocity = (s + (((n * n + n) * a) / 2f) - p) * -1 / n;

		velocity /= physicsTimestep;
		return velocity;
	}

	public static Vector3 GetTrajectoryPoint(Vector3 startingPosition, Vector3 initialVelocity, float timestep, Vector3 gravity)
	{
		float physicsTimestep = Time.fixedDeltaTime;
		Vector3 stepVelocity = initialVelocity * physicsTimestep;

		Vector3 stepGravity = gravity * physicsTimestep * physicsTimestep;

		return startingPosition + (timestep * stepVelocity) + ((( timestep * timestep + timestep) * stepGravity ) / 2.0f);
	}

	#endregion
}
