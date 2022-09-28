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
using System;

namespace GMReloaded
{	
	[Serializable]
	public class WeaponRecyclerBase<T, R> : MonoSingleton<WeaponRecyclerBase<T, R>> 
		where R : IRecyclablePrefab<R>
	{
		private int _prefabCount = -1;
		public int prefabCount 
		{
			get
			{ 
				if(_prefabCount < 0)
					_prefabCount = Enum.GetNames(typeof(T)).Length;

				return _prefabCount;
			}
		}

		private Dictionary<T, PrefabsRecyclerBase<R>> recyclers = new Dictionary<T, PrefabsRecyclerBase<R>>();

		protected void AddPrefab(T type, R reference)
		{
			PrefabsRecyclerBase<R> rec = new PrefabsRecyclerBase<R>(reference, transform);
			rec.Preinstantiate(10);

			recyclers.Add(type, rec);
		}

		public U GetPrefab<U>(T type) where U : R
		{
			return GetPrefab(type) as U;
		}

		public R GetPrefab(T type, Transform parent = null)
		{
			PrefabsRecyclerBase<R> rec = null;

			recyclers.TryGetValue(type, out rec);

			if(rec == null)
				return null;

			var g = rec.Dequeue();

			if(g == null)
				return null;

			if(parent != null)
			{
				g.parent = parent;
				g.localPosition = Vector3.zero;
			}

			return g;
		}

		public static bool GetPrefabWeaponType(Component prefab, out T type)
		{
			type = default(T);

			try
			{
				type = (T)Enum.Parse(typeof(T), prefab.GetType().Name);
				return true;
			}
			catch(Exception e)
			{
				Debug.LogError("GetPrefabWeaponType " + e);
			}

			return false;
		}

		public void EnqueuePrefab(R prefab)
		{
			T type;

			if(GetPrefabWeaponType(prefab, out type))
			{
				EnqueuePrefab(type, prefab);
			}
		}

		private void EnqueuePrefab(T type, R prefab)
		{
			PrefabsRecyclerBase<R> rec = null;

			recyclers.TryGetValue(type, out rec);

			if(rec != null)
			{
				rec.Enqueue(prefab);
			}
		}
	}
	
}
