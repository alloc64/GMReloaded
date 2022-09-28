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
using System.Collections;
using System.Linq;
using TeamUtility.IO;
using System.IO;

namespace GMReloaded.UI.Final.Settings.Tabs
{
	public class KBInputBindingEntry : KBFocusableGUIItemWithStateChange
	{
		public enum Direction
		{
			None,
			Positive,
			Negative
		}

		[SerializeField]
		private tk2dTextMesh nameTextMesh;

		[SerializeField]
		private tk2dTextMesh keyTextMesh;

		//

		private AxisConfiguration axis;

		private Direction direction;

		//

		private IKBInputBindingTab inputBindingTab;

		//

		public override void OnClick(bool keyboardInput)
		{
			base.OnClick(keyboardInput);

			SetMainKey("Type..");

			switch(axis.type)
			{
				case InputType.DigitalAxis:
				case InputType.AnalogAxis:
					InputManager.StartJoystickAxisScan(HandleAxisScan, 0, 10f, null, null);
				break;

				default:
					InputManager.StartKeyScan(HandleKeyScan, 10.0f, null, axis.name, true, direction != Direction.Negative);
				break;
			}

		}

		public void SetAxis(AxisConfiguration axis, Direction direction = Direction.None)
		{
			this.axis = axis;
			this.direction = direction;

			if(nameTextMesh != null)
			{
				string name = axis.name;

				if(axis.type != InputType.AnalogAxis && direction != Direction.None)
					name += direction;

				nameTextMesh.text = localization.GetValue(name);
			}
		}

		public void SetMainKey(KeyCode key)
		{
			SetMainKey(key.ToString());
		}

		private void SetMainKey(string key)
		{
			if(keyTextMesh != null)
			{
				System.Text.StringBuilder builder = new System.Text.StringBuilder();
				foreach (char c in key) 
				{
					if (System.Char.IsUpper(c) && builder.Length > 0) 
						builder.Append(' ');

					builder.Append(c);
				}

				builder.Replace("Joystick Button", "JB");
				builder.Replace("Alpha", "");

				keyTextMesh.text = localization.GetValue(builder.ToString());
			}
		}

		public void SetMainAnalogAxis(int axis)
		{
			SetMainKey("A" + axis);
		}

		public void Setup(IKBInputBindingTab inputBindingTab)
		{
			this.inputBindingTab = inputBindingTab;
		}

		#region Scan Handlers

		private bool HandleAxisScan(int a, object[] userData)
		{
			Debug.Log("HandleAxisScan " + a);

			if(a >= 0)
			{
				axis.SetAnalogAxis(0, a); 

				SetMainAnalogAxis(a);

				if(inputBindingTab != null)
					inputBindingTab.SaveInputs();
				
				return true;
			}
			else
			{
				SetMainAnalogAxis(axis.axis);
			}

			return false;
		}

		private bool HandleKeyScan(KeyCode key, object[] args)
		{
			bool primary = (bool)args[1];
			bool positive = (bool)args[2];

			if(positive)
			{
				if(primary)
					axis.positive = key == KeyCode.Escape ? KeyCode.None : key;
				else
					axis.altPositive = key == KeyCode.Escape ? KeyCode.None : key;
			}
			else
			{
				if(primary)
					axis.negative = key == KeyCode.Escape ? KeyCode.None : key;
				else
					axis.altNegative = key == KeyCode.Escape ? KeyCode.None : key;
			}

			SetMainKey(key);

			if(inputBindingTab != null)
				inputBindingTab.SaveInputs();


			return true;
		}

		#endregion
	}
}