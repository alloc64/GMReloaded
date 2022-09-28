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

public class MonoSingletonSceneLoader : MonoBehaviour
{
	public Action<string> OnSceneLoaded;

	public static MonoSingletonSceneLoader AddLoader()
	{
		GameObject _obj = new GameObject("#MonoSingletonSceneLoader");
		DontDestroyOnLoad(_obj);

		return _obj.AddComponent<MonoSingletonSceneLoader>();
	}

	public void LoadAsync(string _levelID)
	{
		AsyncOperation _operation = Application.LoadLevelAsync(_levelID);

		if(_operation == null)
		{
			Debug.LogError("Error, level " + _levelID + " couldnt be loaded!");
			return;
		}

		StartCoroutine(LoadLevelAsyncProgress(_operation, _levelID));
	}

	public void LoadAsyncAdditive(string _levelID)
	{
		AsyncOperation _operation = Application.LoadLevelAdditiveAsync(_levelID);

		if(_operation == null)
		{
			Debug.LogError("Error, level " + _levelID + " couldnt be loaded!");
			return;
		}

		StartCoroutine(LoadLevelAsyncProgress(_operation, _levelID));
	}

	//

	private IEnumerator LoadLevelAsyncProgress(AsyncOperation _operation, string _levelID)
	{
		_operation.priority = 10;

		while(!_operation.isDone)
		{
			yield return null;
		}

		float _pausedTime = Time.realtimeSinceStartup + 0.7f;
		while(Time.realtimeSinceStartup < _pausedTime)
		{
			yield return null;
		}

		if(OnSceneLoaded != null)
			OnSceneLoaded(_levelID);
	}

	public void Destroy()
	{
		Destroy(gameObject);
	}
}
