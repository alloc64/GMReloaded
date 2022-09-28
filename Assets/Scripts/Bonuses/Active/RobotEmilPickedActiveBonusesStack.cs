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

namespace GMReloaded.Bonuses.Active
{
	[System.Serializable]
	public class RobotEmilPickedActiveBonusesStack
	{
		[SerializeField]
		private RobotEmil parentRobot;

		//

		public StackItem [] stack { get; private set; }

		private ISound bonusGranted;

		//

		public bool isFull { get { return usedSlotsCount >= stack.Length; } }

		public int usedSlotsCount
		{
			get
			{ 
				int cnt = 0;

				for(int i = 0; i < stack.Length; i++)
				{
					if(stack[i].picked)
						cnt++;
				}

				return cnt;
			}
		}

		//

		private HUD hud { get { return HUD.Instance; }}

		protected GMReloaded.SoundManager snd { get { return GMReloaded.SoundManager.GetInstance(); } }

		//

		public RobotEmilPickedActiveBonusesStack(RobotEmil parentRobot) 
		{ 		
			this.parentRobot = parentRobot;

			stack = new StackItem[Config.Bonuses.maxCount];

			for(int i = 0; i < stack.Length; i++)
				stack[i] = new StackItem();

			bonusGranted = snd.Load(Config.Sounds.bonusGranted);
		}

		//

		public bool PickBonus(Bonus bonus)
		{
			if(bonus == null)
				return false;
				
			Debug.Log("PickBonus " + bonus.behaviour);

			for(int i = 0; i < stack.Length; i++)
			{
				if(!stack[i].picked)
				{
					bonus.activeBonusStackItemId = i;
					stack[i].PickUp(bonus);

					if(parentRobot.clientType == RobotEmil.ClientType.LocalClient)
					{
						hud.OnLocalPlayerActiveBonusPickedUp(bonus);
					}

					Debug.Log("PickBonus/set" + i);

					return true;
				}
			}

			return false;
		}

		public void OnBonusPickupRefused(Bonus bonus)
		{
			if(parentRobot.clientType == RobotEmil.ClientType.LocalClient)
			{
				hud.OnLocalPlayerActiveBonusPickUpRefused(bonus);
			}
		}

		//

		public bool UseBonusStackItem(int index, bool usedRemote, double timestamp)
		{
			if(index < 0 || index >= Config.Bonuses.maxCount)
				return false;

			var stackItem = stack[index];

			if(!stackItem.picked)
				return false;

			var bonus = stackItem.bonus;

			if(bonus == null)
				return false;

			if(timestamp > 0f)
				bonus.usedTimestamp = timestamp;

			var pickerRobot = bonus.pickerRobot;

			if(pickerRobot == null)
				return false;
			
			bool used = pickerRobot.UseBonus(bonus, usedRemote);

			if(pickerRobot.clientType == RobotEmil.ClientType.LocalClient)
			{
				if(used)
				{
					if(bonusGranted != null)
						bonusGranted.Play();
				}
				/*else
				{
					if(bonusDenied != null)
						bonusDenied.Play();
				}*/
			}

			return used;
		}

		public bool BonusUsageComplete(int index)
		{
			if(index < 0 || index >= Config.Bonuses.maxCount)
				return false;

			var stackItem = stack[index];

			Debug.Log("BonusUsageComplete " + index + " - " + stackItem + " - " + stackItem.picked);

			if(!stackItem.picked)
				return false;

			stackItem.BonusUsed();

			return true;
		}
	}
	
}
