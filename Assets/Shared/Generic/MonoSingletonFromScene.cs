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
using System.Collections.Generic;

public abstract class IMonoSingleton : MonoBehaviourTO
{
	public abstract void OnLoaded();
	public virtual void OnLoaded(string _loadedSceneID)
	{

	}
}

public class MonoSingletonFromScene<T> : IMonoSingleton where T : IMonoSingleton
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

	public static void LoadScene(string _sceneID, bool _async = true)
	{
		T _oldInstance = FindObjectOfType(typeof(T)) as T;

		if(_oldInstance != null)
		{
			//Debug.LogWarning("Error, possible memory leak, trying to LoadScene multiple times!");
			_instance = _oldInstance;
		}
		else
		{
			LoadSceneWithoutReferenceCheck(_sceneID, _async);
		}
	}

	public static void LoadSceneWithoutReferenceCheck(string _sceneID, bool _async)
	{
		if(_async)
		{
			MonoSingletonSceneLoader _loader = MonoSingletonSceneLoader.AddLoader();

			if(_loader == null)
			{
				Debug.Log("Error, unable to get instance of MonoSingletonSceneLoader!");
				return;
			}

			_loader.LoadAsyncAdditive(_sceneID);

			_loader.OnSceneLoaded = (string _loadedSceneID) =>
			{
				_instance = FindObjectOfType(typeof(T)) as T;
#if UNITY_EDITOR
				Debug.Log("Getting instance of #" + typeof(T));
#endif

				if(_instance != null)
				{
					_instance.name = "#" + typeof(T);
					_instance.OnLoaded(_sceneID);
				}

				_loader.Destroy();
			};
		}
		else
		{
			Independent.Coroutine.Instance.ProcessCoroutine(SyncLoadLevelAdditive(_sceneID));
		}
	}

	private static IEnumerator SyncLoadLevelAdditive(string _sceneID)
	{
		Application.LoadLevelAdditive(_sceneID);

		int cnt = 0;
		while(_instance == null && cnt++ < 1000)
		{
			_instance = FindObjectOfType(typeof(T)) as T;

			if(_instance != null)
			{
				#if UNITY_EDITOR
				Debug.Log("Getting instance of #" + typeof(T) + " / " + _instance);
				#endif

				_instance.name = "#" + typeof(T);
				_instance.OnLoaded(_sceneID);
			}

			yield return null;
		}
	}

	public static T GetInstance() 
	{
		if(_instance == null) 
		{
			_instance = FindObjectOfType(typeof(T)) as T;
		}

		return _instance;
	}

	public override void OnLoaded(string _loadedSceneID)
	{
#if UNITY_EDITOR
		Debug.Log(name + " loaded!");
#endif
	}

	public override void OnLoaded()
	{
		throw new NotImplementedException();
	}

	public override void Destroy()
	{
		if(this != null && gameObject != null)
			Destroy(gameObject);

		_instance = null;

		//Debug.Log("Destroy " + name + " ref: " + (_instance == null ? "null" : "not null"));
	}

	protected override void OnDestroy()
	{
		//Debug.LogWarning("OnDestroy " + name);

		if(_instance == this)
		{
			_instance = null;
		}
	}
}