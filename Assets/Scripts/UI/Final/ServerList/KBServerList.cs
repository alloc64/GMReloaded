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
#if UNITY_XBOXONE
using GMReloaded.Xbox.Linq;
#else
using System.Linq;
#endif
using System.Collections.Generic;

namespace GMReloaded.UI.Final.ServerList
{
	public class KBServerList : KBFocusableSuccessorsGUIPUN
	{
		[SerializeField]
		private KBServerListItem serverListItemTemplate;

		[SerializeField]
		private tk2dUIScrollableArea serverListScrollableArea;

		[SerializeField]
		private Transform serverListItemsContainer;

		[SerializeField]
		private KBFocusableSuccessors serverListFocusableSuccessors;

		[SerializeField]
		private tk2dTextMesh playersIndicatorText;


		//

		[SerializeField]
		private tk2dTextMesh arenaText;

		[SerializeField]
		private tk2dTextMesh timeLeftText;

		[SerializeField]
		private tk2dTextMesh playersText;

		//

		private Timer serverListUpdateTimer = new Timer();

		//

		private KBServerListItem selectedServerListItem = null;

		//

		#region Unity

		protected override void Awake()
		{
			base.Awake();

			Assert.IsAssigned(playersIndicatorText);
			Assert.IsAssigned(serverListScrollableArea);

			LocalClientRobotEmil.user.UpdateNick();
			GlobalStateController.GetInstance();

			UpdateServerList(true);
		}

		protected override void Update()
		{
			base.Update();

			playersIndicatorText.text = PhotonNetwork.countOfPlayers + " PLAYERS ONLINE";

			serverListUpdateTimer.Delay(4f, () => UpdateServerList(false));
		}

		#endregion

		#region Events

		public override void OnClick(KBFocusableGUIItem guiItem)
		{
			base.OnClick(guiItem);

			switch(guiItem.name)
			{
				case "Connect":

					GoNext();

				break;
			}
		}

		protected override void OnFocused(KBFocusableGUIItem guiItem)
		{
			base.OnFocused(guiItem);

			var serverListItem = guiItem as KBServerListItem;

			if(serverListItem != null)
			{
				//TODO: timeleft

				if(arenaText != null)
					arenaText.text = serverListItem.roomName;

				if(timeLeftText != null)
					timeLeftText.text = "Time left: N/A";

				if(playersText != null)
					playersText.text = localization.GetValue("ServerList_PlayersCount", serverListItem.playerCountInfo);
				
				if(menuRenderer != null)
				{
					CreateGame.KBGameRoom gameRoom = new CreateGame.KBGameRoom();

					gameRoom.roomName = serverListItem.roomName;
					gameRoom.arenaId = serverListItem.arenaId;
					gameRoom.roundTime = Config.Arenas.roundTimeDefault;
					gameRoom.botCount = Config.Bots.defaultBotCount;

					menuRenderer.gameRoom = gameRoom;
				}

				this.selectedServerListItem = serverListItem;
			}
			else
			{
				// ostatni
			}
		}

		#endregion

		//

		private PrefabsRecyclerBase<KBFocusableGUIItem> _serverListItemsRecycler;
		private PrefabsRecyclerBase<KBFocusableGUIItem> serverListItemsRecycler
		{
			get
			{
				if(_serverListItemsRecycler == null)
				{
					_serverListItemsRecycler = new PrefabsRecyclerBase<KBFocusableGUIItem>(serverListItemTemplate, serverListItemsContainer);
					_serverListItemsRecycler.Preinstantiate(40);
				}

				return _serverListItemsRecycler;
			}
		}

		//

		private Dictionary<string, KBServerListItem> serverListItems = new Dictionary<string, KBServerListItem>();
		private HashSet<RoomInfo> roomListCache = new HashSet<RoomInfo>();

		private Dictionary<string, KBServerListItem> modifiedServerListItems = new Dictionary<string, KBServerListItem>();

		//

