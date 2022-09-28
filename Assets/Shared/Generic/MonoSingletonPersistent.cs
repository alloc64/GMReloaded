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


public class MonoSingletonPersistent<T> : IMonoSingleton where T : IMonoSingleton
{
	protected static T _instance;

	public static bool IsNull
	{
		get
		{
			return _instance == null;
		}
	}

	public static T Instance
	{
		get
		{
			return GetInstance();
		}
	}

	public static T GetInstance() 
	{
		if(_instance == null) 
		{
			_instance = FindObjectOfType(typeof(T)) as T;

			if(_instance == null) 
			{
				string _objName = "#" + typeof(T).ToString();
				GameObject _obj = Prefabs.LoadInternal(_objName);

				if(_obj == null)
				{
					_obj = new GameObject(_objName);
					_instance = _obj.AddComponent<T>();
				} 
				else
				{
					_obj.name = _objName;
					_instance = _obj.GetComponent<T>();
				}

				_instance.OnLoaded();
				
				//Debug.Log(_objName + " initialized!");
			}
		}
		
		return _instance;
	}

	public override void OnLoaded()
	{

	}

	protected virtual void Awake()
	{
		DontDestroyOnLoad(this.gameObject);
		
		if(_instance == null)
			_instance = this as T;
		else
			Destroy(gameObject);
	}

	public override void Destroy()
	{
		if(gameObject != null)
			Destroy(gameObject);

		_instance = null;

		//Debug.Log("Destroy " + name + " ref: " + (_instance == null ? "null" : "not null"));
	} 

	protected override void OnDestroy()
	{
		if(_instance == this)
		{
			_instance = null;
		}
	}

    public static void MakeSureExists()
    {
        if(Instance == null){}
    }

}