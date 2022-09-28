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


#if UNITY_XBOXONE

using System;
using System.Collections.Generic;
using System.Collections;

namespace GMReloaded.Xbox.Linq
{
	public static class XboxLINQ
	{
		public static List<T> ToList<T>(this IEnumerable<T> source)
		{
			List<T> list = new List<T>();

			foreach(var e in source)
			{
				list.Add(e);
			}

			return list;
		}
		
		public static List<T> SortAOT<T>(this IEnumerable<T> source, Comparison<T> comparison)
		{
			var l = source.ToList();
			
			Quicksort.Sort<T>(l, comparison);
			
			return l;
		}
	}
}


#endif



