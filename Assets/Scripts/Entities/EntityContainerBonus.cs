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
using GMReloaded.Bonuses;

namespace GMReloaded.Entities
{
	[System.Serializable]
	public class EntityContainerBonus
	{
		//

		[SerializeField]
		private Bonus.Behaviour bonusBehaviour;

		private BonusPickable.State bonusState;

		//

		private BonusPickable bonusPickable = null;

		//

		private EntityContainer entityContainer;

		//

		public Bonus bonus { get; private set; }

		public bool canBonusBePickedUp { get { return bonus != null && bonus.behaviour != Bonus.Behaviour.None; } }

		//

		private float setupTimestamp;

		//

		protected DestroyableEntityController dec { get { return DestroyableEntityController.Instance; } }

		//

		public EntityContainerBonus(EntityContainer entityContainer)
		{
			this.entityContainer = entityContainer;
		}

		#region Entity Bonuses

		// tato metoda se vola jenom na masterovi z EntityContaineru po destroyi entity
		public bool GenerateBonus()
		{
			if(!PhotonNetwork.isMasterClient)
				return false;

			return GenerateBonus(dec.PeekBonusBehaviour());
		}

		public bool GenerateBonus(Bonus.Behaviour bonusBehaviour)
		{
			RecycleBonus();

			bonus = new Bonus(bonusBehaviour);
			bonusState = BonusPickable.State.Create;

			//Debug.Log(entityContainer.name + " - Generate bonus " + bonus.behaviour, entityContainer);

			// update probehne v EntityContaineru a entita se vytvori v HandleNetworkData

			return bonus.behaviour != Bonus.Behaviour.None;
		}

		private void SetBonusBehaviour(Bonus.Behaviour setBonusBehaviour, BonusPickable.State bonusState, bool networkRefresh)
		{
			var currBonusBehaviour = bonus == null ? Bonus.Behaviour.None : bonus.behaviour;

			if(bonusState == BonusPickable.State.Denied) // TODO: masterclient ma hodnoty jeste pred propagaci pres network tj currBonusBehaviour == setBonusBehaviour
			{
				return;
			}

			//Debug.Log(entityContainer.name + ": SetBonusBehaviour: " + setBonusBehaviour + " - " + currBonusBehaviour + " - " + bonusState + " - " + networkRefresh);

			switch(bonusState)
			{
				case BonusPickable.State.Undefined:
				break;
					
				case BonusPickable.State.Create:

					if(setBonusBehaviour != Bonus.Behaviour.None && bonusPickable == null)
					{
						RecycleBonus();

						bonus = new Bonus(setBonusBehaviour);

						bonusPickable = dec.DequeueBonusPickable(bonus.type);

						if(bonusPickable == null)
						{
							Debug.LogError("Failed to dequeue bonus pickable: " + setBonusBehaviour + " / bonusType: " + bonus.type);
							return;
						}

						bonusPickable.Setup(entityContainer, setBonusBehaviour);
						bonusPickable.Show();

						setupTimestamp = Time.realtimeSinceStartup;

						bonusState = BonusPickable.State.Created;
					}

				break;
					
				case BonusPickable.State.NormalPickUp:
				case BonusPickable.State.SilentPickUp:
				case BonusPickable.State.Destroyed:

					if(setBonusBehaviour == Bonus.Behaviour.None && currBonusBehaviour != Bonus.Behaviour.None && bonusPickable != null)
					{
						bonusState = BonusPickable.State.Undefined;
						bonus = null; // odstranim aktualni bonus, reycklace se provede callbackem

						bonusPickable.PickUpBonus(bonusState);
					}

				break;
			}

			if(networkRefresh && setBonusBehaviour != currBonusBehaviour)
			{
				if(entityContainer != null)
					entityContainer.RefreshNetworkState();
			}
		}

		public void PickUpBonus(BonusPickable.State pickupType, bool ignoreNetworkUpdate)
		{
			if(pickupType == BonusPickable.State.Denied)// && (setupTimestamp > 0 && (Time.realtimeSinceStartup - setupTimestamp) < 1.5f))
				return;

			this.bonusState = pickupType;

			SetBonusBehaviour(Bonus.Behaviour.None, pickupType, !ignoreNetworkUpdate);
		}

		public void RecycleBonus()
		{
			if(bonusPickable != null)
				dec.RecycleBonusPickable(bonusPickable);
			
			ResetVariables();
		}


		private void ResetVariables()
		{
			bonusState = BonusPickable.State.Undefined;
			bonusPickable = null;
			bonus = null;
		}

		#endregion

		#region Network

		public void RefreshNetworkState(GMReloaded.Entities.EntityContainer.NetworkProperties np)
		{
			np.bonusBehaviour = bonus == null ? Bonus.Behaviour.None : bonus.behaviour;
			np.bonusState = bonusState;
		}

		public void HandleNetworkData(GMReloaded.Entities.EntityContainer.NetworkProperties np)
		{
			SetBonusBehaviour(np.bonusBehaviour, np.bonusState, false);
		}

		#endregion
	}
}
