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

namespace GMReloaded.Bonuses
{
	[Serializable]
	public class Bonus
	{
		public enum Type
		{
			Healing,
			Passive,
			Danger,
			XP,
			Active,
			Special
		}

		public enum Behaviour : int
		{
			None,
			//aktivní bonusy
			Speed,                      //(zvýšení rychlosti hráče na 20s)  *testovano*
			GlobalSlowdown,            	//všichni hráči budou na určitou dobu zpomaleni kromě toho, který abilitu aktivoval -- chybuje
			//TimeSpeedUp,              	//Celá hra se na chvíli zrychlí *testovano*
			Teleport,               	//(Hráči jsou náhodně přehozeni, tam kde byl H1 je nyní H3 kde byl H3 je H2, tam kde byl H2 je H1) *testovano*
			Detonation,                 //Všechny položený miny v aréně bouchnou *testovano*
			Shield,                     //Po 15s bude hraci ubirat 1/2 damage *testovano*
			Ghost,						//Hrac se stane na 15 pruhlednym *testovano*
			MoreGrenadeDamage,  		//Větší Dmg (Dodá granátu vetší sílu, *3)
			GrenadeMasking,      	//Maskování granátu (gránát bude neviditelnej, platí na 3 granáty*2)

			//pasivni bonusy
			GrenadePlus,     	//Zvýšení poctu granátù (cooldown použití,*3)  
			Magnet,               	//Magnet (Hrác se na chvíli zasekne na míste
			HPPack,              	//Balíky s HP
			PointsPack,          	//přidá 20 bodů při sebrání bonusu, 4 balíky v jednom levelu
			//BoxPack
			TopSecret,
			HolyExplosions,
			GunUpgrade,
			QuickExplodes
		};

		public Bonus(Bonus.Behaviour bonusBehaviour)
		{
			CreateBonus(bonusBehaviour);
		}

		public Type type;

		public Behaviour behaviour;

		public bool hasIconIndicator;

		public bool usedRemote = false;

		public double usedTimestamp = -1f;

		public bool isIngameBonus = true;

		// neni null jenom v pripade, ze byl pouzitej a je to aktivni bonus
		public HUDActiveBonusStack.BonusStackItem activeBonusStackItem { get; set; }

		public int activeBonusStackItemId = -1;

		public RobotEmil pickerRobot;

		public GameMessage.MsgType bonusUseMessageType { get; private set; }

		public GameMessage.MsgType bonusPickUpMessageType { get; private set; }

		//

		private string _name;
		public string name
		{
			get
			{ 
				if(_name == null)
					_name = Localization.Instance.GetValue("Bonus_" + behaviour);

				return _name;
			}
		}

		private string _message;
		public string message
		{
			get
			{ 
				if(_message == null)
					_message = Localization.Instance.GetValue("Bonus_" + behaviour + "_Use");

				return _message;
			}
		}

		private bool HasIconIndicator(Behaviour behaviour)
		{
			return behaviour == Behaviour.GrenadeMasking || behaviour == Behaviour.GrenadePlus || behaviour == Behaviour.Magnet || behaviour == Behaviour.MoreGrenadeDamage;
		}

		private void CreateBonus(Bonus.Behaviour behaviour)
		{
			this.behaviour = behaviour;
			this.type = GetBonusType(behaviour);

			//

			this.bonusPickUpMessageType = GetBonusPickUpMessageType(this.behaviour);
			this.bonusUseMessageType = GetBonusUseMessageType(this.behaviour);

			//

			this.hasIconIndicator = HasIconIndicator(this.behaviour);
		}

		public static Type GetBonusType(Bonus.Behaviour behaviour)
		{
			switch(behaviour)
			{
				default:
				case Behaviour.None:
				case Behaviour.GlobalSlowdown:
				case Behaviour.Teleport:
				case Behaviour.Detonation:
				case Behaviour.Shield:
				case Behaviour.Ghost:
				case Behaviour.GrenadeMasking:
					return Type.Active;

				//case Behaviour.TimeSpeedUp:
				case Behaviour.GrenadePlus:
				case Behaviour.Speed:
				case Behaviour.QuickExplodes:
				case Behaviour.MoreGrenadeDamage:
					return Type.Passive;
					
				case Behaviour.Magnet:
					return Type.Danger;
					
				case Behaviour.HPPack:
					return Type.Healing;

				case Behaviour.PointsPack:
					return Type.XP;

				case Behaviour.TopSecret:
				case Behaviour.HolyExplosions:
				case Behaviour.GunUpgrade:
					return Type.Special;
			}
		}

		private GameMessage.MsgType GetBonusPickUpMessageType(Behaviour behaviour)
		{
			switch(behaviour)
			{
				case Behaviour.HPPack:
				case Behaviour.Magnet:
				case Behaviour.PointsPack:
				//
				case Behaviour.TopSecret:
				return GameMessage.MsgType.None; 

				default:
				case Behaviour.GrenadePlus:
				case Behaviour.QuickExplodes:
				case Behaviour.MoreGrenadeDamage:
				//case Behaviour.TimeSpeedUp:
				case Behaviour.Detonation:
				case Behaviour.Ghost:
				case Behaviour.Speed:
				case Behaviour.GlobalSlowdown:
				case Behaviour.Shield:
				case Behaviour.GrenadeMasking:
				case Behaviour.Teleport:
				case Behaviour.GunUpgrade:
				case Behaviour.HolyExplosions:
				return GameMessage.MsgType.BottomSmallNotice;
			}
		}

		private GameMessage.MsgType GetBonusUseMessageType(Behaviour behaviour)
		{
			switch(behaviour)
			{
				case Behaviour.HPPack:
				case Behaviour.Detonation:
				case Behaviour.Magnet:
				return GameMessage.MsgType.MiddleBigNotice; 

				default:
				case Behaviour.Ghost:
				case Behaviour.Speed:
				case Behaviour.GlobalSlowdown:
				//case Behaviour.TimeSpeedUp:
				case Behaviour.Shield:
				case Behaviour.GrenadePlus:
				case Behaviour.GrenadeMasking:
				case Behaviour.MoreGrenadeDamage:
				case Behaviour.QuickExplodes:
				return GameMessage.MsgType.BottomSmallNotice; 

				case Behaviour.PointsPack:
				return GameMessage.MsgType.TopSmallNotice;


				case Behaviour.Teleport:
				return GMReloaded.GameMessage.MsgType.None;
			}
		}

		#region Events

		public void OnBonusDispatchStart()
		{
			if(pickerRobot != null)
			{
				pickerRobot.OnBonusUsageStarted(this);
			}

			if(activeBonusStackItem != null)
				activeBonusStackItem.Pulse();
		}

		public void OnBonusDispatching(int remainingTime)
		{
			if(activeBonusStackItem != null)
				activeBonusStackItem.SetBonusRemainingTime(remainingTime);
		}

		public void OnBonusDispatched()
		{
			Debug.LogWarning("OnBonusDispatched " + activeBonusStackItemId + " - " + pickerRobot);

			if(activeBonusStackItem != null)
				activeBonusStackItem.StopPulsing();
			
			if(pickerRobot != null)
			{
				pickerRobot.OnBonusUsageComplete(this);
			}
		}

		#endregion

		public void SetIsIngameBonus(bool isIngameBonus)
		{
			this.isIngameBonus = isIngameBonus;
		}

	}
	
}
