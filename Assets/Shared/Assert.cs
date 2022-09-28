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
using System;
using System.Collections;

public class Assert 
{
	public static void IsAssigned(UnityEngine.Object _obj)
	{
		IsAssigned(_obj, null);
	}
	
	public static void IsAssigned(UnityEngine.Object _obj, Transform _transform)
	{
		if(_obj == null)
		{
			string _trace =	StackTrace();
			Debug.LogError(_trace, _transform);
		}
	}

	private static string StackTrace()
	{
#if UNITY_METRO
		return "Null reference Asserted, no stack trace on WP_METRO :(";
#else
		System.Diagnostics.StackTrace _trace = new System.Diagnostics.StackTrace(true);

		return "Null reference " + _trace.ToString();
#endif
	}
	
	public static void IsAssigned(UnityEngine.Object [] _obj)
	{
		if(_obj == null)
		{
			string _trace =	StackTrace();
			Debug.LogError(_trace);
		}
	}
}
