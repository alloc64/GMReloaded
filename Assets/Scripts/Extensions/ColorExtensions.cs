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

public static class ColorExtensions 
{
	public static Color32 ColorFromByteArray(this byte[] bytes)
	{
		return StructSerializer.Deserialize<Color32>(bytes, (br) => 
		{
			Color32 rgb = Color.white;
			rgb.r = br.ReadByte();
			rgb.g = br.ReadByte();
			rgb.b = br.ReadByte();

			return rgb;
		});
	}

	public static byte[] ToByteArray(this Color rgb)
	{
		return ToByteArray32(rgb);
	}

	public static byte[] ToByteArray32(this Color32 rgb)
	{
		return StructSerializer.Serialize((bw) =>
		{
			bw.Write(rgb.r);
			bw.Write(rgb.g);
			bw.Write(rgb.b);
		});
	}



	public static string ToTK2DColor(this Color32 c)
	{
		Color c1 = c;
		return ToTK2DColor(c1);
	}

	public static string ToTK2DColor(this Color c)
	{
		return "^C" + ((int)(c.r * 255)).ToString("X2") + ((int)(c.g * 255)).ToString("X2") + ((int)(c.b * 255)).ToString("X2") + ((int)(c.a * 255)).ToString("X2");
	}
}
