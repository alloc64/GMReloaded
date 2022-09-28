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
using System.Reflection;

namespace TouchOrchestra
{
	public class UISwipeController : MonoBehaviour
	{	
		public Action<Touch> OnTouchBeganEvent;
		public Action<Touch> OnTouchMovedEvent;
		public Action<Touch> OnTouchEndedEvent;

		public enum UIMouseState
		{
			DownThisFrame,
			HeldDown,
			UpThisFrame
		}
	
		#if !NETFX_CORE

	#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER || UNITY_METRO
		private Vector2? lastMousePosition;
	#endif

		protected void Update()
		{
			if(Input.touchCount > 0)
			{
				// Examine all current touches
				for(int i = 0; i < Input.touchCount; i++)
				{
					LookAtTouch(Input.GetTouch(i));
				}
			} 
	#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER || UNITY_METRO
			else
			{
				// no touches. so check the mouse input if we are in the editor

				// check for each mouse state in turn, no elses here. They can all occur on the same frame!
				if(Input.GetMouseButtonDown(0))
				{
					LookAtTouch(CreateTouchFromInput(UIMouseState.DownThisFrame, ref lastMousePosition));
				}

				if(Input.GetMouseButton(0))
				{
					LookAtTouch(CreateTouchFromInput(UIMouseState.HeldDown, ref lastMousePosition));
				}

				if(Input.GetMouseButtonUp(0))
				{
					LookAtTouch(CreateTouchFromInput(UIMouseState.UpThisFrame, ref lastMousePosition));
				}                        
			}
	#endif
		}

		#endif

		protected virtual void OnTouchBegan(Touch touch)
		{
			if(OnTouchBeganEvent != null)
				OnTouchBeganEvent(touch);
		}

		protected virtual void OnTouchMoved(Touch touch)
		{
			if(OnTouchMovedEvent != null)
				OnTouchMovedEvent(touch);
		}

		protected virtual void OnTouchEnded(Touch touch)
		{
			if(OnTouchEndedEvent != null)
				OnTouchEndedEvent(touch);
		}

		#if !NETFX_CORE
		private void LookAtTouch(Touch touch)
		{
			if(touch.phase == TouchPhase.Began)
			{
				OnTouchBegan(touch);
			} 
			else if(touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
			{
				OnTouchMoved(touch);
			} 
			else if(touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
			{
				OnTouchEnded(touch);                       
			}
		}

		private Type _type;
		private Type type
		{ 
			get
			{
				if(_type == null)
					_type = typeof(Touch);

				return _type;
			}
		}

		private FieldInfo _positionDelta;
		private FieldInfo positionDelta 
		{
			get
			{
				if (_positionDelta == null)
					_positionDelta = type.GetField("m_PositionDelta", BindingFlags.Instance | BindingFlags.NonPublic);

				return _positionDelta;
			}
		}

		private FieldInfo _phase;
		private FieldInfo phase
		{
			get
			{
				if (_phase == null)
					_phase = type.GetField("m_Phase", BindingFlags.Instance | BindingFlags.NonPublic);

				return _phase;
			}
		}

		private FieldInfo _position;
		private FieldInfo position
		{
			get
			{
				if (_position == null)
					_position = type.GetField("m_Position", BindingFlags.Instance | BindingFlags.NonPublic);

				return _position;
			}
		}

		private Touch CreateTouchFromInput(UIMouseState mouseState, ref Vector2? lastMousePosition)
		{
			var self = new Touch();
			ValueType valueSelf = self;

			var currentMousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

			// if we have a lastMousePosition use it to get a delta
			if (lastMousePosition.HasValue)
			{ 
				if(positionDelta != null)
					positionDelta.SetValue(valueSelf, currentMousePosition - lastMousePosition);
			}

			if(mouseState == UIMouseState.DownThisFrame) // equivalent to touchBegan
			{
				if(phase != null)
					phase.SetValue(valueSelf, TouchPhase.Began);

				lastMousePosition = Input.mousePosition;
			} 
			else if(mouseState == UIMouseState.UpThisFrame) // equivalent to touchEnded
			{
				if(phase != null)
					phase.SetValue(valueSelf, TouchPhase.Ended);

				lastMousePosition = null;
			} 
			else // UIMouseState.HeldDown - equivalent to touchMoved/Stationary
			{
				if(phase != null)
					phase.SetValue(valueSelf, TouchPhase.Moved);

				lastMousePosition = Input.mousePosition;
			}

			if(position != null)
				position.SetValue(valueSelf, currentMousePosition);

			return (Touch)valueSelf;
		}

		#endif
	}
}
