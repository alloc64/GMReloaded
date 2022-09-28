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
using Independent;
using System.Collections.Generic;
using TouchOrchestra;
using GMReloaded.Bonuses;

namespace GMReloaded
{
	public class GameMessage : MonoBehaviourTO
	{
		public enum MsgType
		{
			TopSmallNotice,
			MiddleBigNotice,
			BottomSmallNotice,

			Achievement,
			Mission,
			Help,

			None
		}

		public struct Message
		{
			public string title;
			public string description;

			//

			public MsgType type;

			//

			public Message(string title, MsgType type)
			{
				this.title = title;
				this.type = type;
				this.description = null;
			}

			public Message(string title, string description, MsgType type) : this(title, type)
			{
				this.description = description;
			}
		}

		[SerializeField]
		private tk2dTextMeshAnimationAdapter infoTopSmallText;

		[SerializeField]
		private tk2dTextMeshAnimationAdapter infoMiddleBigText;

		[SerializeField]
		private tk2dTextMeshAnimationAdapter infoBottomSmallText;

		[SerializeField]
		private Infobox infoBottomBox;

		#region Public functions shortcuts

		public void SetXPMessage(int exp)
		{
			if(exp > 0)
				SetMessage((exp < 0 ? "" : "+") + exp + "EXP", GameMessage.MsgType.TopSmallNotice);
		}

		public void SetBonusPickedUp(Bonus bonus)
		{
			if(bonus == null || bonus.bonusPickUpMessageType == MsgType.None)
				return;

			SetMessage(bonus.name, bonus.bonusPickUpMessageType);
		}
		public void SetMessage(Config.MadnessMode.MadnessStep step)
		{
			SetMessageLocalized(step.name, MsgType.BottomSmallNotice);
		}

		public void SetMessage(string text, MsgType type)
		{
			ShowTextByMessageType(new Message(text, type));	
		}

		public void SetMessageLocalized(string locId, MsgType type, params object[] p)
		{
			ShowTextByMessageType(new Message(localization.GetValue(locId, p), type));	
		}

		//

		public void SetInfoBox(string title, string description, MsgType type)
		{
			switch(type)
			{
				case MsgType.Achievement:
				case MsgType.Mission:
				case MsgType.Help:
					ShowInfoBox(new Message(title, description, type));
				break;
					
				default:
					Debug.LogError("SetInfoBox - invalid type" + type);
				break;
			}
		}

		public void SetMissionInfoBox(Achievements.Mission mission)
		{
			SetInfoBox(localization.GetValue(mission.key), localization.GetValue(mission.key + "_Desc"), mission.isAchievement ? GameMessage.MsgType.Achievement : GameMessage.MsgType.Mission);
		}

		#endregion

		#region Bonus State Changes

		public void SetBonusUseStarted(Bonus bonus)
		{
			if(bonus == null || bonus.bonusUseMessageType == MsgType.None)
				return;

			snd.speech.PlayBonus(bonus.behaviour);

			SetMessage(bonus.message, bonus.bonusUseMessageType);
		}

		public void SetBonusUsageEnded(Bonus.Behaviour bonusBehaviour)
		{
			snd.speech.PlayBonus(bonusBehaviour);

			SetMessageLocalized("Bonus_" + bonusBehaviour + "_Used", GameMessage.MsgType.BottomSmallNotice);
		}

		#endregion

		private void ShowTextByMessageType(Message msg)
		{
			switch(msg.type)
			{
				case MsgType.TopSmallNotice:
					ShowText(msg, infoTopSmallText);
				break;

				case MsgType.MiddleBigNotice:
					ShowText(msg, infoMiddleBigText);
				break;
					
				case MsgType.BottomSmallNotice:
					ShowText(msg, infoBottomSmallText);
				break;

				default:
					Debug.LogError("ShowTextByMessageType - invalid msg.type" + msg.type);
				break;
			}
		}

		private void ShowText(Message msg, tk2dTextMeshAnimationAdapter textMesh)
		{
			if(textMesh != null)
			{
				textMesh.SetText(msg.title);

				var anim = textMesh.GetComponent<Animation>();

				if(anim != null)
					anim.Play(() => {});
			}
		}

		private void ShowInfoBox(Message msg)
		{
			if(infoBottomBox == null)
				return;

			infoBottomBox.SetMessage(msg);
		}

		//

		public void Flush()
		{
		}
	}
}
