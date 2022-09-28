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
using System.Collections.Generic;
using TouchOrchestra;

namespace GMReloaded.UI.Final
{
	public class KBMenuRenderer : MonoSingletonPersistent<KBMenuRenderer>
	{
		[System.Serializable]
		private class MenuItem
		{
			public State state;
			public KBFocusableSuccessorsGUI gui;
			public bool useBackgroundSprite = false;
		}

		public enum State
		{
			Loading,
			MainMenu,
			ServerList,
			Equip,
			Weapons,
			Missions,
			CreateGame,
			CreateGame_MadnessMode,
			Charts,
			Settings,


			//toto se este zmeni
			InGame,
			GamePaused,

			Changelog,
		}

		[SerializeField]
		private GameObject menuContainer;

		[SerializeField]
		private Error.KBError error;

		[SerializeField]
		public Popup.KBPopup popup;

		[SerializeField]
		public Popup.Nick.KBRobotNickPopup robotNickPopup;

		[SerializeField]
		private AutoPlayerLevelIndicator playerLevelIndicator;

		[SerializeField]
		private MenuItem[] menuItems;

		[SerializeField]
		private Kino.Bloom bloom;

		[SerializeField]
		private GameObject solidBackgroundGO;

		[SerializeField]
		private tk2dBaseSprite fadeInOutSprite;

		[SerializeField]
		private tk2dBaseSprite transparentBackgroundSprite;

		[SerializeField]
		private tk2dTextMesh clientVersionText;
 
		private Dictionary<State, MenuItem> _menuItemsCache;
		private Dictionary<State, MenuItem> menuItemsCache
		{
			get
			{
				if(_menuItemsCache == null)
				{
					_menuItemsCache = new Dictionary<State, MenuItem>();

					foreach(var mi in menuItems)
					{
						if(mi != null)
						{
							var gui = mi.gui;

							if(gui != null)
							{
								gui.state = mi.state;
							}

							_menuItemsCache[mi.state] = mi;
						}
					}
				}

				return _menuItemsCache;
			}
		}

		[SerializeField]
		private State state;

		[SerializeField]
		private State lastState;

		public float outMenuTimer = 0f;
		public bool isInMenu { get { return state != State.InGame || outMenuTimer < 0.4f || isChatting; } }

		public bool isChatting { get ; set; }

		//

		private CreateGame.KBGameRoom _gameRoom = null; 
		public CreateGame.KBGameRoom gameRoom 
		{ 
			get { return _gameRoom; } 
			set 
			{ 
				_gameRoom = value; 

				#if UNITY_EDITOR 
					Debug.Log("gameroom set " + _gameRoom); 
				#endif 
			} 
		}

		//

		public KBFocusableSuccessorsGUI focusedGUI { get; private set; }
		private KBFocusableSuccessorsGUI lastFocusedGUI;

		//

		private GlobalStateController gsc { get { return GlobalStateController.Instance; } }

		private API.Login login { get { return API.Login.Instance; } }

		//

		private Stack<State> backStack = new Stack<State>();

		//

		#region Unity

		protected override void Awake()
		{
			Scene.UISceneLoader.LoadSceneAdditive();

			#if UNITY_XBOXONE && !UNITY_EDITOR
			GMReloaded.Xbox.XboxAPI.GetInstance();
			#else

			#if !UNITY_METRO && STEAM_ENABLED
			GMReloaded.Steam.Steamworks.GetInstance();
			#endif

			#endif

			Utils.Initialize();

			Settings.Tabs.KBGraphicsTab.SetPresetValues();

			Independent.Timer.GetInstance();

			TouchOrchestra.Ease.GetInstance();

			SoundManager.GetInstance();

			Config.madnessMode.SetMadnessSteps();

			gsc.OnGameStarted();

			base.Awake();

			if(menuContainer != null)
				menuContainer.SetActive(true);

			SetState(null, KBMenuRenderer.State.Loading);
	
			#if !UNITY_EDITOR

			CodeStage.AntiCheat.Detectors.SpeedHackDetector.StartDetection(OnSpeedhackDetected);
			CodeStage.AntiCheat.Detectors.ObscuredCheatingDetector.StartDetection(OnPPHackDetected);

			CodeStage.AntiCheat.Detectors.InjectionDetector.StartDetection(OnInjectionDetected);
			#endif

			if(clientVersionText != null)
				clientVersionText.text = localization.GetValue("ClientVersionBottom", Config.clientVersion);
		}

		protected virtual void Start()
		{
			#if UNITY_WEBPLAYER
			Shader.WarmupAllShaders();
			#endif
		}

		protected virtual void Update()
		{
			if(state == State.InGame)
				outMenuTimer += Independent.Timer.deltaTime;
			else
				outMenuTimer = 0f;
		}

		#endregion

		#region Hack events

		private void OnSpeedhackDetected()
		{
			OnHacked("SpeedHack");
		}

		private void OnPPHackDetected()
		{
			OnHacked("PlayerPrefs");
		}

		private void OnInjectionDetected()
		{
			OnHacked("DLLInjection");
		}

		private void OnHacked(string type)
		{
			login.LogEvent("hacked", type);
		}

		#endregion

		#region GUI States

