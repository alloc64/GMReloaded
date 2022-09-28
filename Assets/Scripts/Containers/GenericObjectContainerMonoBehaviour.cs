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
using System.Collections.Generic;

namespace GMReloaded
{
	public class GenericObjectContainerMonoBehaviour<T> : MonoBehaviourTO where T : class
	{
		private List<T> objects = new List<T>();

		public virtual bool Register(T obj)
		{
			if(obj == null || objects.Contains(obj))
			{
				Debug.LogWarning("Ignoring object registration " + obj + " - already registered");
				return false;
			}

			objects.Add(obj);

			return true;
		}

		public virtual bool Unregister(T obj)
		{
			if(obj == null)
				return false;

			return objects.Remove(obj);
		}

		public virtual List<T> Objects { get { return objects; } }
	}
	
}
