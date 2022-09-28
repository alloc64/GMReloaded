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
	public class KBMadnessModeRow : KBFocusableChooser
	{
		[SerializeField]
		private tk2dBaseSprite iconsSprite;

		[SerializeField]
		private KBMadnessModeRowHint hint;

		//

		private Config.MadnessMode.MadnessStep step;

		private KBMadnessMode madnessModeGUI;

		private int roomTime;

		//

		public override void Reinstantiate()
		{
			SetActive(true);
		}

		//

		public void Setup(KBMadnessMode madnessModeGUI, Config.MadnessMode.MadnessStep step, int roomTime)
		{
			this.madnessModeGUI = madnessModeGUI;
			this.step = step;
			this.roomTime = roomTime;

			bool locked = LocalClientRobotEmil.level < step.unlockLevel;

			SetTitle(string.Format(step.name + "\n^C2ac9daFF({0} MP)", Config.madnessMode.GetMadnessStepPrice(step, roomTime, true)));

			if(iconsSprite != null)
				iconsSprite.SetSpriteByID(step.stepType.ToString(), "Icon_placeholder");

			if(hint != null)
				hint.SetHint(step, locked);
			
			ClearItems();

			var textScale = Vector3.one * 0.035f;

			if(step.max == 1)
			{
				AddItemLocalized("Disabled");
				AddItemLocalized("Enabled");
			}
			else
			{
				for(int i = step.min; i <= step.max; i++)
				{
					AddItem((i > 0 ? "+" : "") + i);
				}

				textScale *= 1.4f;
			}

			textTextMesh.scale = textScale;

			//Debug.Log("Setup " + step.GetHashCode() + " - " + step.usedCount);

			SetListIndex(step.usedCount);
		}

		public override void OnSwitchItem(int d)
		{
			if(madnessModeGUI == null)
				return;

			int i = listIndex + d;

			//Debug.Log("i = " + listIndex + " + " + d);

			if(i >= 0 && i <= listItems.Count - 1)
			{
				//Debug.Log(LocalClientRobotEmil.madnessPoints + " - " + madnessModeGUI.usedMadnessPoints + " >= " + Config.madnessMode.GetMadnessStepPrice(step, roomTime, true));

				if(d < 0 || LocalClientRobotEmil.madnessPoints - madnessModeGUI.usedMadnessPoints >= Config.madnessMode.GetMadnessStepPrice(step, roomTime, true))
				{
					base.OnSwitchItem(d);
				}
			}
		}

		public void UpdateAvailability()
		{
			SetDisabled(step.usedCount < 1 && (LocalClientRobotEmil.level < step.unlockLevel || LocalClientRobotEmil.madnessPoints - madnessModeGUI.usedMadnessPoints < Config.madnessMode.GetMadnessStepPrice(step, roomTime, true)));
		}

		public override void SetDisabled(bool disabled)
		{
			base.SetDisabled(disabled);

			if(iconsSprite != null)
				iconsSprite.SetAlpha(disabled ? 0.5f : 1f);
		}

		protected override void OnHover(bool over)
		{
			base.OnHover(over);

			if(hint != null)
				hint.SetActive(over);
		}

		protected override void _OnItemChanged(int index)
		{
			base._OnItemChanged(index);

			step.usedCount = Convert.ToByte(index);

			if(madnessModeGUI != null)
				madnessModeGUI.OnCheckedStateChanged();
		}
	}
	
}
