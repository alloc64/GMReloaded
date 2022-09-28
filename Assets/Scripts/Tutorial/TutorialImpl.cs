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
using System.Collections.Generic;

namespace GMReloaded.Tutorial
{
	public class TutorialImpl
	{
		private Queue<TutorialStep> stepQueue;

		private Dictionary<TutorialEvent, TutorialEventData> eventsData = new Dictionary<TutorialEvent, TutorialEventData>(); 

		//

		private TutorialStep currStep = null;

		//

		private WeaponType[] initEquippedWeaponTypes;

		//

		private GMReloaded.Tutorial.HUD.TutorialWindow tutorialWindow;

		public bool randomGenerateBonuses { get; set; }

		public TutorialSceneObjectsManager sceneObjects { get; private set; }

		public HUD.TutorialFadeInOut fadeInOut { get; private set; }

		//

		public TutorialImpl(System.Collections.Generic.Queue<TutorialStep> stepQueue)
		{
			this.stepQueue = new Queue<TutorialStep>();

			foreach(var step in stepQueue)
			{
				this.stepQueue.Enqueue(step);
			}
		}

		//

		private void Initialize()
		{
			this.tutorialWindow = GameObject.FindObjectOfType<GMReloaded.Tutorial.HUD.TutorialWindow>();
			Assert.IsAssigned(this.tutorialWindow);

			this.sceneObjects = GameObject.FindObjectOfType<GMReloaded.Tutorial.TutorialSceneObjectsManager>();
			Assert.IsAssigned(this.sceneObjects);

			this.fadeInOut = GameObject.FindObjectOfType<GMReloaded.Tutorial.HUD.TutorialFadeInOut>();
			Assert.IsAssigned(this.fadeInOut);

			initEquippedWeaponTypes = Config.Weapons.localClientEquipedWeapons.Dump();
		}

		private void Restore()
		{
			Config.Weapons.localClientEquipedWeapons.Restore(initEquippedWeaponTypes);
		}

		//

		public void HandleEvent(TutorialEvent eventType, object data)
		{
			//

			switch(eventType)
			{
				case TutorialEvent.OnGameStarted:
					Initialize();
				break;
					
				case TutorialEvent.OnLeftTutorial:
					Restore();
				break;
			}

			TutorialStep step = null;

			//Debug.Log("stepQueue " + stepQueue.Count);

			while(stepQueue != null && stepQueue.Count > 0 && step == null)
				step = stepQueue.Peek();

			if(eventType != TutorialEvent.OnAnyKeyPressed && eventType != TutorialEvent.OnMovedForward)
				Debug.Log("HandleEvent " + eventType + " / " + data);
			//Debug.Log("TutorialStep " + step);

			if(step == null)
				return;

			TutorialEventData ted = null;

			eventsData.TryGetValue(eventType, out ted);

			if(ted == null)
				eventsData[eventType] = ted = new TutorialEventData(eventType);

			ted.usedCount++;

			if(currStep == null && step.eventType == eventType && step.ExecutionCondition(ted) && (step.desiredData == null || step.desiredData.Equals(data)))
			{
				this.currStep = step;

				ExecuteStep(step);
			}
		}

		private void ExecuteStep(TutorialStep step)
		{
			if(tutorialWindow == null)
			{
				Debug.LogError("tutorialWindow == null");
				return;
			}

			stepQueue.Dequeue();

			if(step is InvisibleTutorialStep)
			{
				OnStepStarted(step);

				tutorialWindow.SetActive(false);

				OnStepEnded(step);
			}
			else
			{
				tutorialWindow.Execute(this, step);
			}
		}

		#region Step Events

		public void OnStepStarted(TutorialStep step)
		{
			Debug.Log("OnStepStarted " + step);

			step.OnStepStartedDispatch();
		}

		public void OnStepEnded(TutorialStep step)
		{
			Debug.Log("OnStepEnded " + step);

			step.OnStepEndedDispatch();

			currStep = null;
		}

		#endregion
	}
}