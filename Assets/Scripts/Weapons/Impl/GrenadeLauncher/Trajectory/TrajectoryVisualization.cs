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

namespace GMReloaded.Weapons.Impl.GrenadeLauncher
{
	public class TrajectoryVisualization : MonoBehaviourTO
	{
		[SerializeField]
		public LineRenderer sightLine;

		private List<Vector3> lineSegments = new List<Vector3>();

		public void PredictTrajectory(Vector3 dir, float bounciness, int segmentCount, float segmentScale, int layerMask, int maxBounceCount)
		{
			if(sightLine == null)
				return;

			TrajectoryPredictor.PredictTrajectory(ref lineSegments, position, dir, bounciness, segmentCount, segmentScale, layerMask, maxBounceCount);

			sightLine.SetVertexCount(lineSegments.Count);

			for(int i = 0; i < lineSegments.Count; i++)
				sightLine.SetPosition(i, lineSegments[i]);
		}

		public void ClearTrajectory()
		{
			if(sightLine == null)
				return;
			
			sightLine.SetVertexCount(0);
		}
	}
}