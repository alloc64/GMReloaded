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

public static class BoundsExtensions
{
	public static bool IsVisibleFrom(this Bounds bounds, Camera camera)
	{
		if(camera == null)
		{
			Debug.Log("RendererExtensions.IsVisibleFrom camera NULL");
			return false;
		}

		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);

		return GeometryUtility.TestPlanesAABB(planes, bounds);
	}

	public static Bounds TransformBounds(this Transform self, Bounds bounds)
	{
		var center = self.TransformPoint(bounds.center);
		var points = bounds.GetCorners();

		var result = new Bounds(center, Vector3.zero);
		foreach (var point in points)
			result.Encapsulate(self.TransformPoint(point));
		return result;
	}

	public static Bounds InverseTransformBounds(this Transform self, Bounds bounds)
	{
		var center = self.InverseTransformPoint(bounds.center);
		var points = bounds.GetCorners();

		var result = new Bounds(center, Vector3.zero);
		foreach (var point in points)
			result.Encapsulate(self.InverseTransformPoint(point));
		return result;
	}

	// bounds
	public static List<Vector3> GetCorners(this Bounds obj, bool includePosition = true)
	{
		var result = new List<Vector3>();
		for (int x = -1; x <= 1; x += 2)
			for (int y = -1; y <= 1; y += 2)
				for (int z = -1; z <= 1; z += 2)
					result.Add(Vector3.Scale((includePosition ? obj.center : Vector3.zero) + (obj.size / 2), new Vector3(x, y, z)));
		
		return result;
	}
}
