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
#if UNITY_XBOXONE
using GMReloaded.Xbox.Linq;
#else
using System.Linq;
#endif
using System.Collections.Generic;

namespace GMReloaded.UI.Final.CreateGame.MadnessMode
{
	public class KBMadnessMode : KBFocusableSuccessorsGUI
	{
		[SerializeField]
		private KBMadnessModeTimeline madnessModeTimeline;

		[SerializeField]
		private KBServerController serverController;

		[SerializeField]
		private KBMadnessModeRow madnessModeRowTemplate;

		[SerializeField]
		private tk2dUIScrollableArea scrollArea;

		//

		[SerializeField]
		private tk2dTextMesh madnessPointsTextMesh;

		//

		[SerializeField]
		private KBFocusableSuccessors madnessModeRowsSuccessors;

		//

		[SerializeField]
		private GUISkin guiSkin;

		private KBFileChooser activeFileChooser = null;

		//

		private PrefabsRecyclerBase<KBFocusableGUIItem> _madnessModeRowsRecycler;
		private PrefabsRecyclerBase<KBFocusableGUIItem> madnessModeRowsRecycler
		{
			get
			{
				if(_madnessModeRowsRecycler == null)
					_madnessModeRowsRecycler = new PrefabsRecyclerBase<KBFocusableGUIItem>(madnessModeRowTemplate, madnessModeRowsSuccessors.transform);

				return _madnessModeRowsRecycler;
			}
		}

		private HashSet<KBMadnessModeRow> madnessModeRows = new HashSet<KBMadnessModeRow>();

		//

		public int usedMadnessPoints
		{
			get { return GetUsedMadnessPointsCount(Config.madnessMode.steps); }
		}

		//

		#region Unity

		private void OnEnable()
		{
			UpdateMadnessPoints();
		}

		protected override void Awake()
		{
			base.Awake();
		
			//

			SetFocusFirstItem(false);
		}

		private void OnGUI()
		{
			if(activeFileChooser != null)
				activeFileChooser.OnGUI();
		}

		#endregion

		public override void Show(object bundle)
		{
			base.Show(bundle);

			if(madnessModeTimeline != null)
			{
				madnessModeTimeline.Show(this);
			}

			UpdateMadnessEvents();
		}

		protected override void OnFocused(KBFocusableGUIItem guiItem)
		{
			base.OnFocused(guiItem);

			var tp = guiItem as KBMadnessModeTimelinePoint;

			Debug.Log("OnFocused " + guiItem);

			if(tp != null)
			{
				GenerateMadnessModeSteps(tp.time);
			}
			else
			{
				//zbytek GUI
			}
		}

		//

		private void GenerateMadnessModeSteps(int roomTime)
		{
			Debug.Log("GenerateMadnessModeSteps roomTime: " + roomTime);

			foreach(var row in madnessModeRows)
			{
				if(row != null)
					madnessModeRowsRecycler.Enqueue(row);
			}

			madnessModeRows.Clear();

			//

			var steps = GetMadnessStepsAtTime(roomTime);

			if(steps == null)
				return;

			#if UNITY_EDITOR
			Debug.Log("Generating madness steps for time " + roomTime + " / " + steps.GetHashCode());
			#endif

			if(madnessModeTimeline != null)
				madnessModeTimeline.SetSelectedTime(roomTime);

			//

			Vector2 offset = Vector2.zero;
			int i = 0;

			if(scrollArea != null)
				scrollArea.Value = 0;
			
			foreach(var kvp in steps)
			{
				var row = madnessModeRowsRecycler.Dequeue() as KBMadnessModeRow;

				row.ResetTransforms();

				var lp = row.localPosition;
				lp.x = offset.x;
				lp.y = offset.y;
				row.localPosition = lp;

				row.Setup(this, kvp.Value, roomTime);

				madnessModeRows.Add(row);

				madnessModeRowsSuccessors.RegisterFocusableItem(row);

				offset.x += 0.65f;

				i++;

				if(i % 3 == 0)
				{
					offset.x = 0f;
					offset.y -= 0.22f;
				}
			}

			if(scrollArea != null)
				scrollArea.ContentLength = Mathf.Abs(offset.y);

			if(madnessModeRowTemplate != null)
				madnessModeRowTemplate.SetActive(false);

			NotifyFocusableItemsDatasetChanged(false);

			UpdateAvailability();
		}

		private Dictionary<MadnessStepType, Config.MadnessMode.MadnessStep> GetMadnessStepsAtTime(int time)
		{
			Dictionary<MadnessStepType, Config.MadnessMode.MadnessStep> steps = null;

			Config.madnessMode.steps.TryGetValue(time, out steps);

			if(steps == null)
			{
				Debug.LogError("Failed to query Madness Steps for time " + time);
				return null;
			}

			return steps;
		}

		private int GetUsedMadnessPointsCount(Dictionary<int, Dictionary<MadnessStepType,Config.MadnessMode.MadnessStep>> steps)
		{
			int usedMadnessPoints = 0;

			foreach(var aKvp in steps)
			{
				int selectedTime = aKvp.Key;

				foreach(var bKvp in aKvp.Value)
				{
					var step = bKvp.Value;

					if(step.usedCount > 0)
						usedMadnessPoints += Config.madnessMode.GetMadnessStepPrice(step, selectedTime);
				}
			}

			return usedMadnessPoints;
		}

