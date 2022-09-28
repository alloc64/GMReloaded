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

namespace TouchOrchestra
{
	public static class Vector2Fast 
	{
		public static float DistanceSqr(Vector2 a, Vector2 b)
		{
			return (a - b).sqrMagnitude;
		}

		public static bool IsInDistance(Vector2 a, Vector2 b, float distance)
		{
			return DistanceSqr(a, b) <= distance * distance;
		}

		public static float MagnitudeSqr(Vector2 v)
		{
			return v.x * v.x + v.y * v.y;
		}

		public static float Magnitude(Vector2 v)
		{
			return UnityEngine.Mathf.Sqrt(MagnitudeSqr(v));
		}

		public static Vector2 Normalize(Vector2 v)
		{
			float d = Magnitude(v);

			if(d > 1E-05)
			{
				v /= d;
				return v;
			}
			else
			{
				return Vector2.zero;
			}
		}

		public static Vector2 RotatePointAroundOrigin(Vector2 origin, float angle, Vector2 point)
		{
			float aSin = UnityEngine.Mathf.Sin(angle);
			float aCos = UnityEngine.Mathf.Cos(angle);

			float rotatedX = aCos * (point.x - origin.x) - aSin * (point.y - origin.y) + origin.x;
			float rotatedY = aSin * (point.x - origin.x) + aCos * (point.y - origin.y) + origin.y;

			return new Vector2(rotatedX, rotatedY);
		}

	}
}
