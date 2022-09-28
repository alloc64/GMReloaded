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
	public class ActiveBonusSlot : MonoBehaviourTO
	{
		public enum State
		{
			Idle,
			Pulsing
		}

		[SerializeField]
		private State state;

		[SerializeField]
		private tk2dSprite sprite;

		[SerializeField]
		private tk2dSprite activeFrame;

		[SerializeField]
		private tk2dTextMesh progressTextMesh;

		[SerializeField]
		private Animation slotOccupiedAnimation;

		private Timer pulsingTimer = new Timer();


		#region Unity

		private void Awake()
		{
			Assert.IsAssigned(sprite);
		}

		private void Update()
		{
			switch(state)
			{
				case State.Idle:
				break;

				case State.Pulsing:

					float s = pulsingTimer.Loop(0.8f, 1f, 0.5f);
					sprite.localScale = new Vector3(s, s, 1f);

					activeFrame.SetAlpha((s - 0.8f) / 0.2f);
				break;
			}
		}

		#endregion

		//

		public void SetIcon(string icon, string fallback)
		{
			sprite.SetActive(true);
			sprite.SetSpriteByID(icon, fallback);
		}

		public void Pulse()
		{
			SetState(State.Pulsing);
		}

		public void Shake()
		{
			if(slotOccupiedAnimation != null)
				slotOccupiedAnimation.Play(() => {});
		}

		//

		public void SetBonusRemainingTime(int time)
		{
			if(progressTextMesh != null)
				progressTextMesh.text = time + "s";
		}

		public void StopPulse()
		{
			SetState(State.Idle);
			sprite.SetActive(false);
		}

		private void SetState(State state)
		{
			this.state = state;

			switch(state)
			{
				case State.Idle:
					sprite.localScale = Vector3.one;
				break;
					
				case State.Pulsing:
				break;
			}

			if(activeFrame != null)
				activeFrame.SetActive(state == State.Pulsing);
		}
	}
}
