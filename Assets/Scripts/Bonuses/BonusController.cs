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

namespace GMReloaded.Bonuses
{
	public abstract class IBonusDispatchable
	{
		public abstract bool Active { get; }

		public abstract bool Permanent { get; }

		public abstract bool Dispatch(Bonus bonus, RobotEmilNetworked robotParent, bool permanent);
	}

	public class BonusImplementationsDispatcher
	{
		private Dictionary<Bonus.Behaviour, IBonusDispatchable> bonusImpls = new Dictionary<Bonus.Behaviour, IBonusDispatchable>();
		
		public BonusImplementationsDispatcher()
		{
			foreach(Bonus.Behaviour behaviour in System.Enum.GetValues(typeof(Bonus.Behaviour)))
			{
				IBonusDispatchable dispatchImpl = null;

				switch(behaviour)
				{
					case Bonus.Behaviour.Speed:
						dispatchImpl = new Pasive.SpeedBonusImpl();
					break;

					case Bonus.Behaviour.GlobalSlowdown:
						dispatchImpl = new Active.GlobalSlowdownBonusImpl();
					break;

					//case Bonus.Behaviour.TimeSpeedUp:
					//	dispatchImpl = new Pasive.TimeSpeedUpBonusImpl();
					//break;

					case Bonus.Behaviour.Ghost:
						dispatchImpl = new Active.GhostBonusImpl();
					break;

					case Bonus.Behaviour.Shield:
						dispatchImpl = new Active.ShieldBonusImpl();
					break;

					case Bonus.Behaviour.GrenadePlus:
						dispatchImpl = new Pasive.GrenadePlusImpl();
					break;

					case Bonus.Behaviour.Magnet:
						dispatchImpl = new Active.MagnetBonusImpl();
					break;

					case Bonus.Behaviour.MoreGrenadeDamage:
						dispatchImpl = new Pasive.MoreGrenadeDamageBonusImpl();
					break;
						
					case Bonus.Behaviour.QuickExplodes:
						dispatchImpl = new Pasive.QuickExplodesBonusImpl();
					break;

					case Bonus.Behaviour.GrenadeMasking:
						dispatchImpl = new Active.GrenadeMaskingBonusImpl();
					break;

					case Bonus.Behaviour.HolyExplosions:
						dispatchImpl = new Pasive.HolyExplosionsBonusImpl();
					break;

					case Bonus.Behaviour.GunUpgrade:
						dispatchImpl = new Pasive.GunUpgradeBonusImpl();
					break;
				}

				bonusImpls[behaviour] = dispatchImpl;
			}
		}

		public bool UseActiveOrPassiveBonus(Bonus bonus, RobotEmilNetworked parentRobot, bool permanent)
		{
			if(bonus == null)
				return false;

			IBonusDispatchable bonusDispatch = null;

			bonusImpls.TryGetValue(bonus.behaviour, out bonusDispatch);

			if(bonusDispatch != null && !bonusDispatch.Active)
			{
				return bonusDispatch.Dispatch(bonus, parentRobot, permanent);
			}
			else if(bonusDispatch == null)
			{
				//toto by se nemělo stávat, jenom v připadě, že bonus chybí fallback

				bonus.OnBonusDispatchStart();
				bonus.OnBonusDispatched();

				Debug.LogError("Implementation of bonus " + bonus.behaviour + " not found!");
			}

			return false;
		}

		public void StopUsedActiveBonusesDispatch()
		{
			foreach(var kvp in bonusImpls)
			{
				var bonus = kvp.Value;

				if(bonus is Active.IBonusActiveDispatch)
				{
					var activeBonus = bonus as Active.IBonusActiveDispatch;

					if(activeBonus.Active)
						activeBonus.OnDispatched();
				}
			}	
		}

		public void StopUsedPasiveBonusesDispatch()
		{
			foreach(var kvp in bonusImpls)
			{
				var bonus = kvp.Value as Pasive.IBonusPasiveDispatch;

				//

				bool noDeathPenaltyActive = false;

				//

				var aed = ArenaEventDispatcher.Instance;

				if(aed != null)
				{
					noDeathPenaltyActive = aed.madnessMode != null && aed.madnessMode.isNoDeathPenaltyActive;
				}

				if(bonus != null && !bonus.Permanent && !noDeathPenaltyActive)
				{
					bonus.StopDispatch();
				}
			}
		}

		//

		public void ResetUsedPasiveBonuses()
		{
			foreach(var kvp in bonusImpls)
			{
				var bonus = kvp.Value;

				if(bonus is Pasive.IBonusPasiveDispatch)
				{
					var pasiveBonus = bonus as Pasive.IBonusPasiveDispatch;

					pasiveBonus.Reset();
				}
			}
		}
	}

	public class BonusController : MonoSingleton<BonusController> 
	{
		private Dictionary<RobotEmil, BonusImplementationsDispatcher> dispatchers = new Dictionary<RobotEmil, BonusImplementationsDispatcher>();

		public BonusSoundController bonusSoundController = new BonusSoundController();

		[SerializeField]
		private List<Texture> bonusIconTexList = new List<Texture>();

		private Dictionary<string, Texture> _bonusIconsCache;
		private Dictionary<string, Texture> bonusIconsCache
		{
			get
			{
				if(_bonusIconsCache == null)
				{
					_bonusIconsCache = new Dictionary<string, Texture>();

					foreach(var t in bonusIconTexList)
					{
						if(t != null)
						{
							_bonusIconsCache[t.name] = t;
						}
					}
				}

				return _bonusIconsCache;
			}
		}

		//

		public bool UseActiveOrPassiveBonus(Bonus.Behaviour bonusBehaviour, RobotEmilNetworked parentRobot, bool permanent)
		{
			return UseActiveOrPassiveBonus(new Bonus(bonusBehaviour), parentRobot, permanent);
		}

		public bool UseActiveOrPassiveBonus(Bonus bonus, RobotEmilNetworked parentRobot, bool permanent = false)
		{
			if(bonus == null)
				return false;

			BonusImplementationsDispatcher dispatcher = null;

			dispatchers.TryGetValue(parentRobot, out dispatcher);

			if(dispatcher == null)
			{
				dispatcher = new BonusImplementationsDispatcher();
				dispatchers[parentRobot] = dispatcher;
			}

			return dispatcher.UseActiveOrPassiveBonus(bonus, parentRobot, permanent);
		}

		public void UseOnetimeBonus(Bonus bonus)
		{
			if(bonus == null)
				return;
			
			bonus.OnBonusDispatchStart();
			bonus.OnBonusDispatched();
		}

		//

		public void StopBonusesDispatchAfterDeath(RobotEmilNetworked parentRobot)
		{
			BonusImplementationsDispatcher dispatcher = null;

			dispatchers.TryGetValue(parentRobot, out dispatcher);

			if(dispatcher != null)
			{
				dispatcher.StopUsedActiveBonusesDispatch();
				dispatcher.StopUsedPasiveBonusesDispatch();
			}
		}

		//

		public void ResetAllBonuses()
		{
			foreach(var kvp in dispatchers)
			{
				var dispatcher = kvp.Value;

				if(dispatcher != null)
				{
					dispatcher.StopUsedActiveBonusesDispatch();
					dispatcher.ResetUsedPasiveBonuses();
				}
			}
		}

		public Texture GetBonusIconTexture(Bonus.Behaviour bonusBehaviour)
		{
			Texture tex = null;

			bonusIconsCache.TryGetValue(bonusBehaviour.ToString(), out tex);

			return tex;
		}
	}
}