		#region Events

		public override void OnClick(KBFocusableGUIItem guiItem)
		{
			base.OnClick(guiItem);

			switch(guiItem.name)
			{
				case "Next":
					GoNext();
				break;

				case "Load":
					LoadMadnessStepsFromFile();
				break;

				case "Save":
					SaveMadnessStepsToFile();
				break;
			}
		}

		#endregion

		public override void GoNext()
		{
			base.GoNext();

			var gameRoom = menuRenderer.gameRoom;

			if(gameRoom == null)
			{
				SetError("gameRoom == null in KBMandnessMode");
			}
			else
			{
				menuRenderer.SetState(this, KBMenuRenderer.State.Equip);
			}
		}

		//

		public void OnCheckedStateChanged()
		{
			//#if UNITY_EDITOR
			//Debug.Log("usedMadnessPoints " + usedMadnessPoints);
			//#endif

			UpdateAvailability();
			UpdateMadnessPoints();
		}

		//

		private void UpdateAvailability()
		{
			foreach(var madnessModeRow in madnessModeRows)
			{
				if(madnessModeRow != null)
					madnessModeRow.UpdateAvailability();
			}
		}

		private void UpdateMadnessPoints()
		{
			if(madnessPointsTextMesh != null)
			{
				int usedMp = usedMadnessPoints;

				madnessPointsTextMesh.text = localization.GetValue("MadnessPoints_Indicator", usedMp, LocalClientRobotEmil.madnessPoints);

				int currMp = LocalClientRobotEmil.madnessPoints;

				if(currMp > 0)
				{
					float p = (((float)usedMp) / ((float)currMp));

					if(p >= 0.8f && p < 0.92f)
					{
						madnessPointsTextMesh.color = Color.yellow;
					}
					else if(p >= 0.92f)
					{
						madnessPointsTextMesh.color = Color.red;
					}
					else
					{
						madnessPointsTextMesh.color = Color.white;
					}
				}
			}

			//

			UpdateMadnessEventsCount(madnessModeTimeline.selectedTime);
		}

		//

		private void UpdateMadnessEvents()
		{
			for(int time = Config.madnessMode.timeBetweenMadnessSteps; time <= Config.madnessMode.maxMadnessModeTime; time += Config.madnessMode.timeBetweenMadnessSteps)
			{
				UpdateMadnessEventsCount(time);
			}

			GenerateMadnessModeSteps(Config.madnessMode.selectedMadnessModeTime);
		}

		private void UpdateMadnessEventsCount(int currTime)
		{
			var steps = GetMadnessStepsAtTime(currTime);

			if(steps != null)
			{
				int usedStepsCount = 0;

				foreach(var kvp in steps)
				{
					var step = kvp.Value;

					usedStepsCount += step.usedCount;
				}

				madnessModeTimeline.SetUsedMadnessStepsCount(currTime, usedStepsCount);
			}
		}

		#region Save/Load Madness Steps

		private KBFileChooser _fileChooser = null;
		private KBFileChooser fileChooser
		{
			get
			{ 
				if(_fileChooser == null)
				{
					_fileChooser = new KBFileChooser("Choose path...", OnPathSelected, guiSkin);
				}

				return _fileChooser;
			}
		}

		private void LoadMadnessStepsFromFile()
		{
			activeFileChooser = fileChooser;
			activeFileChooser.BrowserType = FileChooserType.File;

		}

		private void SaveMadnessStepsToFile()
		{
			activeFileChooser = fileChooser;
			activeFileChooser.BrowserType = FileChooserType.Directory;

		}

		private void OnPathSelected(string path)
		{
			if(activeFileChooser == null)
				return;

			#if UNITY_EDITOR
			Debug.Log("OnPathSelected " + path);
			#endif

			if(path != null)
			{
				Madness.MadnessModeStateSerializer serializer = new GMReloaded.Madness.MadnessModeStateSerializer();

				switch(activeFileChooser.BrowserType)
				{
					case FileChooserType.File:
						var madness = serializer.DeserializeFromFile(path);

						if(madness != null)
						{
							Dictionary<int, Dictionary<MadnessStepType, GMReloaded.Config.MadnessMode.MadnessStep>> steps = new Dictionary<int, Dictionary<MadnessStepType, Config.MadnessMode.MadnessStep>>();
							Config.MadnessMode.ParseMadnessModeMPStructToDictionary(ref steps, madness);

							int loadedFileUsedMadnessPoints = GetUsedMadnessPointsCount(steps);

							if(loadedFileUsedMadnessPoints > LocalClientRobotEmil.madnessPoints)
							{
								menuRenderer.SetErrorLocalized("MadnessMode_NotEnoughMadnessPoints", loadedFileUsedMadnessPoints);
							}
							else
							{
								Config.madnessMode.SetMadnessSteps(madness);

								UpdateMadnessEvents();
							}
						}
					break;
					
					case FileChooserType.Directory:
						serializer.SerializeToFile(path);
					break;
				}
			}

			activeFileChooser = null;
		}

		#endregion
	}
}
