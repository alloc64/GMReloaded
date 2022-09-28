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
using System.Collections.Generic;

namespace GMReloaded.Bonuses.Pasive
{
	public class QuickExplodesBonusImpl : IBonusPasiveDispatch
	{
		private float explosionSpeedUp = 0f;

		private float speedUpMin = Config.Bonuses.QuickExplodes_SpeedUp_Min;
		private float speedUpMax = Config.Bonuses.QuickExplodes_SpeedUp_Max;

		//

		public override bool Dispatch(Bonus bonus, RobotEmilNetworked robotParent, bool permanent)
		{
			if(explosionSpeedUp >= speedUpMax)
			{
				// presahl jsem maxima, nejde sebrat
				return false;
			}

			bool ret = base.Dispatch(bonus, robotParent, permanent);

			SetExplosionSpeedUp(1f);

			return ret;
		}

		private void SetExplosionSpeedUp(float diff)
		{
			if(robotParent == null)
				return;

			explosionSpeedUp = Mathf.Clamp(explosionSpeedUp + Config.Bonuses.QuickExplodes_SpeedUp_Progress * diff, speedUpMin, speedUpMax);

			robotParent.SetGrenadeExposionSpeedUp(explosionSpeedUp);
		}

		public override void StopDispatch()
		{
			if(useCount > 0)
				SetExplosionSpeedUp(-1f);
			
			base.StopDispatch();
		}

		public override void Reset()
		{
			base.Reset();

			explosionSpeedUp = 0f;
		}
	}
}
