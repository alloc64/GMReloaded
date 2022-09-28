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
using System.Collections.Generic;


#if !UNITY_METRO && STEAM_ENABLED

using Steamworks;


namespace GMReloaded.Steam
{
	public class SteamStats
	{
		private Steamworks steamworks = null;

		private Dictionary<string, object> defaultStats = new Dictionary<string, object>();
		private Dictionary<string, object> stats = new Dictionary<string, object>();

		public SteamStats(Steamworks steamworks)
		{
			this.steamworks = steamworks;

			RegisterStat(GMReloaded.API.Score.Deaths, 0);
			RegisterStat(GMReloaded.API.Score.Kills, 0);
			RegisterStat(GMReloaded.API.Score.Rounds, 0);
		}

		//

		public void RequestStats()
		{
			if(steamworks == null)
				return;

			foreach(var kvp in defaultStats)
			{
				string key = kvp.Key;

				var data = kvp.Value;

				if(data is int)
				{
					int i = 0;
					SteamUserStats.GetStat(key, out i);

					//Debug.Log("Stat debug " + key + " = " + i);

					stats[key] = i;
				}
				else if(data is float)
				{
					float f = 0f;
					SteamUserStats.GetStat(key, out f);

					//Debug.Log("Stat debug " + key + " = " + f);

					stats[key] = f;
				}
				else
				{
					Debug.LogError("Unknown get stat data type " + key);
				}

#if UNITY_EDITOR
				Debug.Log("Loaded stat " + key + " = " + GetStat(key));
#endif
			}
		}

		public void SetStat(string id, int value)
		{
			stats[id] = value;
		}

		public void SetStat(string id, float value)
		{
			stats[id] = value;
		}

		public object GetStat(string id)
		{
			object value = null;

			if(!stats.TryGetValue(id, out value))
				Debug.Log("Stat value not found " + id);

			return value;
		}

		public void IncrementStat(string id, float value)
		{
			var prevValue = (float)GetStat(id);

			prevValue += value;

			SetStat(id, prevValue);
		}

		public void IncrementStat(string id, int value)
		{
			var prevValue = (int)GetStat(id);

			prevValue += value;

			SetStat(id, prevValue);
		}

		public void StoreStats(bool dummyParam)
		{
			if(steamworks == null)
				return;

			foreach(var kvp in stats)
			{
				string key = kvp.Key;

				var data = kvp.Value;

				//Debug.Log("Storing stat " + key + " = " + data);

				if(data is int)
				{
					SteamUserStats.SetStat(key, (int)data);
				}
				else if(data is float)
				{
					SteamUserStats.SetStat(key, (float)data);
				}
				else
				{
					Debug.LogError("Unknown set stat data type " + key);
				}
			}
		}

		//

		private void RegisterStat(string stat, object initValue)
		{
			defaultStats[stat] = initValue;
		}
	}

	/// <summary>
	/// Make sure that you have the steam client running and have setup everything according to the 
	/// Setup Instructions in the documentation, or the library will fail to load.
	/// </summary>
	public class Steamworks : MonoSingletonPersistent<Steamworks>
	{
		private bool _initialized;

		public static bool Initialized  { get { return Instance._initialized; } }

		private SteamAPIWarningMessageHook_t _SteamAPIWarningMessageHook;
		//private Callback<GameOverlayActivated_t> _gameOverlayActivated;

		// Steam - Leaderboards

		private CallResult<LeaderboardFindResult_t> LeaderboardFindResult;
		private CallResult<LeaderboardScoresDownloaded_t> LeaderboardScoresDownloaded;
		private CallResult<LeaderboardScoresDownloaded_t> LeaderboardScoresForUsersDownloaded;
		private CallResult<LeaderboardScoreUploaded_t> LeaderboardScoreUploaded;

		// Steam - Stats

		public SteamStats stats;

		private Callback<UserStatsReceived_t> UserStatsReceived;
		private Callback<UserStatsStored_t> UserStatsStored;

		// Steam - invite

		private Callback<GameLobbyJoinRequested_t> m_GameLobbyJoinRequested;

