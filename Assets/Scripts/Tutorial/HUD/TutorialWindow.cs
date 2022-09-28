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
using GMReloaded.UI.Final;

using Input = TeamUtility.IO.InputManager;

namespace GMReloaded.Tutorial.HUD
{
	public class TutorialWindow : MonoBehaviourTO 
	{
		[SerializeField]
		private tk2dTextMesh textMesh;

		[SerializeField]
		private tk2dTextMesh actionTextMesh;

		[SerializeField]
		private GameObject container;

		//

		private TutorialImpl tutorialImpl;

		private TutorialStep step;

		private int messageIdx = 0;

		private float textTrimTimer = 0f;


		//

		private Timer actionTextMeshAlfaTimer = new Timer();

		//

		private KBMenuRenderer menuRenderer { get { return KBMenuRenderer.Instance; }}

		//

		private static readonly string nextMessageActionKey = Config.Player.KeyBind.AttackPrimary;

		#region Unity

		private void Awake()
		{
			if(textMesh != null)
				textMesh.text = "";

			SetActionText("");
		}

		private bool waitingForUserInput = false;

		private void Update()
		{
			if(!tutorial.isActive || textMesh == null || menuRenderer.isInMenu)
				return;

			UpdateText(textMesh.text);

			if(waitingForUserInput)
			{
				if(Input.GetButtonUp(nextMessageActionKey))
				{
					UpdateMessage();
					waitingForUserInput = false;
				}
			}

			if(actionTextMesh != null)
				actionTextMesh.SetAlpha(actionTextMeshAlfaTimer.Loop(0.6f, 1f, 1f));
		}

		private void UpdateText(string text)
		{
			if(!string.IsNullOrEmpty(text) && textTrimTimer < text.Length)
			{
				textTrimTimer += Independent.Timer.deltaTime * 30f;

				textMesh.maxChars = (int)textTrimTimer;

				if(((int)textTrimTimer) >= text.Length)
				{
					Debug.Log(messageIdx + " >= " + step.messages.Count);

					if(messageIdx >= step.messages.Count)
					{
						OnStepEnded();
					}
					else
					{
						waitingForUserInput = true;

						SetActionTextLocalized("Tut_PressKey", localization.GetValue(TutorialStep.GetAxis(nextMessageActionKey).positive.ToString()));
					}
				}
				else
				{
					if(Input.GetButtonUp(nextMessageActionKey))
					{
						textTrimTimer = text.Length-1;
					}
				}
			}
		}

		#endregion

		public void Execute(TutorialImpl tutorialImpl, TutorialStep step)
		{
			if(step.delay > 0f)
			{
				SetActive(false);

				tutorialImpl.OnStepStarted(step);

				Timer.DelayAsyncIndependent(step.delay, () => Show(tutorialImpl, step));
			}
			else
			{
				tutorialImpl.OnStepStarted(step);

				Show(tutorialImpl, step);
			}
		}

		public void Show(TutorialImpl tutorialImpl, TutorialStep step)
		{
			this.tutorialImpl = tutorialImpl;
			this.step = step;

			Debug.Log("TutorialWindow.Show " + step);

			SetActive(true);

			waitingForUserInput = false;

			messageIdx = 0;
			UpdateMessage();
		}

		private void UpdateMessage()
		{
			SetActionText("");

			if(textMesh != null)
			{
				var currMessage = step.GetMessage(messageIdx);

				if(currMessage != null && !string.IsNullOrEmpty(currMessage.message))
				{
					Debug.Log("SetMessage " + currMessage.message);

					textMesh.text = currMessage.message;
					textMesh.maxChars = 0;

					textTrimTimer = 0;

					if(currMessage.OnMessageStarted != null)
						currMessage.OnMessageStarted();
				}
			}

			messageIdx++;

			Debug.Log("UpdateMessage messageIdx " + messageIdx);
		}

		private void OnStepEnded()
		{
			SetActionText(step.action);

			tutorialImpl.OnStepEnded(step);
		}

		private void SetActionTextLocalized(string locId, params object[] p)
		{
			SetActionText(localization.GetValue(locId, p));
		}

		private void SetActionText(string text)
		{
			if(actionTextMesh != null)
				actionTextMesh.text = text;
		}

		public override void SetActive(bool active)
		{
			if(container != null)
				container.SetActive(active);
		}
	}
}