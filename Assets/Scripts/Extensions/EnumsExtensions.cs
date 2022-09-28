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
using System.Collections.Generic;
using System;

public static class EnumsExtensions
{
	public static bool HasFlag(this Enum input, Enum matchTo)
	{
		return (Convert.ToUInt32(input) & Convert.ToUInt32(matchTo)) != 0;
	}

	public static T Add<T>(this System.Enum type, T value)
	{
		try
		{
			return (T)(object)(((int)(object)type | (int)(object)value));
		}
		catch(Exception ex)
		{
			throw new ArgumentException(string.Format("Could not append value from enumerated type '{0}'.", typeof(T).Name), ex);
		}    
	}

	public static T Remove<T>(this System.Enum type, T value)
	{
		try
		{
			return (T)(object)(((int)(object)type & ~(int)(object)value));
		}
		catch(Exception ex)
		{
			throw new ArgumentException(string.Format("Could not remove value from enumerated type '{0}'.", typeof(T).Name), ex);
		}  
	}

	public static IEnumerable<Enum> GetUniqueFlags(this Enum flags)
	{
		var flag = 1ul;
		foreach(var v in Enum.GetValues(flags.GetType()))
		{
			Enum value = v as Enum;

			ulong bits = Convert.ToUInt64(value);
			while(flag < bits)
			{
				flag <<= 1;
			}

			if(flag == bits && flags.HasFlag(value))
			{
				yield return value;
			}
		}
	}

	public static int UnknownEnum() { return 99; }

	public static T UnknownEnum<T>()
	{
		return (T)(object)((UnknownEnum()));
	}
}
