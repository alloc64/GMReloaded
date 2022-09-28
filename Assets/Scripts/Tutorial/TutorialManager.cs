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
using CodeStage.AntiCheat.ObscuredTypes;

namespace GMReloaded.Tutorial
{
	public class TutorialManager
	{
		private static TutorialManager _instance = null;
		public static TutorialManager Instance
		{
			get
			{
				if(_instance == null)
					_instance = new TutorialManager();

				return _instance;
			}
		}

		//

		private const string completedTutorialKey = "TM_CompletedTutorialKey_R0.1";
		public static bool isTutorialCompleted 
		{
			get 
			{ 
				return Cloud.CloudSyncedPlayerPrefs.GetBool(completedTutorialKey); 
			} 

			#if UNITY_EDITOR
			set
			#else
			private set 
			#endif
			{ 
				Cloud.CloudSyncedPlayerPrefs.SetBool(completedTutorialKey, value); 
			} 
		}

		//

		public bool isActive { get { return impl != null; } }

		public bool randomGenerateBonuses { get { return impl != null && impl.randomGenerateBonuses; } set { if(impl != null) impl.randomGenerateBonuses = value; } }

		public TutorialSceneObjectsManager sceneObjects { get { return impl == null ? null : impl.sceneObjects; } }

		public HUD.TutorialFadeInOut fadeInOut { get { return impl == null ? null : impl.fadeInOut; } }

		//

		private TutorialImpl impl = null;

		//

		public void Initialize(TutorialImpl impl)
		{
			this.impl = impl;

			//
		}

		#region Events

		public void HandleEvent(TutorialEvent eventType, object data = null)
		{
			if(impl != null)
				impl.HandleEvent(eventType, data);
		}

		#endregion

		//

		public void OnFirstStepCompleted()
		{
			isTutorialCompleted = true;
		}

		public void OnTutorialCompleted()
		{
			GameStateController.Instance.LeaveRoom();
		}

		//

		public static void Destroy()
		{
			if(_instance == null)
				return;

			_instance.OnDestroy();
		}

		private void OnDestroy()
		{
			HandleEvent(TutorialEvent.OnLeftTutorial);

			impl = null;
			_instance = null;
		}
	}
}