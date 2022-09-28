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
using System.Collections.Generic;

namespace GMReloaded
{
	#if DEBUG_GENERIC_CONTAINERS
	[Serializable]
	#endif
	public class GenericObjectContainerMonoSingleton<U, W> : GenericObjectContainerMonoBehaviour<W>
		where W : class
		where U : Component
	{
		protected static U _instance;

		public static bool IsNull { get { return _instance == null; } }

		public static U Instance { get { return GetInstance(); } }

		public static U GetInstance() 
		{
			if(_instance == null) 
			{
				_instance = FindObjectOfType(typeof(U)) as U;

				if(_instance == null) 
				{
					string _objName = "#" + typeof(U).ToString();
					GameObject _obj = Prefabs.LoadInternal(_objName);

					if(_obj == null)
					{
						_obj = new GameObject(_objName);
						_instance = _obj.AddComponent<U>();
					} 
					else
					{
						_obj.name = _objName;
						_instance = _obj.GetComponent<U>();
					}
				}
			}

			return _instance;
		}

		public override void Destroy()
		{
			base.Destroy();

			_instance = null;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if(_instance == this)
			{
				_instance = null;
			}
		}
	}
}
