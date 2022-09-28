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
using TouchOrchestra;

namespace GMReloaded
{
	public class ScreenCorners : MonoBehaviourTO 
	{
		public enum State
		{
			Idle,
			Slap
		}

		[SerializeField]
		private tk2dSprite[] corners;

		private float slapWaitTime;
		private float slapTime;
		private State state;

		private RadialBlur radialBlur { get { return RobotEmilImageEffects.Instance.radialBlur; } }

		[SerializeField]
		private Color slapColor = Color.red;
		[SerializeField]
		private Color healColor = Color.green;

		public void Show(float change)
		{
			if(state != State.Idle)
				return;

			SetState(State.Slap);
			slapWaitTime = 0f;
			slapTime = 0f;

			SetActive(true);

			bool useBlur = change < 0f && radialBlur != null && !radialBlur.alreadyUsed;

			Independent.Coroutine.Instance.ProcessCoroutine(SlapUpdate(change, useBlur));
		}

		private IEnumerator SlapUpdate(float change, bool useBlur)
		{
			while(state == State.Slap)
			{
				if(slapWaitTime > 0.1)
				{
					slapTime += Time.deltaTime;

					if(slapTime > 1.5f)
					{
						SetState(State.Idle);
						slapTime = 0.0f;
						slapWaitTime = 0.0f;

						SetActive(false);
					}
					else
					{
						float a = Mathf.Clamp(1f - slapTime, 0.0f, 1.0f);

						if(useBlur && radialBlur != null)
						{
							radialBlur.density = 0.12f * a;
						}

						Color c;

						if(change > 0f)
						{
							c = healColor;
						}
						else
						{
							c = slapColor;
						}

						c.a = a;

						SetColor(c);
					}
				}

				if(slapWaitTime <= 0.14f)
					slapWaitTime += Time.deltaTime * 0.46f;

				yield return null;
			}
		}

		private void SetState(State state)
		{
			this.state = state;
		}

		private void SetColor(Color color)
		{
			foreach(var c in corners)
			{
				if(c != null)
				{
					c.color = color;
				}
			}
		}

		public override void SetActive(bool active)
		{
			foreach(var c in corners)
			{
				if(c != null)
				{
					c.SetActive(active);
				}
			}
		}
	}
}