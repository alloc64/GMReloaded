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
using System.Linq;
using System.Collections.Generic;
using System;

namespace GMReloaded.UI.Final.CreateGame.MadnessMode
{
	public class KBMadnessModeRowHint : MonoBehaviourTO
	{
		[SerializeField]
		private tk2dTextMesh hintTextMesh;

		public void SetHint(Config.MadnessMode.MadnessStep step, bool locked)
		{
			//Debug.Log("SetHint " + step.description);

			if(hintTextMesh != null)
			{
				string desc = step.description;

				if(locked)
					desc += "\n\nUnlocks in level " + step.unlockLevel;

				hintTextMesh.text = desc;
			}
		}
	}
	
}
