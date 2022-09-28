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
using GMReloaded.Entities;

namespace GMReloaded.Madness
{
	public class Diarrhea_MadnessModeImpl : IMadnessModeStepDispatch
	{
		//


		public float diarrheaShootTime { get { return _diarrheaShootTime; } }

		//

		private float _diarrheaShootTime = -1f;

		//

		public override void Dispatch(MadnessModeController mmc, Config.MadnessMode.MadnessStep step, float dispatchTime, double timestamp)
		{
			UpdateDiarrheaShootTime();
			base.Dispatch(mmc, step, dispatchTime, timestamp);
		}

		public override void RestoreState()
		{
			base.RestoreState();
			UpdateDiarrheaShootTime();
		}

		private void UpdateDiarrheaShootTime()
		{
			_diarrheaShootTime = Mathf.Clamp(Config.MadnessMode.DiarrheaShootTime_Max - (useCount * Config.MadnessMode.DiarrheaShootTime_Progress), 
			                                 Config.MadnessMode.DiarrheaShootTime_Min, 
			                                 Config.MadnessMode.DiarrheaShootTime_Max); 
			
			//Debug.Log("_diarrheaShootTime " + _diarrheaShootTime);
		}
	}
}
