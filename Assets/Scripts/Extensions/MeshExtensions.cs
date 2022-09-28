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

public static class MeshExtensions 
{
	public static void RecalculateTangents(this Mesh mesh)
	{
		int vertexCount = mesh.vertexCount;
		var vertices = mesh.vertices;
		var normals = mesh.normals;
		var texcoords = mesh.uv;
		var triangles = mesh.triangles;
		int triangleCount = triangles.Length/3;

		var tangents = new Vector4[vertexCount];
		Vector3 [] tan1 = new Vector3[vertexCount];
		Vector3 [] tan2 = new Vector3[vertexCount];
		int tri = 0;

		for (int i = 0; i < triangleCount; i++)
		{
			var i1 = triangles[tri];
			var i2 = triangles[tri+1];
			var i3 = triangles[tri+2];

			var v1 = vertices[i1];
			var v2 = vertices[i2];
			var v3 = vertices[i3];

			var w1 = texcoords[i1];
			var w2 = texcoords[i2];
			var w3 = texcoords[i3];

			var x1 = v2.x - v1.x;
			var x2 = v3.x - v1.x;
			var y1 = v2.y - v1.y;
			var y2 = v3.y - v1.y;
			var z1 = v2.z - v1.z;
			var z2 = v3.z - v1.z;

			var s1 = w2.x - w1.x;
			var s2 = w3.x - w1.x;
			var t1 = w2.y - w1.y;
			var t2 = w3.y - w1.y;

			float r = 1.0f / (s1 * t2 - s2 * t1);
			var sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
			var tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

			tan1[i1] += sdir;
			tan1[i2] += sdir;
			tan1[i3] += sdir;

			tan2[i1] += tdir;
			tan2[i2] += tdir;
			tan2[i3] += tdir;

			tri += 3;
		}

		for(int i = 0; i < vertexCount; i++)
		{
			Vector3 n = normals[i];
			Vector3 t = tan1[i];

			// Gram-Schmidt orthogonalize
			Vector3.OrthoNormalize(ref n, ref t);

			tangents[i].x  = t.x;
			tangents[i].y  = t.y;
			tangents[i].z  = t.z;

			// Calculate handedness
			tangents[i].w = ( Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0 ) ? -1.0f : 1.0f;
		}      

		mesh.tangents = tangents;
	}
}
