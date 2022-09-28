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

namespace GMReloaded.UI.Final.CreateGame.MadnessMode
{
	public class KBMadnessModeTimelinePoint : KBFocusableGUIItemWithStateChange
	{
		[SerializeField]
		private tk2dTextMesh timeTextMesh;

		[SerializeField]
		private tk2dTextMesh madnessStepsCountTextMesh;

		//

		public int time { get; private set; }

		//

		public override void Reinstantiate()
		{
			SetActive(true);

			time = 0;
		}

		//

		public void SetTime(int time)
		{
			this.time = time;

			if(timeTextMesh != null)
				timeTextMesh.text = string.Format("{0}:{1:00}", (time / 60), (time % 60));
		}

		public void SetUsedMadnessStepsCount(int usedCount)
		{
			if(madnessStepsCountTextMesh != null)
			{
				madnessStepsCountTextMesh.text = (usedCount > 0) ? "+" + usedCount : "0";
			}
		}

		public void SetLocalPositionX(float posX)
		{
			var lp = localPosition;
			lp.x = posX;
			lp.z = -0.3f;
			localPosition = lp;
		}

		//

		public bool UpdateState(int selectedTime)
		{
			var selected = time == selectedTime;

			SetFocused(selected);

			return selected;
		}
	}
}