		protected override void Awake()
		{
			base.Awake();

			if(!Packsize.Test())
			{
				Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
			}

			if(!DllCheck.Test())
			{
				Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
			}

			try
			{
				// If Steam is not running or the game wasn't started through Steam, SteamAPI_RestartAppIfNecessary starts the 
				// Steam client and also launches this game again if the User owns it. This can act as a rudimentary form of DRM.

				// Once you get a Steam AppID assigned by Valve, you need to replace AppId_t.Invalid with it and
				// remove steam_appid.txt from the game depot. eg: "(AppId_t)480" or "new AppId_t(480)".
				// See the Valve documentation for more information: https://partner.steamgames.com/documentation/drm#FAQ
				if(SteamAPI.RestartAppIfNecessary(Config.Steam.SteamAppID))
				{
					Application.Quit();
					return;
				}
			}
			catch(System.DllNotFoundException e)
			{ // We catch this exception here, as it will be the first occurence of it.
				Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e, this);

				Application.Quit();
				return;
			}

			// Initialize the SteamAPI, if Init() returns false this can happen for many reasons.
			// Some examples include:
			// Steam Client is not running.
			// Launching from outside of steam without a steam_appid.txt file in place.
			// Running under a different OS User or Access level (for example running "as administrator")
			// Valve's documentation for this is located here:
			// https://partner.steamgames.com/documentation/getting_started
			// https://partner.steamgames.com/documentation/example // Under: Common Build Problems
			// https://partner.steamgames.com/documentation/bootstrap_stats // At the very bottom

			// If you're running into Init issues try running DbgView prior to launching to get the internal output from Steam.
			// http://technet.microsoft.com/en-us/sysinternals/bb896647.aspx
			_initialized = SteamAPI.Init();

			if(!_initialized)
			{
				Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);

				return;
			}

			//_gameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);

			try
			{
				SteamUtils.SetOverlayNotificationPosition(ENotificationPosition.k_EPositionBottomLeft);
			}
			catch(Exception e)
			{
				Debug.LogException(e);
			}

			string steamLang = SteamApps.GetCurrentGameLanguage();

