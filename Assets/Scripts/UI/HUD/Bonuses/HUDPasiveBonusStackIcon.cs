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

namespace GMReloaded
{
	public class HUDPasiveBonusStackIcon : IRecyclablePrefab<HUDPasiveBonusStackIcon>
	{
		[SerializeField]
		private tk2dSprite icon;

		[SerializeField]
		private tk2dTextMesh nameTextMesh;

		[SerializeField]
		private tk2dTextMesh countTextMesh;

		[SerializeField]
		private Animation slotOccupiedAnimation;

		//

		public override void Reinstantiate()
		{
			SetActive(true);
			ResetTransforms();
		}

		public void SetIcon(string id)
		{
			//Debug.Log("HUDPasiveBonusStack " + id);

			if(nameTextMesh != null)
				nameTextMesh.text = localization.GetValue(id);

			if(icon != null)
				icon.SetSpriteByID(id, "Bonus_Speed");
		}

		public void SetValue(int value)
		{
			if(countTextMesh != null)
				countTextMesh.text = "+" + value;
		}

		public void Shake()
		{
			if(slotOccupiedAnimation != null)
				slotOccupiedAnimation.Play(() => {});
		}

		public void SetLocalPostionY(float offset)
		{
			var lp = localPosition;
			lp.y = offset;
			localPosition = lp;
		}
	}
	
}