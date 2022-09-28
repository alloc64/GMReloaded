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

namespace GMReloaded.Bonuses.Active
{
	public abstract class IBonusActiveDispatch : IBonusDispatchable
	{
		public enum State
		{
			Idle,
			Dispatching,
			Completed
		}

		public override bool Active 
		{
			get { return state == State.Dispatching; }
		}

		public override bool Permanent { get { return false; } }

		//

		protected State state;

		protected Bonus bonus;

		protected RobotEmilNetworked robotParent;

		protected float timer = 0f;
		protected float delay = 0f;

		private bool useSound = true;

		protected BonusSoundController bonusSoundController { get { return BonusController.Instance != null ? BonusController.Instance.bonusSoundController : null; } }

		protected Madness.MadnessModeController madnessMode { get { return ArenaEventDispatcher.Instance == null ? null : ArenaEventDispatcher.Instance.madnessMode; } }

		private string tickingSoundId;

		protected virtual void Update(float dt) 
		{
			timer += dt;

			if(timer >= delay)
			{
				OnDispatched();
				timer = 0f;
			}

			if(bonus != null)
				bonus.OnBonusDispatching((int)(delay - timer));
		}

		public override bool Dispatch(Bonus bonus, RobotEmilNetworked robotParent, bool permanent)
		{
			this.bonus = bonus;
			this.robotParent = robotParent;

			this.bonus.OnBonusDispatchStart();

			OnPlayBonusSound();

			double usedTimestamp = bonus.usedTimestamp;

			if(usedTimestamp > 0)
			{
				float diff = (float)(PhotonNetwork.time - usedTimestamp);

				delay -= diff;

				if(delay < 0f)
					delay = 0f;
				
				Debug.Log("setting dispatch diff / delay " + diff + " / " + delay);
			}
			
			SetState(State.Dispatching);
			Independent.Coroutine.Instance.ProcessCoroutine(UpdateCoroutine());

			return true;
		}

		protected virtual void OnPlayBonusSound()
		{
			if(!useSound)
				return;

			if(tickingSoundId == null)
				SetTickingSound(Config.Sounds.bonusTicking);

			bonusSoundController.Play(tickingSoundId, robotParent.transform);
		}

		protected virtual void OnStopBonusSound()
		{
			if(!useSound)
				return;

			bonusSoundController.Stop(tickingSoundId);
		}

		protected virtual void SetTickingSound(string soundID)
		{
			tickingSoundId = soundID;
		}

		protected virtual void SetUseSound(bool use)
		{
			useSound = use;
		}

		private IEnumerator UpdateCoroutine()
		{
			while(state == State.Dispatching)
			{
				Update(Time.deltaTime);
				yield return null;
			}
		}

		protected void SetState(State state)
		{
			this.state = state;
		}

		protected void SetDispatchDelay(float delay)
		{
			this.delay = delay;
			this.timer = 0f;
			
			SetState(State.Idle);
		}

		public virtual void OnDispatched()
		{
			SetState(State.Completed);

			OnStopBonusSound();
			
			if(bonus != null)
				bonus.OnBonusDispatched();
		}
	}
	
}
