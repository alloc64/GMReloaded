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

namespace GMReloaded.UI.Final.Settings.Tabs
{
	public class KBGraphicsTab : GMReloaded.UI.Final.Tabs.KBTab, IKBFocusableSuccessorClickReceiver
	{
		[SerializeField]
		private KBFocusableChooser resolutionChooser;

		[SerializeField]
		private KBFocusableChooser qualityChooser;

		[SerializeField]
		private KBFocusableCheckBox fullscreenCheckBox;

		[SerializeField]
		private KBFocusableChooser dofChooser;

		[SerializeField]
		private KBFocusableCheckBox hdrCheckBox;

		[SerializeField]
		private KBFocusableCheckBox antiAliasingCheckBox;

		//

		public class KBResolution : IEquatable<KBResolution>
		{
			public KBResolution(int width, int height)
			{
				this.width = width;
				this.height = height;
			}

			public KBResolution(Resolution res) : this(res.width, res.height)
			{
			}

			public static KBResolution windowResolution { get { var r = new KBResolution(Screen.width, Screen.height); r.currentResolution = true; return r; } }
			public static KBResolution nativeResolution { get { var r = new KBResolution(Screen.currentResolution); r.currentResolution = true; return r; } }

			public int width { get; set; }

			public int height { get; set; }

			public bool currentResolution { get; set; }

			#region IEquatable implementation

			public bool Equals(KBResolution other)
			{
				return this != null && other != null && width == other.width && height == other.height;
			}

			#endregion

			//

			public byte[] Serialize()
			{
				return StructSerializer.Serialize((bw) =>
				{
					bw.Write(width);
					bw.Write(height);
				});
			}

			public static KBResolution Deserialize(byte[] bytes)
			{
				return StructSerializer.Deserialize<KBResolution>(bytes, (br) =>
				{
					return new KBResolution(br.ReadInt32(), br.ReadInt32());
				});
			}

			//

			public override string ToString()
			{
				return width + "x" + height;
			}
		}

		//

		private List<KBResolution> resolutions = new List<KBResolution>();

		//

		#region Unity

		protected override void OnEnable()
		{
			base.OnEnable();

			UpdateResolutions();
			UpdateQuality();
			UpdateFullscreen();
			UpdateDOF();
			UpdateHDR();
			UpdateAntiAliasing();
		}

		protected override void Awake()
		{
			base.Awake();

			foreach(var fi in FocusableItems)
				if(fi != null)
					fi.RegisterClickReceiver(this);
		}

		private void OnApplicationFocus(bool paused)
		{
			if(!paused)
			{
				UpdateResolutions();
				UpdateFullscreen();
				UpdateDOF();
				UpdateHDR();
				UpdateAntiAliasing();
			}
		}

		#endregion

		#region Events

		public void OnClick(KBFocusableGUIItem guiItem)
		{
			switch(guiItem.name)
			{
				case "Apply":
					ApplyChanges();
				break;
			}
		}

		#endregion

		#region Settings Selectors

		private void UpdateResolutions()
		{
			if(resolutionChooser == null)
			{
				Debug.LogError("resolutionChooser == null");
				return;
			}

			resolutions.Clear();
			resolutionChooser.ClearItems();

			KBResolution currResolution = KBResolution.windowResolution;

			bool currResolutionFound = false;

			for(int i = 0; i < Screen.resolutions.Length; i++)
			{
				var res = Screen.resolutions[i];

				var kbRes = new KBResolution(res);

				if(resolutions.Contains(kbRes))
					continue;
				
				if(kbRes.Equals(currResolution))
				{
					kbRes.currentResolution = currResolutionFound = true;
				}

				resolutions.Add(kbRes);
			}

			if(!currResolutionFound)
			{
				resolutions.Add(currResolution);
			}

			resolutions = resolutions.OrderBy(r => r.width).ToList();
				
			//

			int currResolutionIdx = 0;

			for(int i = 0; i < resolutions.Count; i++)
			{
				var r = resolutions[i];

				if(r != null)
				{
					if(r.currentResolution)
						currResolutionIdx = i;

					resolutionChooser.AddItem(r.ToString());
				}
			}

			resolutionChooser.NotifyItemsChanged();
			resolutionChooser.SetListIndex(currResolutionIdx);
		}

		private void UpdateQuality()
		{
			if(qualityChooser == null)
			{
				Debug.LogError("qualityChooser == null");
				return;
			}

			qualityChooser.ClearItems();

			string[] qualityNames = QualitySettings.names;

			for(int i = 0; i < qualityNames.Length; i++)
			{
				qualityChooser.AddItemLocalized("Quality_" + qualityNames[i]);
			}

			qualityChooser.NotifyItemsChanged();

			qualityChooser.SetListIndex(Config.ClientPersistentSettings.Quality);
		}

		private void UpdateFullscreen()
		{
			if(fullscreenCheckBox == null)
			{
				Debug.LogError("fullscreenCheckBox == null");
				return;
			}

			fullscreenCheckBox.IsOn = Screen.fullScreen;
		}

		private void UpdateDOF()
		{
			if(dofChooser == null)
			{
				Debug.LogError("dofChooser == null");
				return;
			}

			dofChooser.ClearItems();

			foreach(Config.ClientPersistentSettings.DOFAmount suit in Enum.GetValues(typeof(Config.ClientPersistentSettings.DOFAmount))) 
			{
				dofChooser.AddItemLocalized("DOF_Amount_" + suit);
			}

			dofChooser.NotifyItemsChanged();

			dofChooser.SetListIndex((int)Config.ClientPersistentSettings.DOF);
		}

