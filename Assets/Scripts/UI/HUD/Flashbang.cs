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

using Independent;

namespace GMReloaded
{
	public class Flashbang : MonoBehaviourTO
	{
		[SerializeField]
		private tk2dSprite sprite;

		[SerializeField]
		private new Animation animation;

		public void GrenadeFlash(float p, RobotEmilNetworked robotParent)
		{
			if(animation == null)
				return;

			animation.Stop();

			if(robotParent != null)
				robotParent.SetIsFlashed(true);

			string id = "Flashbang";

			AnimationState state = animation[id];

			animation[id].time = state.length * p;
			animation[id].speed = 1.0f;
			animation.Play(id, () => 
			{
				if(robotParent != null)
					robotParent.SetIsFlashed(false);
			});
		}

		public void TeleportFlash()
		{
			if(animation == null || animation.isPlaying)
				return;
			
			animation.Play("TeleportFlashbang");
		}
	}
}
