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

namespace GMReloaded.UI.Final.CreateGame.MadnessMode
{
	public class KBMadnessModeTimeline : KBFocusableSuccessors
	{
		[SerializeField]
		private KBMadnessModeTimelinePoint timelinePointTemplate;

		//

		private PrefabsRecyclerBase<KBFocusableGUIItem> _timelinePointsRecycler;
		private PrefabsRecyclerBase<KBFocusableGUIItem> timelinePointsRecycler
		{
			get
			{ 
				if(_timelinePointsRecycler == null)
					_timelinePointsRecycler = new PrefabsRecyclerBase<KBFocusableGUIItem>(timelinePointTemplate, transform);

				return _timelinePointsRecycler;
			}
		}

		private Dictionary<int, KBMadnessModeTimelinePoint> timelinePoints = new Dictionary<int, KBMadnessModeTimelinePoint>();

		//

		public int selectedTime { get; private set; }

		//

		public void Show(KBMadnessMode madnessModeWindow)
		{
			if(timelinePointTemplate != null)
				timelinePointTemplate.SetActive(false);

			foreach(var kvp in timelinePoints)
			{
				timelinePointsRecycler.Enqueue(kvp.Value);
			}

			timelinePoints.Clear();

			//

			var menuRenderer = KBMenuRenderer.Instance;

			//

			if(menuRenderer == null)
			{
				Debug.LogError("menuRenderer == null failed to initialize KBMadnessModeTimeline");
				return;
			}

			if(menuRenderer.gameRoom == null)
			{
				Debug.LogError("menuRenderer.gameRoom == null failed to initialize KBMadnessModeTimeline");
				return;
			}

			//

			int roundTime = menuRenderer.gameRoom.roundTime;

			if(roundTime > Config.madnessMode.maxMadnessModeTime)
				roundTime = Config.madnessMode.maxMadnessModeTime;

			int pointsCount = (roundTime / Config.madnessMode.timeBetweenMadnessSteps)-1;

			//

			int k = 0;
			for(int time = Config.madnessMode.timeBetweenMadnessSteps; time <= roundTime; time += Config.madnessMode.timeBetweenMadnessSteps)
			{
				KBMadnessModeTimelinePoint timelinePoint = timelinePointsRecycler.Dequeue() as KBMadnessModeTimelinePoint;

				timelinePoint.ResetTransforms();
				timelinePoint.SetTime(time);
				timelinePoint.SetLocalPositionX(-k * (1.85f / ((float)pointsCount)));

				if(timelinePoint != null)
				{
					timelinePoints[time] = timelinePoint;

					//

					RegisterFocusableItem(timelinePoint);
				}

				k++;
			}
		}

		//

		public void SetSelectedTime(int time)
		{
			this.selectedTime = time;

			foreach(var kvp in timelinePoints)
			{
				var timelinePoint = kvp.Value;

				if(timelinePoint != null)
				{
					timelinePoint.UpdateState(time);
				}
			}
		}

		public void SetUsedMadnessStepsCount(int time, int usedCount)
		{
			KBMadnessModeTimelinePoint timelinePoint = null;

			timelinePoints.TryGetValue(time, out timelinePoint);

			if(timelinePoint != null)
			{
				timelinePoint.SetUsedMadnessStepsCount(usedCount);
			}
		}
	}
}
