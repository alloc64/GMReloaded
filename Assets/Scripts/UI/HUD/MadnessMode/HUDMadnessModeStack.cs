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

namespace GMReloaded.UI.HUD.MadnessMode
{
	public class HUDMadnessModeStack : MonoBehaviour
	{
		public HUDMadnessModeStackIcon baseIcon;

		private PrefabsRecyclerBase<HUDMadnessModeStackIcon> recycler;

		private Dictionary<MadnessStepType, HUDMadnessModeStackIcon> icons = new Dictionary<MadnessStepType, HUDMadnessModeStackIcon>();

		#region Unity

		private void Awake()
		{
			recycler = new PrefabsRecyclerBase<HUDMadnessModeStackIcon>(baseIcon, transform);
			recycler.Preinstantiate(15);

			baseIcon.SetActive(false);
		}

		#endregion

		//

		private HUDMadnessModeStackIcon GetStackIcon(Config.MadnessMode.MadnessStep step)
		{
			if(step == null)
				return null;

			var stepType = step.stepType;

			HUDMadnessModeStackIcon icon = null;
			icons.TryGetValue(stepType, out icon);

			if(icon == null)
			{
				icon = recycler.Dequeue();
			}

			if(icon != null)
			{
				icons[stepType] = icon;
				RecalcOffsets();
			}

			return icon;
		}

		//

		public void PrepareMadnessStepForDispatch(Config.MadnessMode.MadnessStep step, int prepareForDispatchTime)
		{
			var icon = GetStackIcon(step);

			if(icon == null)
				return;

			icon.PrepareForDispatch(step, prepareForDispatchTime);
		}


		public void DispatchMadnessStep(Config.MadnessMode.MadnessStep step)
		{
			var icon = GetStackIcon(step);

			if(icon == null)
				return;

			icon.Dispatch(step);
		}

		public void TimedMadnessStepDispatched(Config.MadnessMode.MadnessStep step)
		{
			var icon = GetStackIcon(step);

			if(icon == null)
				return;

			icon.TimedMadnessStepDispatched(step);
		}

		//

		private void RecalcOffsets()
		{
			float offset = 0;
			foreach(var kvp in icons)
			{
				var icon = kvp.Value;

				icon.SetLocalPostionY(offset);
				offset += 0.08f;
			}
		}
	}
}