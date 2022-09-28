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

public static class ResolutionExtensions 
{
	public static byte[] Serialize(this Resolution res)
	{
		return StructSerializer.Serialize((bw) =>
		{
			bw.Write(res.width);
			bw.Write(res.height);
		});
	}

	public static Resolution Deserialize(this Resolution res, byte[] bytes)
	{
		return StructSerializer.Deserialize<Resolution>(bytes, (br) =>
		{
			Resolution r = new Resolution();

			r.width = br.ReadInt32();
			r.height = br.ReadInt32();

			return r;
		});
	}
}
