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

namespace TouchOrchestra
{
	public class ObjectPool<T, U> : MonoSingleton<U> 
		where T : IPooledObject 
		where U : Component
	{
		public T baseObject { get; private set; }

		public bool allowPostObjectCreation { get; protected set; }

		protected Queue<IPooledObject> objectQueue = new Queue<IPooledObject>();

		public void PrepoolObject(T go, int numObjects)
		{
			if(go == null)
			{
				Debug.LogError("ObjectPool: Unable prepool NULL object!");
				return;
			}

			if(numObjects < 1)
			{
				Debug.LogError("ObjectPool: Unable prepool zero or negative number of objects!");
				return;
			}

			this.baseObject = go;

			for(int i = 0; i < numObjects; i++)
			{
				var newInstance = CreateObject();

				EnqueueUnusedObject(newInstance);
			}
		}

		private T CreateObject()
		{
			T newInstance = Prefabs.Load<T>(this.baseObject, transform);

			if(newInstance == null)
			{
				Debug.Log("ObjectPool: Unable create pooled object instance!");
				return null;
			}

			return newInstance;
		}

		public T GetObject(Transform parent, Vector3 localPosition, Vector3 localScale)
		{
			T objectInstance = null;

			while(objectInstance == null && objectQueue.Count > 0)
				objectInstance = objectQueue.Dequeue() as T;

			if(allowPostObjectCreation && objectInstance == null)
				objectInstance = CreateObject();

			if(objectInstance != null)
			{
				objectInstance.parent = parent;
				objectInstance.localScale = localScale;
				objectInstance.localPosition = localPosition;

				objectInstance.Reset();
			}

			return objectInstance;
		}

		public void EnqueueUnusedObject(IPooledObject objectInstance)
		{
			if(objectInstance == null)
				return;

			objectInstance.SetActive(false);
			objectInstance.parent = transform;

			objectQueue.Enqueue(objectInstance);
		}
	}
}
