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

namespace GMReloaded
{
	public class MissionIndicator : MonoBehaviourTO 
	{
		[SerializeField]
		private UIProgressBarClipped progressBar;

		[SerializeField]
		private tk2dTextMesh descTextMesh;

		[SerializeField]
		private tk2dTextMesh progressTextMesh;

		private Achievements.Mission currMission;

		private float missionProgress = -1f;

		//

		private void Awake()
		{
			SetCurrRoundMission(Achievements.MissionsController.Instance.nextMission);
		}

		private void Update()
		{
			if(currMission != null)
			{
				var p = currMission.progressPercent;

				if(!Mathf.Approximately(missionProgress, p))
				{
					missionProgress = p;
					progressBar.SetProgress(missionProgress);
				}

				if(progressTextMesh != null)
					progressTextMesh.text = currMission.progress + " / " + currMission.threshold;
			}
		}

		public void SetCurrRoundMission(Achievements.Mission mission)
		{
			this.currMission = mission;

			SetActive(mission != null);

			if(mission == null)
				return;

			descTextMesh.text = localization.GetValue(mission.key);
		}
	}
}