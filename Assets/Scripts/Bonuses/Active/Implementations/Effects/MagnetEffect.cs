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
using Independent;
using TouchOrchestra;

namespace GMReloaded.Bonuses.Effects
{
	public class MagnetEffect : IBonusEffect
	{
		[SerializeField]
		private new Animation animation;

		private void Awake()
		{
			Assert.IsAssigned(animation);
		}

		public override void Show()
		{
			if(state != State.Hidden)
				return;

			SetActive(true);
			animation.Play("MagnetShow");

			state = State.Shown;
		}

		public override void Hide()
		{
			if(state != State.Shown)
				return;

			animation.Play("MagnetHide", () => 
			{
				SetActive(false);
				state = State.Hidden;
			});
		}
	}
}
