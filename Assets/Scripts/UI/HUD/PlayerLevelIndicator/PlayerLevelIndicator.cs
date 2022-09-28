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
	public class PlayerLevelIndicator : MonoBehaviourTO
	{
		[SerializeField]
		private tk2dTextMesh levelTextMesh;

		[SerializeField]
		private tk2dTextMesh expsTextMesh;

		[SerializeField]
		private UIProgressBarClipped levelProgressBar;

		public void SetXP(int level, int currPts, int nextPts, float progress)
		{
			if(levelTextMesh != null)
			{
				string key = "Level_" + level;

				levelTextMesh.text = (localization.HasValue(key) ? localization.GetValue(key) : "Level " + level);
			}

			if (expsTextMesh != null) 
			{
				expsTextMesh.text = currPts + " / " + nextPts;
			}

			if(levelProgressBar != null)
			{
				levelProgressBar.SetProgress(progress);
			}
		}
	}
}
