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
using System;
using System.Collections.Generic;

namespace GMReloaded
{
	public class PrefabsRecyclerBase<T> where T : IRecyclablePrefab<T>
	{
		protected Queue<T> prefabQueue = new Queue<T>();

		protected T basePrefab;

		private int prefabCounter = 0;

		public PrefabsRecyclerBase() {}

		public PrefabsRecyclerBase(T basePrefab, Transform parent = null) 
		{
			this.SetParent(parent);
			this.Initialize(basePrefab); 
		}

		protected Transform parent;

		public virtual void Initialize(T basePrefab)
		{
			if(basePrefab == null)
			{
				throw new ArgumentNullException("_basePrefab");
			}

			this.basePrefab = basePrefab;
		}

		public virtual void SetParent(Transform parent)
		{
			this.parent = parent;
		}

		public virtual void Preinstantiate(int preinstantatedCount)
		{
			if(basePrefab == null)
			{
				Debug.Log("Preinstantiate - basePrefab not initialized");
				return;
			}

			for(int i = 0; i < preinstantatedCount; i++)
			{
				Enqueue(ClonePrefab());
			}
		}

		public virtual T Dequeue()
		{
			if(basePrefab == null)
			{
				throw new ArgumentNullException("basePrefab");
			}

			T prefab = null;

			while(prefabQueue.Count > 0 && prefab == null)
				prefab = prefabQueue.Dequeue();

			if(prefab == null)
				prefab = ClonePrefab();

			if(prefab != null)
			{
				prefab.InitializeRecycler(this);
				prefab.Reinstantiate();
			}

			return prefab;
		}

		private T ClonePrefab()
		{
			if(basePrefab == null)
				return null;

			T prefab = GameObject.Instantiate(basePrefab) as T;

			if(prefab == null)
				return null;

			prefab.name = prefab.name.Replace("(Clone)", "") + " - " + prefabCounter++;
			prefab.transform.parent = parent;

			return prefab;
		}

		public void Enqueue(T prefab)
		{
			if(prefab == null)
				return;

			prefab.SetActive(false);
			prefab.transform.parent = parent;

			prefabQueue.Enqueue(prefab);
		}
	}
	
}