		private void UpdateHDR()
		{
			if(hdrCheckBox == null)
			{
				Debug.LogError("hdrCheckBox == null");
				return;
			}

			hdrCheckBox.IsOn = Config.ClientPersistentSettings.HDR;
		}

		private void UpdateAntiAliasing()
		{
			if(antiAliasingCheckBox == null)
			{
				Debug.LogError("antiAliasingCheckBox == null");
				return;
			}

			antiAliasingCheckBox.IsOn = Config.ClientPersistentSettings.AntiAliasing;
		}

		//

		#endregion

		#region Apply


		private void ApplyChanges()
		{
			Debug.Log("Apply graphics settings...");

			if(fullscreenCheckBox != null)
			{
				Config.ClientPersistentSettings.Fullscreen = fullscreenCheckBox.IsOn;
			}
			else
			{
				Debug.LogError("Failed to update quality - fullscreenCheckBox == null");
			}

			int resIdx = resolutionChooser.Index;

			if(resolutions != null && resIdx >= 0 && resIdx < resolutions.Count)
			{
				SaveSelectedResolution(resolutions[resIdx]);
			}
			else
			{
				Debug.LogError("Failed to update resolution - invalid resIdx");
			}

			if(qualityChooser != null)
			{
				int selectedQuality = qualityChooser.Index;

				if(selectedQuality >= 0)
					SetSelectedQuality(selectedQuality);
			}
			else
			{
				Debug.LogError("Failed to update quality - qualityChooser == null");
			}

			if(dofChooser != null)
			{
				int selectedDOF = dofChooser.Index;

				if(selectedDOF >= 0)
					SetDOFAmount((Config.ClientPersistentSettings.DOFAmount)selectedDOF);
			}
			else
			{
				Debug.LogError("Failed to update quality - dofCheckBox == null");
			}

			if(hdrCheckBox != null)
			{
				SetHDREnabled(hdrCheckBox.IsOn);
			}
			else
			{
				Debug.LogError("Failed to update quality - hdrCheckBox == null");
			}

			if(antiAliasingCheckBox != null)
			{
				SetAntiAliasingEnabled(antiAliasingCheckBox.IsOn);
			}
			else
			{
				Debug.LogError("Failed to update quality - antiAliasingCheckBox == null");
			}
			
			//menuRenderer.GoBack();
		}

		private void SaveSelectedResolution(KBResolution selectedResolution)
		{
			var currentResolution = KBResolution.windowResolution;

			bool fullscreen = Config.ClientPersistentSettings.Fullscreen;

			if(currentResolution.Equals(selectedResolution) && Screen.fullScreen == fullscreen)
			{
				Debug.Log("Ignoring resolution save - the same as old config");
				return;
			}

			Screen.SetResolution(selectedResolution.width, selectedResolution.height, fullscreen);

			Config.ClientPersistentSettings.Resolution = selectedResolution;

			Debug.Log("Saving resolution " + selectedResolution + "...");
		}

		private static void SetSelectedQuality(int quality)
		{
			QualitySettings.SetQualityLevel(quality);
			Config.ClientPersistentSettings.Quality = quality;

			Debug.Log("Saving quality " + ((QualityLevel)quality) + "...");
		}


		private static void SetDOFAmount(Config.ClientPersistentSettings.DOFAmount amount)
		{
			Config.ClientPersistentSettings.DOF = amount;

			if(LocalClientRobotEmil.Instance != null)
				LocalClientRobotEmil.Instance.UpdateScreenEffectAvailability(LocalClientRobotEmil.ScreenEffects.DOF);
			
			Debug.Log("Setting DOF to " + amount + "...");
		}

		private void SetHDREnabled(bool hdrEnabled)
		{
			Config.ClientPersistentSettings.HDR = hdrEnabled;

			if(LocalClientRobotEmil.Instance != null)
				LocalClientRobotEmil.Instance.UpdateScreenEffectAvailability(LocalClientRobotEmil.ScreenEffects.HDR);
			
			Debug.Log("Setting HDR to " + hdrEnabled + "...");
		}

		private void SetAntiAliasingEnabled(bool aaEnabled)
		{
			Config.ClientPersistentSettings.AntiAliasing = aaEnabled;

			if(LocalClientRobotEmil.Instance != null)
				LocalClientRobotEmil.Instance.UpdateScreenEffectAvailability(LocalClientRobotEmil.ScreenEffects.AntiAliasing);
			
			Debug.Log("Setting AntiAliasing to " + aaEnabled + "...");
		}

		#endregion


		#region Presets

		public static void SetPresetValues()
		{
			// resolution
			var currentResolution = KBResolution.windowResolution;
			var selectedResolution = Config.ClientPersistentSettings.Resolution;
			var fullscreen = Config.ClientPersistentSettings.Fullscreen;

			if(!currentResolution.Equals(selectedResolution) || Screen.fullScreen != fullscreen)
			{
				Debug.Log("Reseting window resolution to " + selectedResolution);
				Screen.SetResolution(selectedResolution.width, selectedResolution.height, fullscreen);
			}

			// quality
			QualitySettings.SetQualityLevel(Config.ClientPersistentSettings.Quality);
		}

		#endregion
	}
}