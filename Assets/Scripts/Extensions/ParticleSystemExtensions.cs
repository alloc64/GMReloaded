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

public static class ParticleSystemExtensions 
{
	public static void PlayWithClear(this ParticleSystem ps)
	{
		if(ps != null)
		{
			ps.Clear();
			ps.Play();
		}
	}

	public static void Play(this ParticleSystem ps, Action OnComplete)
	{
		try
		{
			if(ps == null)
			{
				if(OnComplete != null)
					OnComplete();

				return;
			}

			if(OnComplete != null)
			{
				Timer.DelayAsync(ps.duration + ps.startLifetime, OnComplete);
			}

			ps.Play();
		}
		catch(Exception e)
		{
			Debug.LogError("Caught exception + " + e + " in method ParticleSystemExtensions.Play(ParticleSystem, Action)!");
		}
	}
}
