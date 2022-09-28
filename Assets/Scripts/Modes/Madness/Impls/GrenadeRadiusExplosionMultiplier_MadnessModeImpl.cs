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
using GMReloaded.Bonuses;

namespace GMReloaded.Madness
{
	public class GrenadeRadiusExplosionMultiplier_MadnessModeImpl : IMadnessModeStepDispatch
	{
		private float radiusMultiplier = 1f;

		//

		public override void Dispatch(MadnessModeController mmc, Config.MadnessMode.MadnessStep step, float dispatchTime, double timestamp)
		{
			base.Dispatch(mmc, step, dispatchTime, timestamp);

			radiusMultiplier = Mathf.Clamp(radiusMultiplier + Config.MadnessMode.GrenadeDamageRadiusMultiplier_DamageMultiplier_Progress, 
			                               Config.MadnessMode.GrenadeDamageRadiusMultiplier_DamageMultiplier_Min, 
			                               Config.MadnessMode.GrenadeDamageRadiusMultiplier_DamageMultiplier_Max);


			robotParent.SetGrenadeDamageRadiusMultiplier_MadnessMode(radiusMultiplier);
		}

		public override void RestoreState()
		{
			base.RestoreState();

			radiusMultiplier = 1f;
		}
	}
	
}
