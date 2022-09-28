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

public static class VectorExtensions
{
	public static void Write(this System.IO.BinaryWriter bw, Vector3 vector)
	{
		if(bw != null)
		{
			bw.Write(vector.x);
			bw.Write(vector.y);
			bw.Write(vector.z);
		}
	}

	public static Vector3 ReadVector3(this System.IO.BinaryReader br)
	{
		Vector3 vector = Vector3.zero;

		if(br != null)
		{
			vector.x = br.ReadSingle();
			vector.y = br.ReadSingle();
			vector.z = br.ReadSingle();
		}

		return vector;
	}
}