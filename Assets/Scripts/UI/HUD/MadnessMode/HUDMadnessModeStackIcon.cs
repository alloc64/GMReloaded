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

namespace GMReloaded.UI.HUD.MadnessMode
{
	public class HUDMadnessModeStackIcon : IRecyclablePrefab<HUDMadnessModeStackIcon> 
	{
		[SerializeField]
		private tk2dSprite icon;

		[SerializeField]
		private tk2dTextMesh nameTextMesh;

		[SerializeField]
		private tk2dTextMesh countTextMesh;

		//

		private float countDownTimer = 0.0f;
		private bool countdown = false;

		//

		private void Update()
		{
			UpdateCountdown();
		}

		//

		public override void Reinstantiate()
		{
			SetActive(true);
			ResetTransforms();
		}

		//

		private void SetName(Config.MadnessMode.MadnessStep step, bool active)
		{
			if(step != null && nameTextMesh != null)
			{
				nameTextMesh.text = step.name + " x" + step.usedCount;
				nameTextMesh.SetAlpha(active ? 1f : 0.5f);

				if(countTextMesh != null)
					countTextMesh.text = active ? "Active" : "";
			}
		}

		//

		public void PrepareForDispatch(Config.MadnessMode.MadnessStep step, int prepareForDispatchTime)
		{
			SetName(step, false);

			/*
			if(icon != null)
				icon.SetSpriteByID(id, "Bonus_Speed");
			*/

			countDownTimer = prepareForDispatchTime;
			countdown = prepareForDispatchTime > 0;
		}

		public void Dispatch(Config.MadnessMode.MadnessStep step)
		{
			SetName(step, true);

			countDownTimer = 0f;
			countdown = false;
		}

		public void TimedMadnessStepDispatched(Config.MadnessMode.MadnessStep step)
		{
			if(countTextMesh != null)
				countTextMesh.text = "Timed out";
		}

		//

		private void UpdateCountdown()
		{
			if(!countdown)
				return;

			if(countDownTimer > 0f)
				countDownTimer -= Time.deltaTime;

			if(countDownTimer <= 0.0f)
				countDownTimer = 0.0f;

			if(countTextMesh != null)
			{
				int seconds = ((int)countDownTimer) % 60;
				int minutes = ((int)countDownTimer) / 60;

				countTextMesh.text = string.Format("{0}:{1:00}", minutes, seconds);
			}
		}

		//

		public void SetLocalPostionY(float offset)
		{
			var lp = localPosition;
			lp.y = offset;
			localPosition = lp;
		}
	}
}