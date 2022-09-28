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
	public class HUDPasiveBonusStack : MonoBehaviourTO
	{
		public HUDPasiveBonusStackIcon baseIcon;

		private PrefabsRecyclerBase<HUDPasiveBonusStackIcon> recycler;

		private Dictionary<Bonus.Behaviour, HUDPasiveBonusStackIcon> icons = new Dictionary<Bonus.Behaviour, HUDPasiveBonusStackIcon>();

		#region Unity

		private void Awake()
		{
			recycler = new PrefabsRecyclerBase<HUDPasiveBonusStackIcon>(baseIcon, transform);
			recycler.Preinstantiate(5);

			baseIcon.SetActive(false);
		}

		#endregion

		public void SetBonusUse(Bonus bonus, int useCount)
		{
			if(bonus == null)
				return;

			var bonusBehaviour = bonus.behaviour;

			HUDPasiveBonusStackIcon icon = null;
			icons.TryGetValue(bonusBehaviour, out icon);

			if(useCount > 0)
			{
				if(icon == null)
				{
					icon = recycler.Dequeue();
				}

				if(icon != null)
				{
					icon.SetIcon("Bonus_" + bonusBehaviour);

					icons[bonusBehaviour] = icon;
				}

				if(icon != null)
					icon.SetValue(useCount);
			}
			else
			{
				if(icon != null)
				{
					icons.Remove(bonusBehaviour);
					recycler.Enqueue(icon);
				}
			}

			RecalcOffsets();
		}

		public void OnBonusPickUpRefused(Bonus bonus)
		{
			if(bonus == null)
				return;

			HUDPasiveBonusStackIcon icon = null;
			icons.TryGetValue(bonus.behaviour, out icon);

			if(icon != null)
				icon.Shake();
		}

		//

		private void RecalcOffsets()
		{
			float offset = 0;
			foreach(var kvp in icons)
			{
				var icon = kvp.Value;

				icon.SetLocalPostionY(offset);
				offset += 0.07f;
			}
		}
	}
}