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

namespace GMReloaded.Tutorial
{
	public class TutorialSceneObjectsManager : MonoBehaviourTO
	{
		[SerializeField]
		private List<GameObject> objectsList = new List<GameObject>();

		private Dictionary<string, GameObject> _objects = null;
		public Dictionary<string, GameObject> objects
		{
			get
			{
				if(_objects == null)
				{
					_objects = new Dictionary<string, GameObject>();

					foreach(var o in objectsList)
					{
						if(o != null)
							_objects[o.name] = o;
					}
				}

				return _objects;
			}
		}

		//

		public GameObject GetObject(string name)
		{
			GameObject go = null;
		
			objects.TryGetValue(name, out go);

			if(go == null)
				Debug.LogError("GetObject " + name + " - not found");

			return go;
		}

		public T GetObject<T>(string name)
		{
			var go = GetObject(name);

			return go == null ? default(T) : go.GetComponent<T>();
		}

		public List<T> GetObjects<T>(string[] names)
		{
			List<T> objects = new List<T>();

			foreach(var name in names)
			{
				objects.Add(GetObject<T>(name));
			}

			return objects;
		}

		//

		public void SetObjectActive(string name, bool active)
		{
			var go = GetObject(name);

			if(go != null)
				go.SetActive(active);
		}
	}
	
}