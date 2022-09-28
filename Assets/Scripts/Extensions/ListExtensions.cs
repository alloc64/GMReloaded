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

using System.Collections.Generic;
using System;
using System.Linq;

public static class ListExtensions
{
	private static Random rnd;

	static ListExtensions()
	{
		rnd = new Random();
	}

	public static void Shuffle<T>(this IList<T> list)
	{
		for(var i=0; i < list.Count; i++)
			list.Swap(i, rnd.Next(i, list.Count));
	}

	public static void Swap<T>(this IList<T> list, int i, int j)
	{
		var temp = list[i];
		list[i] = list[j];
		list[j] = temp;
	}

	public static List<T> GetTrueDistinctRandom<T>(this List<T> sourceList, int itemsToSelect)
	{
		int sourceSize = sourceList.Count;

		// Generate an array representing the element to select from 0... number of available
		// elements after previous elements have been selected.
		int[] selections = new int[itemsToSelect];

		// Simultaneously use the select indices table to generate the new result array
		List<T> resultArray = new List<T>();

		var random = new Random();

		for (int count = 0; count < itemsToSelect; count++) {

			// An element from the elements *not yet chosen* is selected
			int selection = random.Next(sourceSize - count);
			selections[count] = selection;
			// Store original selection in the original range 0.. number of available elements

			// This selection is converted into actual array space by iterating through the elements
			// already chosen.
			for (int scanIdx = count - 1; scanIdx >= 0; scanIdx--) {
				if (selection >= selections[scanIdx]) {
					selection++;
				}
			}
			// When the first selected element record is reached all selections are in the range
			// 0.. number of available elements, and free of collisions with previous entries.

			// Write the actual array entry to the results
			resultArray.Add(sourceList[selection]);
		}
		return resultArray;
	}

	private static Random random = new Random();

	public static T GetRandom<T>(this IList<T> list)
	{
		if (list.Count == 0)
		{
			return default(T);
		}
		return list[random.Next(0, list.Count)];
	}
}