			if(steamLang != null && steamLang.Length > 1)
			{
				steamLang = char.ToUpper(steamLang[0]) + steamLang.Substring(1);

				try
				{
					SystemLanguage language = (SystemLanguage)System.Enum.Parse(typeof(SystemLanguage), steamLang);

					Debug.Log("Language used in steam " + language);
				}
				catch(Exception e)
				{
					Debug.Log(e);
				}
			}
		}


		// This should only ever get called on first load and after an Assembly reload, You should never Disable the Steamworks Manager yourself.
		private void OnEnable()
		{
			if(!_initialized)
				return;

			if(_SteamAPIWarningMessageHook == null)
			{
				// Set up our callback to recieve warning messages from Steam.
				// You must launch with "-debug_steamapi" in the launch args to recieve warnings.
				_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
				SteamClient.SetWarningMessageHook(_SteamAPIWarningMessageHook);
			}

			//

			LeaderboardFindResult = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardFindResult);
			LeaderboardScoresDownloaded = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardScoresDownloaded);
			LeaderboardScoresForUsersDownloaded = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardScoresForUsersDownloaded);
			LeaderboardScoreUploaded = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardScoreUploaded);

			//

			UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
			UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);

			//

			m_GameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);


			RequestStats(() => 
			{
				stats = new SteamStats(this);
				stats.RequestStats();
			});

			//

			FindLeaderboard(Config.Steam.leaderboardId, (bool result, SteamLeaderboard_t leaderboard, HashSet<LeaderboardEntry_t> entries) =>
			{
				CSteamID[] Users = { SteamUser.GetSteamID() };
				SteamAPICall_t handle = SteamUserStats.DownloadLeaderboardEntriesForUsers(leaderboard, Users, Users.Length);
				LeaderboardScoresForUsersDownloaded.Set(handle);
			});
		}


		// OnApplicationQuit gets called too early to shutdown the SteamAPI.
		// Because the SteamManager should be persistent and never disabled or destroyed we can shutdown the SteamAPI here.
		// Thus it is not recommended to perform any Steamworks work in other OnDestroy functions as the order of execution can not be garenteed upon Shutdown. Prefer OnDisable().
		protected override void OnDestroy()
		{
			base.OnDestroy();

			if(!_initialized)
				return;
			
			SteamAPI.Shutdown();
		}

		private void Update()
		{
			if(!_initialized)
				return;
			
			// Run Steam client callbacks
			SteamAPI.RunCallbacks();
		}

		#region Overlay

		private void OnGameOverlayActivated(GameOverlayActivated_t pCallback) 
		{
			//TODO: v pripade problemu s GUI nastavovat rendereru ze jsem v Steam Overlayi
		}

		#endregion

		#region Player

		public string playerName { get { return _initialized ? SteamFriends.GetPersonaName() : null; } }

		#endregion

		#region Achievements

		public bool IndicateAchievementProgress(string key, int progress, int maxProgress)
		{
			if(!_initialized)
				return false;

			return SteamUserStats.IndicateAchievementProgress(key, (uint)progress, (uint)maxProgress);
		}

		public bool SetAchievement(string key)
		{
			if(!_initialized)
				return false;

			bool ret = SteamUserStats.SetAchievement(key);

			SteamUserStats.StoreStats();

			return ret;
		}

		public bool GetAchievement(string key)
		{
			if(!_initialized)
				return false;

			bool achieved = false;
			SteamUserStats.GetAchievement(key, out achieved);

			return achieved;
		}

		#endregion

		#region Leaderboards

		// status, leaderboard, leaderboard entries
		private Action<bool, SteamLeaderboard_t, HashSet<LeaderboardEntry_t>> onFindLeaderboardCallback;

		private string lastFindLeaderboardId = null;

		private Dictionary<string, SteamLeaderboard_t> leaderboardInstances = new Dictionary<string, SteamLeaderboard_t>();

		public void FindLeaderboard(string leaderboardId, Action<bool, SteamLeaderboard_t, HashSet<LeaderboardEntry_t>> onFindLeaderboardCallback)
		{
			if(!_initialized)
				return;
			
			this.lastFindLeaderboardId = leaderboardId;
			this.onFindLeaderboardCallback = onFindLeaderboardCallback;

			SteamAPICall_t handle = SteamUserStats.FindLeaderboard(leaderboardId);
			LeaderboardFindResult.Set(handle);
		}

		private void OnLeaderboardFindResult(LeaderboardFindResult_t callback, bool bIOFailure) 
		{
			//Debug.Log("OnLeaderboardFindResult " + callback.m_hSteamLeaderboard + " -- " + callback.m_bLeaderboardFound);

			if(callback.m_bLeaderboardFound != 0)
			{
				SteamAPICall_t handle = SteamUserStats.DownloadLeaderboardEntries(callback.m_hSteamLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 1, 20);
				LeaderboardScoresDownloaded.Set(handle);
			}
			else
			{
				Debug.LogError("Failed to find leaderboard in steam - error " + callback.m_bLeaderboardFound);

				if(onFindLeaderboardCallback != null)
				{
					onFindLeaderboardCallback(false, default(SteamLeaderboard_t), null);
					onFindLeaderboardCallback = null;
				}
			}
		}

		private void OnLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure) 
		{
			HashSet<LeaderboardEntry_t> entries = new HashSet<LeaderboardEntry_t>();

			for(int i = 0; i < pCallback.m_cEntryCount; i++)
			{
				LeaderboardEntry_t le;
				bool ret = SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, i, out le, null, 0);

				if(ret)
					entries.Add(le);
			}

			if(onFindLeaderboardCallback != null)
			{
				onFindLeaderboardCallback(true, pCallback.m_hSteamLeaderboard, entries);
				onFindLeaderboardCallback = null;
			}
		}

		//
		private int leaderboardEntryKills = -1;

		private void OnLeaderboardScoresForUsersDownloaded(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure)
		{
#if UNITY_EDITOR
			Debug.Log("OnLeaderboardScoresForUsersDownloaded " + lastFindLeaderboardId + " / " + pCallback.m_hSteamLeaderboard);
#endif
			if(lastFindLeaderboardId != null)
			{
				leaderboardInstances[lastFindLeaderboardId] = pCallback.m_hSteamLeaderboard;
			}

			for(int i = 0; i < pCallback.m_cEntryCount; i++)
			{
				LeaderboardEntry_t le;
				bool ret = SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, i, out le, null, 0);

				if(ret && le.m_steamIDUser == SteamUser.GetSteamID())
				{
					leaderboardEntryKills = le.m_nScore;
				}
			}

			// v pripade, ze nemam zaznam v charts

			if(leaderboardEntryKills == -1)
				leaderboardEntryKills = 0;
		}

		//

		public void UpdateRelativeLeaderboardScore(string leaderboardId, int value)
		{
			if(leaderboardEntryKills == -1)
			{
				Debug.LogError("Failed to update leaderboard score - leaderboardEntryKills not already updated");
				return;
			}

			Debug.Log("UpdateRelativeLeaderboardScore " + leaderboardEntryKills + " + " + value);

			leaderboardEntryKills += value;

			UpdateLeaderboardScore(leaderboardId, leaderboardEntryKills);
		}

		public bool UpdateLeaderboardScore(string leaderboardId, int value)
		{
			if(!_initialized)
				return false;
			
			SteamLeaderboard_t leaderboardInstance;
		
			if(!leaderboardInstances.TryGetValue(leaderboardId, out leaderboardInstance))
			{
				Debug.LogError("Failed to update leaderboard score - leaderboard not found in cache " + leaderboardId);
				return false;
			}

			UpdateLeaderboardScore(leaderboardInstance, value);

			return true;
		}

		public void UpdateLeaderboardScore(SteamLeaderboard_t steamLeaderboard, int value)
		{
			if(!_initialized)
				return;
			
			SteamAPICall_t handle = SteamUserStats.UploadLeaderboardScore(steamLeaderboard, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate, (int)value, null, 0);
			LeaderboardScoreUploaded.Set(handle);
		}

		private void OnLeaderboardScoreUploaded(LeaderboardScoreUploaded_t pCallback, bool bIOFailure) 
		{
			Debug.Log("[" + LeaderboardScoreUploaded_t.k_iCallback + " - LeaderboardScoreUploaded] - " + pCallback.m_bSuccess + " -- " + pCallback.m_hSteamLeaderboard + " -- " + pCallback.m_nScore + " -- " + pCallback.m_bScoreChanged + " -- " + pCallback.m_nGlobalRankNew + " -- " + pCallback.m_nGlobalRankPrevious);
		
			Debug.Log("Leaderboard score uploaded...");
		}

		#endregion

		#region Stats

		private Action requestStatsCallback;

		private bool RequestStats(Action requestStatsCallback)
		{
			if(!_initialized)
				return false;
			
			this.requestStatsCallback = requestStatsCallback;

			return SteamUserStats.RequestCurrentStats();
		}

		public bool StoreStats()
		{
			if(stats == null)
				return false;

			Debug.Log("Store stats");

			return StoreStats(() => { stats.StoreStats(true); });
		}

		public bool StoreStats(Action storedStatsCallback)
		{
			if(!_initialized)
				return false;
			
			if(storedStatsCallback != null)
				storedStatsCallback();

			return SteamUserStats.StoreStats();
		}

		private void OnUserStatsReceived(UserStatsReceived_t callback) 
		{
			// we may get callbacks for other games' stats arriving, ignore them
			if ((ulong)SteamUtils.GetAppID() == callback.m_nGameID) 
			{
				if (EResult.k_EResultOK == callback.m_eResult) 
				{
					Debug.Log("Received stats and achievements from Steam");

					if(requestStatsCallback != null)
					{
						requestStatsCallback();
						requestStatsCallback = null;
					}
				}
				else 
				{
					Debug.Log("RequestStats - failed, " + callback.m_eResult);
				}
			}
		}

		private void OnUserStatsStored(UserStatsStored_t callback) 
		{
			ulong gameID = (ulong)SteamUtils.GetAppID();

			// we may get callbacks for other games' stats arriving, ignore them
			if (gameID == callback.m_nGameID) 
			{
				if (EResult.k_EResultOK == callback.m_eResult) 
				{
					Debug.Log("StoreStats - stored");
				}
				else if (EResult.k_EResultInvalidParam == callback.m_eResult) 
				{
					// One or more stats we set broke a constraint. They've been reverted,
					// and we should re-iterate the values now to keep in sync.
					Debug.LogWarning("StoreStats - some failed to validate");
					// Fake up a callback here so that we re-load the values.

					UserStatsReceived_t cb = new UserStatsReceived_t();
					cb.m_eResult = EResult.k_EResultOK;
					cb.m_nGameID = gameID;
					OnUserStatsReceived(cb);
				}
				else 
				{
					Debug.Log("StoreStats - failed, " + callback.m_eResult);
				}
			}
		}

		#endregion

		#region Friends 

		private const string inviteServerNameKey = "SW_InviteServerName";

		public void TryInviteFriend(string serverName)
		{
			if(!_initialized)
				return;
			
			SteamFriends.ActivateGameOverlay("LobbyInvite");

			//TODO: tady musi bejt search lobby? / create

			//SteamMatchmaking.SetLobbyData(
		}

		private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback) 
		{
			string serverName = SteamMatchmaking.GetLobbyData(callback.m_steamIDLobby, inviteServerNameKey);

			Debug.Log("OnGameLobbyJoinRequested " + callback.m_steamIDLobby + " - " + serverName);

			if(!string.IsNullOrEmpty(serverName))
			{
				//TODO: serverName join - Steam invite
			}
		}

		#endregion

		// stub

		private static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
		{
			Debug.LogWarning(pchDebugText);
		}
	}
}

#endif