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
using System;

using Input = TeamUtility.IO.InputManager;
using System.Collections.Generic;

namespace GMReloaded.Tutorial
{
	public class TutorialStep
	{
		public TutorialEvent eventType { get; private set; }

		public List<TutorialMessage> messages = new List<TutorialMessage>();

		private string _action = string.Empty;
		public string action { get { return _action; } private set { _action = value; } }

		public float delay { get; private set; }

		public Action OnStepStarted;

		public Action OnStepEnded; 

		public Func<TutorialEventData, bool> ExecutionCondition = (tutorialEventData) => true;

		public object desiredData { get; private set; }

		//

		private const RobotEmilViewObserver.PlayerLimitations defaultPlayerLimitations = RobotEmilViewObserver.PlayerLimitations.WeaponSwitch;

		public RobotEmilViewObserver.PlayerLimitations _onStartPlayerLimitations = defaultPlayerLimitations;
		public RobotEmilViewObserver.PlayerLimitations onStartPlayerLimitations
		{ 
			get { return _onStartPlayerLimitations; } 
			private set {  _onStartPlayerLimitations = value.Add(defaultPlayerLimitations); } 
		}

		public RobotEmilViewObserver.PlayerLimitations _onEndPlayerLimitations = defaultPlayerLimitations;
		public RobotEmilViewObserver.PlayerLimitations onEndPlayerLimitations
		{ 
			get { return _onEndPlayerLimitations; } 
			private set {  _onEndPlayerLimitations = value.Add(defaultPlayerLimitations); } 
		}

		//

		protected static Localization localization { get { return Localization.Instance; } }

		public TutorialManager tutorialManager { get { return TutorialManager.Instance; } }

		public TutorialSceneObjectsManager sceneObjects { get { return tutorialManager.sceneObjects; } }

		public HUD.TutorialFadeInOut fadeInOut { get { return tutorialManager.fadeInOut; } }

		public LocalClientRobotEmil lcre { get { return LocalClientRobotEmil.Instance; } }

		//

		public TutorialStep(TutorialEvent eventType)
		{
			this.eventType = eventType;
		}

		public TutorialStep(TutorialEvent eventType, string messageLocId) : this(eventType)
		{
			SetMessage(messageLocId);
		}

		//

		public TutorialMessage SetMessage(string messageLocId)
		{
			return AddMessage(messageLocId);
		}
		
		public TutorialMessage SetMessage(string messageLocId, params object[] @params)
		{
			return AddMessage(messageLocId, @params);
		}

		//

		public TutorialMessage AddMessage(string messageLocId)
		{
			var msg = new TutorialMessage(localization.GetValue(messageLocId));

			messages.Add(msg);

			return msg;
		}

		public TutorialMessage AddMessage(string messageLocId, params object[] @params)
		{
			var msg = new TutorialMessage(localization.GetValue(messageLocId, @params));

			messages.Add(msg);

			return msg;
		}

		public TutorialMessage GetMessage(int idx)
		{
			if(idx < 0 || idx >= messages.Count)
				return null;

			return messages[idx];
		}

		//

		public void SetAction(string actionLocId)
		{
			this.action = localization.GetValue(actionLocId);
		}

		public void SetAction(string actionLocId, params object[] @params)
		{
			this.action = localization.GetValue(actionLocId, @params);
		}

		public void SetActionByAxis(string axisId)
		{
			SetActionByKeyCode(GetAxis(axisId).positive);
		}

		public void SetActionByKeyCode(KeyCode keyCode)
		{
			SetAction("Tut_PressKey", localization.GetValue(keyCode.ToString()));
		}

		public void SetPressAnyKeyAction()
		{
			SetAction("Tut_PressAnyKey");
		}

		//

		public void SetDelay(float delay)
		{
			this.delay = delay;
		}

		//

		public void SetDesiredData(object data)
		{
			this.desiredData = data;
		}

		//

		#region Player Specific Impl

		public void SpawnOnDefaultSpawn()
		{
			var spawn = sceneObjects.GetObject<SpawnPoint>("PlayerDefaultSpawn");

			if(spawn == null)
			{
				Debug.LogError("Failed to SpawnOnDefaultSpawn - spawn == null");
				return;
			}

			if(lcre == null)
			{
				Debug.LogError("Failed to SpawnOnDefaultSpawn - lcre == null");
				return;
			}

			lcre.parentRobot.Spawn(spawn);
		}

		//

		public void SetOnStartPlayerLimitations(RobotEmilViewObserver.PlayerLimitations limit)
		{
			this.onStartPlayerLimitations = limit;
		}

		public void SetOnEndPlayerLimitations(RobotEmilViewObserver.PlayerLimitations limit)
		{
			this.onEndPlayerLimitations = limit;
		}

		private void UpdatePlayerLimitations(RobotEmilViewObserver.PlayerLimitations limit)
		{
			if(lcre == null)
			{
				Debug.LogError("Failed to UpdatePlayerLimitations - lcre == null");
				return;
			}

			Debug.Log("UpdatePlayerLimitations " + limit);

			lcre.parentRobot.viewObserver.SetMovementLimitations(limit);
		}

		//

		public void DemolishAllEntities()
		{
			if(sceneObjects == null)
			{
				Debug.LogError("Failed to DemolishAllEntities - sceneObjects == null");
				return;
			}

			var tec = sceneObjects.GetObject<GMReloaded.Tutorial.Helpers.TutorialEntityContainers>("EntityContainers");

			if(tec == null)
			{
				Debug.LogError("Failed to DemolishAllEntities - tec == null");
				return;
			}

			tec.DemolishEntities();
		}

		public GMReloaded.Entities.EntityContainer GetEntity(string entityId)
		{
			var tec = sceneObjects.GetObject<GMReloaded.Tutorial.Helpers.TutorialEntityContainers>("EntityContainers");

			if(tec == null)
			{
				Debug.LogError("Failed to DemolishAllEntities - tec == null");
				return null;
			}

			return tec.GetEntityContainer(entityId);
		}

		public GMReloaded.Entities.BoxDestroyable GetDestroyableBox(string entityId)
		{
			var entityContainer = GetEntity(entityId);

			if(entityContainer != null)
				return entityContainer.FindChildDestroyableEntity() as GMReloaded.Entities.BoxDestroyable;

			return null;
		}

		#endregion

		#region Events

		public void OnStepStartedDispatch()
		{
			UpdatePlayerLimitations(onStartPlayerLimitations);

			if(OnStepStarted != null)
				OnStepStarted();
			
			Analytics.GAI.Instance.LogScreen("Tutorial: " + eventType + " - " + desiredData + " - " + action);
		}

		public void OnStepEndedDispatch()
		{
			UpdatePlayerLimitations(onEndPlayerLimitations);
			
			if(OnStepEnded != null)
				OnStepEnded();
		}

		public void GoToNextStep()
		{
			Timer.DelayAsyncIndependent(2f, () => tutorialManager.HandleEvent(TutorialEvent.OnNextStepForced));
		}

		#endregion

		#region Keybinding

		public static TeamUtility.IO.AxisConfiguration GetAxis(string name) 
		{ 
			return Input.GetAxisConfiguration("KeyboardMouse", name); 
		} 

		public static string GetPositiveKey(string axisName)
		{
			return localization.GetValue(GetAxis(axisName).positive.ToString());
		}

		#endregion

		public override string ToString()
		{
			return string.Format("[TutorialStep] eventType={0}, message={1}, action={2}, delay={3}", eventType, messages, action, delay);
		}
	}
}