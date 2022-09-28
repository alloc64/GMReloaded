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

namespace GMReloaded
{
	public class HitIndicator : MonoBehaviourTO 
	{
		[SerializeField]
		private tk2dSprite sprite;

		private bool shown = false;

		[SerializeField]
		private float time = 3f;

		private float timer = 0f;

		private Vector3 lastHitPosition;

		private RobotEmil robotParent;

		#region Unity

		private void Awake()
		{
			ResetSprite();
		}

		private void Update()
		{
			if(robotParent != null && shown)
			{
				//TODO: tahle dir se musí počítat jinak
				SetDirection(robotParent.direction + (lastHitPosition - (position)).normalized);

				SetSpriteAlpha(1f - Mathf.Clamp01((timer / time) * 4f));

				if(timer < time)
					timer += Time.deltaTime;

				if(timer >= time)
				{
					ResetSprite();
				}
			}
		}

		#endregion

		private void ResetSprite()
		{
			timer = 0f;
			shown = false;

			SetSpriteActive(false);
		}

		public void SetRobotParent(RobotEmil robotParent)
		{
			this.robotParent = robotParent;
		}

		public void SetHitPosition(Vector3 position)
		{
			this.lastHitPosition = position;

			shown = true;
			timer = 0f;

			SetSpriteActive(true);
		}

		private void SetDirection(Vector2 dir)
		{
			SetAngle(Mathf.Atan2(dir.y, dir.x) * (180.0f / Mathf.PI));
		}

		private void SetAngle(float angle)
		{
			if(sprite != null)
			{
				var la = sprite.localEulerAngles;
				la.z = angle;
				sprite.localEulerAngles = la;
			}
		}

		private void SetSpriteAlpha(float f)
		{
			if(sprite != null)
				sprite.SetAlpha(f);
			
		}

		private void SetSpriteActive(bool active)
		{
			if(sprite != null)
				sprite.SetActive(active);
		}

	}
}
