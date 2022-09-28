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
using System;

using Input = TeamUtility.IO.InputManager;
using GMReloaded.Bonuses;

namespace GMReloaded
{
	public class HUD : MonoSingletonFromScene<HUD>
	{
		[SerializeField]
		private ScreenCorners screenCorners;

		[SerializeField]
		private HealthBar healthBar;

		[SerializeField]
		public PlayerStatsTable playerStats;

		[SerializeField]
		private GameObject content;

		[SerializeField]
		private HUDActiveBonusStack activeBonusStack;

		[SerializeField]
		public HUDPasiveBonusStack pasiveBonusStack;

		[SerializeField]
		public GameInfoStack gameInfoStack;

		[SerializeField]
		public Crosshair crosshair;

		[SerializeField]
		public RespawnIndicator respawnIndicator;

		[SerializeField]
		public WeaponIndicator weaponIndicator;

		[SerializeField]
		public PlayerLevelIndicator playerLevelIndicator;

		[SerializeField]
		public MissionIndicator missionIndicator;

		[SerializeField]
		public GameMessage gameMessage;

		[SerializeField]
		public Flashbang flashbang;

		[SerializeField]
		public UI.Final.ChatConsole chatConsole;

		[SerializeField]
		public HitIndicator hitIndicator;

		[SerializeField]
		public GMReloaded.UI.HUD.MadnessMode.HUDMadnessModeStack madnessModeStack;

		//

		private static Action OnLoadedEvent; 

		//

		private GrainScreen grain { get { return RobotEmilImageEffects.Instance.grain; } }

		private GMReloaded.UI.Final.KBMenuRenderer menuRenderer { get { return GMReloaded.UI.Final.KBMenuRenderer.IsNull ? null : GMReloaded.UI.Final.KBMenuRenderer.Instance; } }

		//

		public static void LoadScene(Action onLoaded)
		{
			OnLoadedEvent = onLoaded;
			LoadScene("HUD", false);
		}

		public override void OnLoaded(string _loadedSceneID)
		{
			base.OnLoaded(_loadedSceneID);

			if(OnLoadedEvent != null)
			{
				OnLoadedEvent();
				OnLoadedEvent = null;
			}
		}

		#region Unity

		private void Awake()
		{
		}

		#if TRAILER_VERSION

		private bool hudVisible = true;

		#endif

		private void Update()
		{
			if(Input.GetButtonUp(Config.Player.KeyBind.ChatConsole))
				chatConsole.Show();

			#if !UNITY_WEBPLAYER

			if(Input.GetKeyUp(KeyCode.Escape))
				chatConsole.Hide();
			
			#endif

			if(menuRenderer != null && menuRenderer.isInMenu)
				return;
			
			playerStats.ShowOnClick(Input.GetButton(Config.Player.KeyBind.ScoreTable));


			#if TRAILER_VERSION

			if(Input.GetKeyUp(KeyCode.H))
			{
				hudVisible = !hudVisible;
				SetActive(hudVisible);
			}

			#endif
		}

		#endregion


		public void Show()
		{
			SetActive(true);
		}

		public void Hide()
		{
			SetActive(false);
		}

		public override void SetActive(bool active)
		{
			if(content != null)
				content.SetActive(active);
		}

		public void SetHP(float p)
		{
			if(healthBar != null)
				healthBar.SetHP(p);
		}

		public void SetArenaTime(int time)
		{
			int seconds = time % 60;
			int minutes = time / 60;

			var arenaTimeFormated = string.Format("{0}:{1:00}", minutes, seconds);

			playerStats.SetArenaTime(arenaTimeFormated);
		}

		#region Local player Events

		public void OnLocalPlayerJoined(PhotonPlayer player)
		{
			if(playerStats != null)
			{
				foreach(var p in PhotonNetwork.playerList)
					playerStats.AddPlayer(p);
			}

			if(gameInfoStack != null)
				gameInfoStack.PlayerInfo(PhotonNetwork.player.name + " connected");

		}

		public void OnLocalPlayerDied()
		{
			if(crosshair != null)
				crosshair.Hide();

			if(grain != null)
				grain.OnPlayerHPChanged(1f);
		}

		public void OnLocalPlayerSpawned()
		{
			if(crosshair != null)
				crosshair.Show();
		}

		public void OnPlayerHPChanged(float normalizedLives, float change)
		{
			if(grain != null)
				grain.OnPlayerHPChanged(normalizedLives);

			if(screenCorners != null)
				screenCorners.Show(change);
		}


		public void OnLocalPlayerActiveBonusPickedUp(Bonus bonus)
		{
			if(activeBonusStack == null || bonus == null)
				return;

			activeBonusStack.PickUpBonus(bonus);
		}

		public void OnLocalPlayerActiveBonusPickUpRefused(Bonus bonus)
		{
			Debug.Log("OnLocalPlayerActiveBonusPickUpRefused " + bonus);

			if(activeBonusStack == null || bonus == null)
				return;

			activeBonusStack.OnBonusPickUpRefused(bonus);
		}

		//

		public void OnLocalPlayerPasiveBonusPickedUp(Bonus bonus, int useCount)
		{
			if(pasiveBonusStack == null || bonus == null)
				return;

			pasiveBonusStack.SetBonusUse(bonus, useCount);
		}

		public void OnLocalPlayerPasiveBonusPickUpRefused(Bonus bonus)
		{
			Debug.Log("OnLocalPlayerPasiveBonusPickUpRefused " + bonus);
			if(pasiveBonusStack == null || bonus == null)
				return;

			pasiveBonusStack.OnBonusPickUpRefused(bonus);
		}

		public void OnLocalPlayerPasiveBonusDispatchStopped(Bonus bonus, int useCount)
		{
			if(pasiveBonusStack == null || bonus == null)
				return;

			pasiveBonusStack.SetBonusUse(bonus, useCount);
		}


		#endregion

		#region Madness Events Stack

		public void OnMadnessStepPrepareForDispatch(Config.MadnessMode.MadnessStep step, int prepareForDispatchTime)
		{
			if(madnessModeStack == null || step == null)
				return;

			madnessModeStack.PrepareMadnessStepForDispatch(step, prepareForDispatchTime);
		}

		public void OnMadnessStepStartDispatch(Config.MadnessMode.MadnessStep step)
		{
			if(madnessModeStack == null)
				return;

			madnessModeStack.DispatchMadnessStep(step);
		}

		public void OnTimedMadnessStepDispatched(Config.MadnessMode.MadnessStep step)
		{
			if(madnessModeStack == null)
				return;

			madnessModeStack.TimedMadnessStepDispatched(step);
		}

		#endregion

		#region GameInfoStack

		// jmena funkci se biji s internima z unity
		public void OnPlayerConnectedAED(PhotonPlayer player)
		{
			if(playerStats != null)
				playerStats.AddPlayer(player);

			if(gameInfoStack != null)
				gameInfoStack.PlayerInfo(player.name + " connected");
		}

		public void OnPlayerDisconnectedAED(PhotonPlayer player)
		{
			if(playerStats != null)
				playerStats.RemovePlayer(player);

			if(gameInfoStack != null)
				gameInfoStack.PlayerInfo(player.name + " disconnected");
		}

		#endregion
	}
	
}