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


using System;
using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Prefabs : MonoSingleton<Prefabs>
{
	public static T Load<T>(Component component, Transform parent) where T : Component
	{
		if(component == null)
			return null;

		return Load<T>(component.gameObject, parent);
	}

	public static T Load<T>(GameObject loadGameObject, Transform parent) where T : Component
	{
		if(loadGameObject == null)
			return null;

		GameObject go = GameObject.Instantiate(loadGameObject) as GameObject;

		if(go == null)
			return null;

		Vector3 _lastLocalPos = go.transform.localPosition;
		go.transform.parent = parent;
		go.transform.localPosition = new Vector3(0.0f, 0.0f, _lastLocalPos.z);

		return go.GetComponent<T>();
	}

	public static GameObject Load(string _path, Transform _parent)
	{
		GameObject _object = Load(_path);

		if(_object == null)
			return null;
		
		Vector3 _lastLocalPos = _object.transform.localPosition;
		_object.transform.parent = _parent;
		_object.transform.localPosition = new Vector3(0.0f, 0.0f, _lastLocalPos.z);
		
		return _object;
	}

	public static GameObject Load(string _path)
	{
		UnityEngine.Object _prefab = Resources.Load(_path);
		
		if(_prefab == null)
		{
			Debug.Log("Error, prefab " + _path + " not found!");
			return null;
		}	

		return GameObject.Instantiate(_prefab) as GameObject;
	}

	public static T Load<T>(string _path, Transform _parent = null) where T : Component
	{
		GameObject _prefab = Load(_path, _parent);

		if(_prefab == null)
			return null;

		return _prefab.GetComponent<T>();
	}

	public static GameObject LoadInternal(string _path)
	{
		UnityEngine.Object _prefab = Resources.Load(_path);

		if(_prefab == null)
		{
			return null;
		}	

		return GameObject.Instantiate(_prefab) as GameObject;
	}
	
	private static readonly Vector3[] boxExtents = new Vector3[] 
	{
        new Vector3(-1, -1, -1),
        new Vector3( 1, -1, -1),
        new Vector3(-1,  1, -1),
        new Vector3( 1,  1, -1),
        new Vector3(-1, -1,  1),
        new Vector3( 1, -1,  1),
        new Vector3(-1,  1,  1),
        new Vector3( 1,  1,  1),
    };

	
	public static Vector3 GetMinMax(Transform _t)
	{
		Vector3 vector3Min = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		Vector3 vector3Max = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3[] minMax = new Vector3[] {
			vector3Max,
			vector3Min
		};

		GetRendererBoundsInChildren(_t.worldToLocalMatrix, minMax, _t);

		Debug.Log(minMax[0]);
		Debug.Log(minMax[1]);

		return new Vector3(minMax[1].x - minMax[0].x, minMax[1].y - minMax[0].y, minMax[1].z - minMax[0].z);
	}

	/// <summary>
	/// Loads the component from a prefab file.
	/// </summary>
	/// <returns>The component.</returns>
	/// <param name="_path">_path in resources folder.</param>
	/// <typeparam name="T">The component type.</typeparam>
	public static T LoadComponentFromPrefab<T>(string _path) where T : MonoBehaviour{
		UnityEngine.Object _prefab = Resources.Load(_path);

		if (_prefab == null) {
			Debug.LogError ("The prefab: "+_path+" does not exist.");
			return null;
		} else if (_prefab is GameObject) {
			GameObject _go = (GameObject)_prefab;

			T component = _go.GetComponent<T> ();

			if (component == null) {
				Debug.LogError ("The prefab: "+_path+" does not contain an component of type: "+typeof(T).ToString()+".");
				return null;
			}

			return component;
		} else {
			Debug.LogError ("The prefab: "+_path+" is not a valid game object.");
			return null;
		}
	}

	/// <summary>
	/// Creates an prefab with the specified component type at path in the Assets/Game/Resources folder.</br>
	/// Throws an error if this function is ran outside UnityEditor.
	/// </summary>
	/// <typeparam name="T">The type of component usualy a MonoBehaviour extension.</typeparam>
	public static void CreateComponentPrefab<T>(string _path) where T : MonoBehaviour {
		#if UNITY_EDITOR
		if(!Application.isPlaying)
        {
			GameObject _gameObject = new GameObject("Something went wrong :((");

			_gameObject.AddComponent<T>();

			PrefabUtility.CreatePrefab("Assets/Game/Resources/"+_path+".prefab", _gameObject);

			DestroyImmediate(_gameObject);
		}
		else
		{
            Debug.LogError("You're creating new assets in play mode, you might be doing something wrong.");
		}
		#else
		{
			Debug.LogError("Trying to create an prefab outside Unityeditor.");
		}
		#endif
	}


	public static void GetRendererBoundsInChildren(Matrix4x4 rootWorldToLocal, Vector3[] minMax, Transform t) 
	{
        MeshFilter mf = t.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null) {
            Bounds b = mf.sharedMesh.bounds;
            Matrix4x4 relativeMatrix = rootWorldToLocal * t.localToWorldMatrix;
            for (int j = 0; j < 8; ++j) {
                Vector3 localPoint = b.center + Vector3.Scale(b.extents, boxExtents[j]);
                Vector3 pointRelativeToRoot = relativeMatrix.MultiplyPoint(localPoint);
                minMax[0] = Vector3.Min(minMax[0], pointRelativeToRoot);
                minMax[1] = Vector3.Max(minMax[1], pointRelativeToRoot);
            }
        }
        int childCount = t.childCount;
        for (int i = 0; i < childCount; ++i) {
            Transform child = t.GetChild(i);
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
            if (t.gameObject.active) {
#else
            if (t.gameObject.activeSelf) {
#endif
                GetRendererBoundsInChildren(rootWorldToLocal, minMax, child);
            }
        }
    }  
}
