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
using System.Linq;

namespace GMReloaded.UI.Final
{
	public class ChatConsole : MonoBehaviourTO
	{
		private KBMenuRenderer menuRenderer { get { return KBMenuRenderer.Instance; }}

		private ArenaEventDispatcher arenaEventDispatcher { get { return ArenaEventDispatcher.Instance; } }

		private PrefabsRecyclerBase<ChatMessage> recycler;

		[SerializeField]
		private GameObject inputGO; 

		[SerializeField]
		private ChatMessage localPlayerMessage;

		[SerializeField]
		private Transform chatMessagesContainer;

		private int maxMessageLength = 56;

		[SerializeField]
		private int maxMessages = 5;

		private float rowMargin = 0.05f;

		private float rowOffset = 0f;

		private string localPlayerNick;

		private string _message = "";
		public string message
		{
			get { return _message; }
			set
			{
				_message = value;

				if (_message.Length > maxMessageLength)
					_message = _message.Substring(0, maxMessageLength);

				if(localPlayerMessage != null)
				{
					localPlayerMessage.SetText(_message, 0.5f);
				}
			}
		}

		private Queue<ChatMessage> messageStack;

		private void Awake()
		{
			messageStack = new Queue<ChatMessage>();

			recycler = new PrefabsRecyclerBase<ChatMessage>(localPlayerMessage, chatMessagesContainer);
			recycler.Preinstantiate(maxMessages);
		}

		private bool _isChatActive = false;
		private bool isChatActive
		{
			get { return _isChatActive; }
			set { _isChatActive = value; if(menuRenderer != null) menuRenderer.isChatting = _isChatActive; if(inputGO != null) inputGO.SetActive(_isChatActive); }
		}

		public void Show()
		{
			if(isChatActive)
				return;

			message = "";

			Timer.DelayAsync(0.005f, () => tk2dUIManager.Instance.OnInputUpdate += ListenForKeyboardTextUpdate);

			isChatActive = true;
		}

		public void Hide()
		{
			if(!isChatActive)
				return;

			isChatActive = false;
			tk2dUIManager.Instance.OnInputUpdate -= ListenForKeyboardTextUpdate;

			message = "";
		}

		private void OnSubmit()
		{
			if(!isChatActive)
				return;

			SubmitMessage(message);

			Hide();
		}

		public void SubmitMessage(string message)
		{
			if(message.Length > 0)
			{
				arenaEventDispatcher.SubmitChatMessage(message);
			}
		}

		public void OnChatMessageReceived(string text)
		{
			var msg = recycler.Dequeue();

			if(msg != null)
			{
				msg.ResetTransforms();
				msg.SetText(text);

				messageStack.Enqueue(msg);

				if(messageStack.Count > maxMessages)
				{
					var recycledMsg = messageStack.Dequeue();

					if(recycledMsg != null)
						recycler.Enqueue(recycledMsg);
				}

				UpdateMessagesOffsets();
			}
		}

		private void UpdateMessagesOffsets()
		{
			rowOffset = 0f;

			foreach(var msg in messageStack.Reverse())
			{
				msg.SetLocalPositonY(rowOffset);
				rowOffset += rowMargin;
			}
		}

		private void ListenForKeyboardTextUpdate()
		{
			bool change = false;
			string newText = _message;

			string inputStr = Input.inputString;
			char c;
			for (int i=0; i<inputStr.Length; i++)
			{
				c = inputStr[i];
				if (c == "\b"[0])
				{
					if (_message.Length != 0)
					{
						newText = _message.Substring(0, _message.Length - 1);
						change = true;
					}
				}
				else if (c == "\n"[0] || c == "\r"[0])
				{
					OnSubmit();
				}
				else if ((int)c!=9 && (int)c!=27 && newText.Length < maxMessageLength) //deal with a Mac only Unity bug where it returns a char for escape and tab
				{
					newText += c;
					change = true;
				}
			}

			if(change)
			{
				message = newText;
			}
		}
	}
}
