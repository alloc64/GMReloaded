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
using TouchOrchestra;

public class MonoBehaviourTO : MonoBehaviour 
{
	private Transform _cachedTransform;

	public new Transform transform
	{
		get
		{ 
			if(_cachedTransform == null && this != null)
				_cachedTransform = GetComponent<Transform>();

			return _cachedTransform;
		}		
	}

	private GameObject _cachedGameObject;

	public new GameObject gameObject
	{
		get
		{ 
			if(_cachedGameObject == null && this != null && transform != null)
				_cachedGameObject = transform.gameObject;

			return _cachedGameObject;
		}
	}

	private int _layer = -1;
	private bool _layerSet;
	public int layer
	{
		get
		{ 
			if(!_layerSet && this != null)
			{
				_layer = gameObject.layer;
				_layerSet = true;
			}

			return _layer;
		}

		set
		{ 
			if(_layer == value)
				return;

			_layer = value;

			gameObject.layer = layer;
		}
	}

	public void SetLayerRecursively(int layer)
	{
		SetLayerRecursively (transform, layer);
	}

	private void SetLayerRecursively(Transform tran, int layer)
	{
		tran.gameObject.layer = layer;

		if(tran.GetChildCount() > 0)
			foreach(Transform t in tran)
				SetLayerRecursively(t, layer);
	}

	public bool activeSelf
	{
		get
		{
			if(this == null || gameObject == null)
				return false;

			return gameObject.activeSelf;
		}
	}


	public bool activeInHierarchy
	{
		get
		{
			if(this == null || gameObject == null)
				return false;

			return gameObject.activeInHierarchy;
		}
	}

	public Transform parent
	{
		get
		{
			return (this == null || transform == null) ? null : transform.parent;
		}

		set
		{ 
			if(this != null && transform != null)
				transform.parent = value;
		}
	}


	public virtual Vector3 position
	{
		get
		{
			if(this != null && transform != null)
				return transform.position;

			return Vector3.zero;
		}

		set
		{ 
			if(this != null && transform != null)
				transform.position = value;
		}
	}

	private Vector3 _cachedPosition;
	private	bool _cachedPositionSet;
	public Vector3 cachedPosition
	{
		get
		{
			if(!_cachedPositionSet && this != null && transform != null)
			{
				_cachedPosition = transform.position;
				_cachedPositionSet = true;
			}

			return _cachedPosition;
		}

		set
		{ 
			if(value == _cachedPosition)
				return;

			_cachedPosition = value;
			if(transform != null)
			{
				transform.position = _cachedPosition;
			}
		}
	}

	public Vector3 localPosition
	{
		get
		{
			return (this == null || transform == null) ? Vector3.zero : transform.localPosition;
		}

		set
		{ 
			if(this != null && transform != null)
				transform.localPosition = value;
		}
	}

	public Vector3 localScale
	{
		get
		{
			return (this == null || transform == null) ? Vector3.zero : transform.localScale;
		}

		set
		{ 
			if(this != null && transform != null)
				transform.localScale = value;
		}
	}

	public Vector3 eulerAngles
	{
		get
		{
			return (this == null || transform == null) ? Vector3.zero : transform.eulerAngles;
		}

		set
		{ 
			if(this != null && transform != null)
			{
				Quaternion q = transform.rotation;
				q.eulerAngles = value;

				transform.rotation = q;
			}
		}
	}

	public virtual Vector3 localEulerAngles
	{
		get
		{
			return transform.localEulerAngles;
		}

		set
		{ 
			transform.localEulerAngles = value;
		}
	}

	public virtual Quaternion localRotation
	{
		get
		{
			return (this == null || transform == null) ? Quaternion.identity : transform.localRotation;
		}

		set
		{ 
			if(this != null && transform != null)
				transform.localRotation = value;
		}
	}

	public Quaternion rotation
	{
		get
		{
			return (this == null || transform == null) ? Quaternion.identity : transform.rotation;
		}

		set
		{ 
			if(this != null && transform != null)
				transform.rotation = value;
		}
	}

	public float DistanceTo(Vector2 _position)
	{
		return Vector2.Distance(_position, position);
	}

	protected Localization localization
	{
		get
		{
			return Localization.Instance;
		}
	}

	protected GMReloaded.SoundManager snd
	{
		get { return GMReloaded.SoundManager.GetInstance(); }
	}

	protected GMReloaded.Tutorial.TutorialManager tutorial { get { return GMReloaded.Tutorial.TutorialManager.Instance; } }

	public virtual void Destroy()
	{

	}

	protected virtual void OnDestroy()
	{

	}

	public virtual void SetActive(bool active)
	{
		if(this == null)
			return;

		gameObject.SetActive(active);
	}
}