		public void SetState(KBFocusableSuccessorsGUI parentGUI, State state, object bundle = null)
		{
			//Debug.Log("SetState " + parentGUI + " - " + state + ", lastState: " + lastState);

			this.lastState = this.state;
			this.state = state;

			GMReloaded.Analytics.GAI.Instance.LogScreen(this.state.ToString() + (gsc.isInGame ? "_InGame" : ""));

			var mi = GetGUIMenuItem(state);

			if(mi == null)
			{
				Debug.LogError("Failed to SetState " + state + " - mi == null");
				return;
			}

			var uiScene = Scene.UIScene.Instance;

			if(uiScene != null)
				uiScene.OnStateChanged(state);

			if(solidBackgroundGO != null && state == State.InGame)
				solidBackgroundGO.SetActive(false);

			playerLevelIndicator.SetActive(!gsc.isInGame && state != State.Loading);

			if(state == State.Loading)
			{
				Cloud.CloudAPI.Instance.DispatchAsyncQueue();

				backStack.Clear();
			}
			else
			{
				if(parentGUI != null)
					backStack.Push(parentGUI.state);
			}

			ShowGUI(mi.gui, uiScene, mi.useBackgroundSprite, bundle);
		}

		public void GoToLoading(System.Action OnBetweenFadeInOut)
		{
			FadeInOut(() =>
			{
				SetState(null, State.Loading);

				if(OnBetweenFadeInOut != null)
					OnBetweenFadeInOut();

				if(solidBackgroundGO != null)
					solidBackgroundGO.SetActive(true);
			});
		}

		public void GoBackToMainMenu()
		{
			FadeInOut(() =>
			{
				SetState(null, State.Loading);

				if(solidBackgroundGO != null)
					solidBackgroundGO.SetActive(true);
				
				Scene.UISceneLoader.LoadScene(() => 
				{
					FadeInOut(() => 
					{
						if(solidBackgroundGO != null)
							solidBackgroundGO.SetActive(false);

						SetState(null, KBMenuRenderer.State.MainMenu);
					});
				});
			});
		}

		private void FadeInOut(System.Action OnBetweenFadeInOut)
		{
			float t = 0.2f;

			fadeInOutSprite.SetAlpha(0f);
			fadeInOutSprite.SetActive(true);

			Ease.Instance.Alpha(0f, 1f, t, EaseType.In, fadeInOutSprite.SetAlpha, () =>
			{
				if(OnBetweenFadeInOut != null)
					OnBetweenFadeInOut();

				Ease.Instance.Alpha(1f, 0f, t, EaseType.Out, fadeInOutSprite.SetAlpha, () =>
				{
					fadeInOutSprite.SetActive(false);
				});
			});
		}

		public bool GoBack()
		{
			if(backStack.Count < 1)
			{
				Debug.LogWarning("GoBack backStack.Count < 1");
				return false;
			}

			SetState(null, backStack.Pop());

			return true;
		}

		#endregion

		#region GUI Management

		private MenuItem GetGUIMenuItem(State state)
		{
			MenuItem mi = null;

			menuItemsCache.TryGetValue(state, out mi);

			return mi;
		}

		private void ShowGUI(KBFocusableSuccessorsGUI gui, Scene.UIScene uiScene, bool useBackgroundSprite, object bundle = null)
		{
			foreach(var mi in menuItems)
			{
				if(mi != null)
				{
					var _gui = mi.gui;

					if(_gui != null)
						_gui.Hide();
				}
			}

			bool inGame = gsc.isInGame;

			//if(bloom != null)
			//	bloom.enabled = gui.state != State.InGame;

			//if(solidBackgroundGO != null)
			//	solidBackgroundGO.SetActive(!inGame);
			
			if(transparentBackgroundSprite != null)
				transparentBackgroundSprite.SetActive(inGame && useBackgroundSprite);
			
			if(uiScene != null && gui.state == State.MainMenu)
				uiScene.PlayAnimation();
			
			if(gui != null)
				gui.Show(bundle);
		}

		#endregion

		#region Loading

		public void SetLoadingHintText(string id)
		{
			var mi = GetGUIMenuItem(State.Loading);

			if(mi != null)
			{
				var gui = mi.gui as Loading.KBLoadingMenu;

				if(gui != null)
				{
					gui.SetHint(id);
				}
			}
		}

		#endregion

		#region Error

		public void SetErrorLocalized(string locId, params object[] p)
		{
			if(error == null)
			{
				Debug.LogError("Failed to set error - error == null");
				return;
			}

			error.SetErrorLocalized(locId, p);
		}

		public void SetError(string text)
		{
			if(error == null)
			{
				Debug.LogError("Failed to set error - error == null");
				return;
			}

			error.SetError(text);
		}

		#endregion

		#region GUI Focus

		public void SetFocusedGUI(KBFocusableSuccessorsGUI focusedGUI)
		{
			this.lastFocusedGUI = this.focusedGUI;
			this.focusedGUI = focusedGUI;
		}

		public void RestoreLastFocusedGUI()
		{
			if(lastFocusedGUI == null)
			{
				Debug.LogWarning("Failed to restore last focused GUI - lastFocusedGUI == null");
				return;
			}

			SetFocusedGUI(lastFocusedGUI);
		}

		#endregion
	}
	
}
