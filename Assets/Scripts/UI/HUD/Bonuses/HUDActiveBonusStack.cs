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
using GMReloaded.Bonuses;

namespace GMReloaded
{
	public class HUDActiveBonusStack : MonoBehaviourTO
	{
		[System.Serializable]
		public class BonusStackItem
		{
			public Bonus bonus { get; set; }
			public ActiveBonusSlot slot;

			public bool PickUpBonus(Bonus bonus)
			{
				if(this.bonus == null)
				{
					this.bonus = bonus;
					this.bonus.activeBonusStackItem = this;

					slot.SetIcon("Bonus_" + bonus.behaviour, "hp_result");

					return true;
				}

				return false;
			}

			public void PickUpRefused(Bonus bonus)
			{
				slot.Shake();
			}

			public void Pulse()
			{
				if(this.bonus != null)
				{
					slot.Pulse();
				}
			}

			public void SetBonusRemainingTime(int time)
			{
				slot.SetBonusRemainingTime(time);
			}

			public void StopPulsing()
			{
				if(this.bonus != null)
				{
					SetBonusRemainingTime(0);
					slot.StopPulse();

					this.bonus = null;
				}
			}
		}

		[SerializeField]
		private BonusStackItem [] stack = new BonusStackItem[Config.Bonuses.maxCount];

		public bool PickUpBonus(Bonus bonus)
		{
			if(bonus == null)
				return false;

			for(int i = 0; i < stack.Length; i++)
			{
				if(stack[i].bonus == null)
				{
					stack[i].PickUpBonus(bonus);
					return true;
				}
			}

			return false;
		}

		public void OnBonusPickUpRefused(Bonus bonus)
		{
			if(bonus == null)
				return;

			for(int i = 0; i < stack.Length; i++)
			{
				stack[i].PickUpRefused(bonus);
			}
		}

	}
	
}
