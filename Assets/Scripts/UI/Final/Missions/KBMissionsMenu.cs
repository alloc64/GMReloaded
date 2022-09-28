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
using System.Collections.Generic;
#if UNITY_XBOXONE
using GMReloaded.Xbox.Linq;
#else
using System.Linq;
#endif

namespace GMReloaded.UI.Final.Missions
{
	public class KBMissionsMenu : KBFocusableSuccessorsGUI 
	{
		[SerializeField]
		private tk2dTextMesh missionProgressText;

		//

		[SerializeField]
		private tk2dTextMesh itemNameText;

		[SerializeField]
		private tk2dTextMesh itemDescriptionText;

		[SerializeField]
		private tk2dTextMesh itemProgress;

		[SerializeField]
		private UIProgressBarClipped itemProgressBar;

		//

		[SerializeField]
		private tk2dTextMesh rewardsDescriptionText;

		//

		[SerializeField]
		private KBMissionItem missionItemTemplate;

		[SerializeField]
		private Transform missionItemsContainer;

		[SerializeField]
		private tk2dUIScrollableArea scrollableArea;

		[SerializeField]
		private KBFocusableSuccessors missionItemFocusableSuccessors;


		//

		private Dictionary<string, KBMissionItem> missionItems = new Dictionary<string, KBMissionItem>();

		private Achievements.MissionsController missionController { get { return Achievements.MissionsController.Instance; } }

		//

		#region Unity

		protected override void Awake()
		{
			base.Awake();

			GenerateMissionItems();
		}

		#endregion

		private void GenerateMissionItems()
		{
			if(missionItemTemplate == null)
			{
				Debug.LogError("missionItemTemplate == null");
				return;
			}

			var nextMission = missionController.nextMission;

			int numCompleted = 0;

			float offset = 0f;
			float selectedMissionOffset = 0f;

			int i = 0;
			#if UNITY_XBOXONE
			foreach(var kvp in missionController.missions.Missions_OrderBy_AOT((m0, m1) => m0.Value.id.CompareTo(m1.Value.id)))
			#else
			foreach(var kvp in missionController.missions.OrderBy(m => m.Value.id))
			#endif
			{
				var mission = kvp.Value;

				if(mission != null)
				{
					if(mission.activated)
						numCompleted++;

					var row = Utils.CloneItem<KBMissionItem>(missionItemTemplate, missionItemsContainer);

					if(row != null)
					{
						bool isNextMission = mission.key == nextMission.key;

						if(isNextMission)
							selectedMissionOffset = offset;

						row.SetMission(mission, isNextMission, i, offset);
						row.SetActivated(mission.activated);

						if(i > 0 && i % 3 == 2)
							offset -= 0.5f;

						if(missionItemFocusableSuccessors != null)
							missionItemFocusableSuccessors.RegisterFocusableItem(row);

						missionItems[kvp.Key] = row;
					}

					i++;
				}
			}

			missionItemTemplate.SetActive(false);

			scrollableArea.ContentLength = Mathf.Abs(offset+0.25f);

			//scrollableArea.Value = selectedMissionOffset / offset;

			SetCompletedCount(numCompleted);

			NotifyFocusableItemsDatasetChanged(false);
		}

		private void SetCompletedCount(int numCompleted)
		{
			if(missionProgressText != null)
				missionProgressText.text = localization.GetValue("MissionMenu_Progress", numCompleted, missionController.missions.Count);
		}

		public override void Show(object bundle)
		{
			base.Show(bundle);

			int numCompleted = 0;
			foreach(var kvp in missionController.missions)
			{
				var mission = kvp.Value as Achievements.Mission;

				if(mission != null)
				{
					if(mission.activated)
						numCompleted++;

					KBMissionItem row = null;

					missionItems.TryGetValue(kvp.Key, out row);

					if(row != null)
					{
						row.SetActivated(mission.activated);
					}
				}
			}

			//

			SetCompletedCount(numCompleted);
		}

		protected override void OnFocused(KBFocusableGUIItem guiItem)
		{
			base.OnFocused(guiItem);

			KBMissionItem missionItem = guiItem as KBMissionItem;

			if(missionItem != null)
			{
				if(itemNameText != null)
					itemNameText.text = localization.GetValue("Mission") + " " + missionItem.missionName;
				
				if(itemDescriptionText != null)
					itemDescriptionText.text = missionItem.missionDescription;
				
				if(itemProgress != null)
					itemProgress.text = missionItem.missionProgress;

				if(itemProgressBar != null)
					itemProgressBar.SetProgress(missionItem.missionProgressPercents);

				if(rewardsDescriptionText != null)
				{
					string rewardText = "";
				
					if(missionItem.isAchievement)
						rewardText += "\n +1 achievement";
					
					var numberFormat = new System.Globalization.NumberFormatInfo { NumberGroupSeparator = " ", NumberDecimalDigits = 0 };

					if(missionItem.experience > 0)
						rewardText += "\n +" + missionItem.experience.ToString("n", numberFormat) + " " + localization.GetValue("EXP");

					if(missionItem.madnessPoints > 0)
						rewardText += "\n +" + missionItem.madnessPoints + " " + localization.GetValue("MP");

					if(string.IsNullOrEmpty(rewardText))
						rewardText = "\nNone";

					rewardsDescriptionText.text = rewardText;
				}
			}
			else
			{
				// ostatni
			}
		}
	}
}