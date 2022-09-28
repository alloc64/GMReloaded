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
using System.Text;

namespace GMReloaded.UI.Final.Settings.Tabs
{
	public class KBInputBindingTab : GMReloaded.UI.Final.Tabs.KBTab, IKBInputBindingTab
	{
		[SerializeField]
		private KBInputBindingEntry baseKeyEntryTemplate;

		[SerializeField]
		private Transform entryContainer;

		//

		private const string defaultInputConfigKey = "GM_InputConfigDefault";

		private const string inputConfigKey = "GM_InputConfig";

		private string inputsConfiguration;

		private float inputBindingEntryOffset;
		private int inputBindingEntryCounter = -1;

		//

		protected void LoadInputs(string configuration)
		{
			LoadInputs(inputConfigKey, configuration);
		}

		private void LoadInputs(string key, string configuration)
		{
			this.inputsConfiguration = configuration;

			if(PlayerPrefs.HasKey(key))
			{
				string xml = PlayerPrefs.GetString(key);
				using(TextReader reader = new StringReader(xml))
				{
					InputLoaderXML loader = new InputLoaderXML(reader);
					InputManager.Load(loader);
				}
			}

			//TODO: zabstranit aji pro gamepady
			foreach(var axis in InputManager.GetInputConfiguration(configuration).axes.OrderBy(a => a.name))
			{
				switch(axis.name)
				{
					case "DPADVertical":
					case "DPADHorizontal":
					case "LeftTrigger":
					case "RightTrigger":
					case "Mouse X":
					case "Mouse Y":
						continue;
				}

				switch(axis.type)
				{
					case InputType.Button:
						SetupButton(axis);
					break;

					case InputType.AnalogAxis:
						SetupAnalogAxis(axis);
					break;
						
					case InputType.DigitalAxis:
						SetupDigitalAxis(axis);
					break;

					case InputType.AnalogButton:
						//TODO:
					break;

					case InputType.MouseAxis:
						// nezajima nas
					break;

					case InputType.RemoteAxis:
					case InputType.RemoteButton:
						Debug.LogWarning("Input edit not implemented " + axis.type);
					break;
				}
			}

			if(baseKeyEntryTemplate != null)
				baseKeyEntryTemplate.SetActive(false);
		}

		#region Keyboard Entry

		private void SetupButton(AxisConfiguration axis)
		{
			var entry = SetInputAxisGeneric(axis);

			entry.SetAxis(axis);
			entry.SetMainKey(axis.positive);
			//entry.SetSecondaryKey(axis.altPositive);
		}

		private void SetupAnalogAxis(AxisConfiguration axis)
		{
			var positive = SetInputAxisGeneric(axis);
			positive.SetAxis(axis, KBInputBindingEntry.Direction.Positive);
			positive.SetMainAnalogAxis(axis.axis);
		}

		private void SetupDigitalAxis(AxisConfiguration axis)
		{
			var positive = SetInputAxisGeneric(axis);
			positive.SetAxis(axis, KBInputBindingEntry.Direction.Positive);
			positive.SetMainKey(axis.positive);

			var negative = SetInputAxisGeneric(axis);
			negative.SetAxis(axis, KBInputBindingEntry.Direction.Negative);
			negative.SetMainKey(axis.negative);
		}

		private KBInputBindingEntry SetInputAxisGeneric(AxisConfiguration axis)
		{
			var entry = Utils.CloneItem<KBInputBindingEntry>(baseKeyEntryTemplate, entryContainer);

			entry.Setup(this);

			var lp = entry.localPosition;

			lp.y = inputBindingEntryOffset;

			if(inputBindingEntryCounter % 2 == 0)
			{
				inputBindingEntryOffset -= 0.125f;
				lp.x = 0.607f;
			}

			entry.localPosition = lp;

			inputBindingEntryCounter++;

			RegisterFocusableItem(entry);

			return entry;
		}

		#endregion

		#region Save Inputs

		public void SaveInputs()
		{
			SaveInputs(inputConfigKey);
		}

		private void SaveInputs(string key)
		{
			StringBuilder output = new StringBuilder();
			InputSaverXML saver = new InputSaverXML(output);
			InputManager.Save(saver);

			PlayerPrefs.SetString(key, output.ToString());
		}

		private void SaveDefaultInputs()
		{
			if(!PlayerPrefs.HasKey(defaultInputConfigKey))
				SaveInputs(defaultInputConfigKey);
		}

		#endregion

		private void ResetInputsPrompt()
		{
			if(popup != null)
			{
				popup.OnNegativeButtonClicked = () => {};
				popup.OnPositiveButtonClicked = ResetInputs;

				popup.SetTitle("AB_Title");
				popup.SetText("ST_ResetDefaultKeysPrompt");

				popup.SetAlertType(GMReloaded.UI.Final.Popup.KBPopup.Type.YesNO);

				popup.Show();
			}
		}

		private void ResetInputs()
		{
			// TODO: reset neresetuje interni inputy - resetne se to po reloadu appky

			if(string.IsNullOrEmpty(inputsConfiguration))
				return;

			PlayerPrefs.DeleteKey(inputConfigKey);

			LoadInputs(defaultInputConfigKey, inputsConfiguration);
			SaveInputs();
		}
	}
	
}