		private void UpdateServerList(bool focusFirstItem, bool forceNotifyDatasetChanged = false)
		{
			var roomList = GetRoomList();

			bool notifyDatasetChanged = false;

			modifiedServerListItems.Clear();

			foreach(RoomInfo roomListItem in roomList)
			{
				KBServerListItem serverListItem = null;

				string key = roomListItem.name;

				serverListItems.TryGetValue(key, out serverListItem);

				if(serverListItem != null)
				{
					serverListItem.Setup(roomListItem);
				}
				else
				{
					serverListItem = serverListItemsRecycler.Dequeue() as KBServerListItem;

					serverListItem.ResetTransforms();
					serverListItem.Setup(roomListItem);

					serverListItems[key] = serverListItem;

					notifyDatasetChanged = true;
				}

				if(serverListItem != null)
					modifiedServerListItems[key] = serverListItem;
			}

			foreach(var kvp in serverListItems)
			{
				KBServerListItem modifiedServerListItem = null;
				modifiedServerListItems.TryGetValue(kvp.Key, out modifiedServerListItem);

				var serverListItem = kvp.Value;

				if(modifiedServerListItem == null && serverListItem != null)
				{
					serverListItemsRecycler.Enqueue(serverListItem);
				}
			}

			serverListItems.Clear();

			foreach(var kvp in modifiedServerListItems)
			{
				serverListItems[kvp.Key] = kvp.Value;
			}

			if(notifyDatasetChanged)
			{
				if(serverListFocusableSuccessors != null)
					serverListFocusableSuccessors.FocusableItems.Clear();
			}

			//

			float serverListRowMargin = -0.09f;
			float serverListRowOffset = 0f;

			int i = 0;
			#if UNITY_XBOXONE
			foreach(var kvp in serverListItems.ServerList_OrderBy_AOT((r0, r1) => r0.Value.playerCount.CompareTo(r1.Value.playerCount)))
			#else
			foreach(var kvp in serverListItems.OrderByDescending((r) => r.Value.playerCount))
			#endif
			{
				var r = kvp.Value;

				if(r != null)
				{
					if(notifyDatasetChanged)
					{
						serverListFocusableSuccessors.RegisterFocusableItem(r);
					}

					r.SetLocalPositionY(i * serverListRowMargin);
					i++;

					serverListRowOffset += serverListRowMargin;
				}
			}

			//

			if(serverListScrollableArea != null)
				serverListScrollableArea.ContentLength = Mathf.Abs(serverListRowOffset);

			if(notifyDatasetChanged || forceNotifyDatasetChanged)
				NotifyFocusableItemsDatasetChanged(focusFirstItem);
		}

		private HashSet<RoomInfo> GetRoomList()
		{
			roomListCache.Clear();

			var liveRoomList = PhotonNetwork.GetRoomList();

			if(Config.Servers.premadeServerRooms.Length > 0)
			{
				foreach(Config.Servers.ServerConfig serverConfig in Config.Servers.premadeServerRooms)
				{
					var cfg = Config.Arenas.GetConfig(serverConfig.levelId);

					if(cfg == null)
						continue;
					
					ExitGames.Client.Photon.Hashtable propertiesToCache = new ExitGames.Client.Photon.Hashtable();

					propertiesToCache[ExitGames.Client.Photon.GamePropertyKey.MaxPlayers] = (byte)cfg.roomOptions.maxPlayers;
					propertiesToCache[ExitGames.Client.Photon.GamePropertyKey.PlayerCount] = (byte)0;
					propertiesToCache[ArenaEventDispatcher.PropsConstants.LevelKey] = serverConfig.levelId;

					RoomInfo premadeItem = new RoomInfo(serverConfig.name, propertiesToCache);

					bool itemFound = false;
					foreach(var roomListItem in liveRoomList)
					{
						if(roomListItem.name == premadeItem.name)
						{
							itemFound = true;
							break;
						}
					}

					if(!itemFound)
					{
						roomListCache.Add(premadeItem);
					}
				}
			}

			foreach(RoomInfo roomInfo in liveRoomList)
			{
				if(roomInfo != null && !string.IsNullOrEmpty(roomInfo.GetLevelId()) && !roomInfo.name.Contains(Config.tutorial.tutorialRoomNamePrefix)) 
				{
					roomListCache.Add(roomInfo);
				}
			}

			return roomListCache;
		}

		public override void Show(object bundle)
		{
			base.Show(bundle);
			UpdateServerList(true, true);
		}

		public override void GoNext()
		{
			base.GoNext();

			if(selectedServerListItem == null)
			{
				SetError("Select server to connect...");
			}
			else
			{
				menuRenderer.SetState(this, KBMenuRenderer.State.Equip);
			}
		}
	}
}
