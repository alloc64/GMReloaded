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

namespace GMReloaded.UI.Final.Missions
{
	public class KBMissionItem : KBFocusableGUIItemWithStateChange
	{
		[SerializeField]
		private tk2dTextMesh titleText;

		[SerializeField]
		private tk2dBaseSprite iconSprite; 

		[SerializeField]
		private UIProgressBarClipped progressBar; 

		//

		public string missionName
		{
			get { return mission == null ? null : localization.GetValue(mission.key); }
		}

		public string missionDescription
		{
			get { return mission == null ? null : localization.GetValue(mission.key + "_Desc"); }
		}

		public string missionProgress
		{
			get { return mission == null ? "N/A" : localization.GetValue("Progress_Param1", mission.progress + " / " + mission.threshold); }
		}

		public float missionProgressPercents
		{
			get { return mission == null ? 0f : mission.progressPercent; }
		}

		//

		public int experience { get { return mission == null ? 0 : mission.experience; } }

		public int madnessPoints { get { return mission == null ? 0 : mission.madnessPoints; } }

		public bool isAchievement { get { return mission != null && mission.isAchievement; } }

		//

		private GMReloaded.Achievements.Mission mission;

		public void SetMission(GMReloaded.Achievements.Mission mission, bool isNextMission, int idx, float offset)
		{
			this.mission = mission;

			if(titleText != null)
				titleText.text = missionName;

			if(progressBar != null)
				progressBar.SetProgress(mission.progressPercent);

			var lp = localPosition;

			lp.x = 0.367f * (idx % 3);

			lp.y = offset;
			localPosition = lp;
		}

		public void SetActivated(bool activated)
		{
			if(iconSprite != null)
				iconSprite.SetSpriteByID(activated ? "mission_complete": "mission_incomplete");
			
		}
	}
}