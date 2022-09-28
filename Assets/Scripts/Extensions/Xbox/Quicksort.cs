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
using GMReloaded.Achievements;
using System.Collections;

namespace GMReloaded.Xbox
{
	public class Quicksort
	{
		public static void Sort<T>(IList<T> elements, Comparison<T> comparsion)
		{
			Sort(elements, 0, elements.Count-1, comparsion);
		}
		
		public static void Sort<T>(IList<T> elements, int left, int right, Comparison<T> comparsion)
		{
			int i = left, j = right;
			T pivot = elements[(left + right) / 2];
			
			while (i <= j)
			{
				while (comparsion(elements[i], pivot) < 0)
				{
					i++;
				}
				
				while (comparsion(elements[j], pivot) > 0)
				{
					j--;
				}
				
				if (i <= j)
				{
					// Swap
					T tmp = elements[i];
					elements[i] = elements[j];
					elements[j] = tmp;
					
					i++;
					j--;
				}
			}
			
			// Recursive calls
			if (left < j)
			{
				Sort(elements, left, j, comparsion);
			}
			
			if (i < right)
			{
				Sort(elements, i, right, comparsion);
			}
		}
	}
}

#endif
