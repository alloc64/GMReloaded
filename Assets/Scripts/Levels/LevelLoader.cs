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
using GMReloaded.UI;
using System.Collections;
using GMReloaded.UI.Final;


namespace GMReloaded
{
	public class LevelLoader : MonoSingletonPersistent<LevelLoader> 
	{
		private KBMenuRenderer menuRenderer { get { return KBMenuRenderer.IsNull ? null : KBMenuRenderer.Instance; } }

		private GlobalStateController gsc { get { return GlobalStateController.Instance; } }

		private bool loading = false;

		private void LoadAsync(string levelId, float afterLoadDelay = 0.5f)
		{
			if(loading)
				return;

			loading = true;

			OnLoadStarted();

			AsyncOperation op = Application.LoadLevelAsync(levelId);

			if(op == null)
			{
				Debug.Log("Error, level " + levelId + " couldn't be loaded!");
				return;
			}

			StartCoroutine(LoadLevelAsyncProgress(levelId, op, afterLoadDelay));
		}

		private IEnumerator LoadLevelAsyncProgress(string levelId, AsyncOperation op, float afterLoadDelay)
		{
			op.priority = 10;

			float progress = 0.0f;

			while(!op.isDone)
			{
				if(op.progress > progress)
					progress = op.progress;

				OnLoading(progress);

				yield return null;
			}

			OnLoading(1f);

			float pauseTime = Time.realtimeSinceStartup + afterLoadDelay;
			while(Time.realtimeSinceStartup < pauseTime)
				yield return null;
			

			Resources.UnloadUnusedAssets();

			OnLoaded(levelId);
		}

		protected virtual void OnLoadStarted()
		{
		}

		protected virtual void OnLoading(float _progress)
		{
			//loadingMenu.SetProgress(_progress);
		}

		public override void OnLoaded(string levelId)
		{
			Debug.Log("Level " + levelId + " loaded");

			StartCoroutine(DelayMsgQueueActivation());

			var aed = ArenaEventDispatcher.Instance;

			if(aed != null)
				aed.OnArenaLoaded();

			ProjectileRecycler.GetInstance();
			Localization.GetInstance();
			GlobalStateController.GetInstance();

			loading = false;

			gsc.isInGame = true;

			if(gsc.hasStoppedGameLoading)
			{
				if(PhotonNetwork.LeaveRoom())
					Timer.DelayAsyncIndependent(1f, LevelLoader.Instance.LoadServerListMenu);
				
				gsc.hasStoppedGameLoading = false;
			}
		}

		public void LoadArena(string arenaId, float delay = 0f)
		{
			gsc.isInGame = false;

			Debug.Log("LoadArena " + arenaId);

			PhotonNetwork.isMessageQueueRunning = false;

			if(delay <= 0f)
			{
				LoadAsync(arenaId);
			}
			else
			{
				Timer.DelayAsync(delay, () => LoadAsync(arenaId));
			}
		}

		public void LoadServerListMenu()
		{
			gsc.isInGame = false;

			if(menuRenderer != null)
				menuRenderer.GoBackToMainMenu();
			
			//Application.LoadLevel("ClearScene");
		}

		//

		private IEnumerator DelayMsgQueueActivation()
		{
			yield return new WaitForSeconds(2f);
			PhotonNetwork.isMessageQueueRunning = true;
		}
	}
}
