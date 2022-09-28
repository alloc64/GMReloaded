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
using ExitGames.Client.Photon;

namespace GMReloaded.Madness
{
	public class MadnessModeController
	{
		public const string networkPropsId = "MadnessProps";

		//

		private Dictionary<MadnessStepType, IMadnessModeStepDispatch> madnessStepImpls = new Dictionary<MadnessStepType, IMadnessModeStepDispatch>();

		private ArenaEventDispatcher arenaEventDispatcher { get { return ArenaEventDispatcher.Instance; } }

		private HUD hud { get { return HUD.IsNull ? null : HUD.Instance; } }

		private GameMessage gameMessage { get { return HUD.IsNull ? null : hud.gameMessage; } }

		private RoomPropertiesController rpc { get { return RoomPropertiesController.Instance; } }

		//

		public MadnessModeController()
		{
			madnessStepImpls[MadnessStepType.MagSize] = new MagSize_MadnessModeImpl();

			madnessStepImpls[MadnessStepType.MovementSpeed] = new MovementSpeed_MadnessModeImpl();

			madnessStepImpls[MadnessStepType.GrenadeRadiusExplosionMultiplier] = new GrenadeRadiusExplosionMultiplier_MadnessModeImpl();

			madnessStepImpls[MadnessStepType.GrenadeDamageMultiplier] = new GrenadeDamageMultiplier_MadnessModeImpl();

			madnessStepImpls[MadnessStepType.TimeSpeedUp] = new TimeSpeedUp_MadnessModeImpl();

			madnessStepImpls[MadnessStepType.Electrovision] = new Electrovision_MadnessModeImpl();

			//madnessStepImpls[MadnessStepType.HeavyRain] = new HeavyRain_MadnessModeImpl();

			madnessStepImpls[MadnessStepType.RocketLauncher] = new RocketLauncher_MadnessModeImpl();

			//madnessStepImpls[MadnessStepType.Artillery] = new Artillery_MadnessModeImpl();

			madnessStepImpls[MadnessStepType.Craaatesss] = new Craaatesss_MadnessModeImpl();

			madnessStepImpls[MadnessStepType.RemoveCrates] = new RemoveCrates_MadnessModeImpl();

			madnessStepImpls[MadnessStepType.BarrelMania] = new BarrelMania_MadnessModeImpl();

			madnessStepImpls[MadnessStepType.NoDeathPenalty] = new NoDeathPenalty_MadnessModeImpl();

			madnessStepImpls[MadnessStepType.BouncyBounce] = new BouncyBounce_MadnessModeImpl();

			madnessStepImpls[MadnessStepType.ExplosiveCrates] = new ExplosiveCrates_MadnessModeImpl();

			madnessStepImpls[MadnessStepType.StopCratesAutoRespawn] = new StopCratesAutoRespawnMadnessModeImpl();

			madnessStepImpls[MadnessStepType.Diarrhea] = new Diarrhea_MadnessModeImpl();

			madnessStepImpls[MadnessStepType.Silence] = new Silence_MadnessModeImpl();

			madnessStepImpls[MadnessStepType.HolyTime] = new HolyTime_MadnessModeImpl();
		}

		//

		public void SetMadnessMode(object obj)
		{
			if(obj == null || PhotonNetwork.isMasterClient)
				return;
			
			Config.madnessMode.SetMadnessSteps(MadnessModeStepsMPStruct.Deserialize((byte[])obj));

			#if UNITY_EDITOR
			Config.madnessMode.DumpMadnessSteps();
			#endif
		}

		#region Dispatch

		public void UpdateGameTime(int time)
		{
			UpdateMadnessStepPrepareForDispatch((time > 30) ? (time - Config.madnessMode.prepareForDispatchTime) : time);
			UpdateMadnessStepDispatch(time);
		}

		//

		private void UpdateMadnessStepPrepareForDispatch(int time)
		{
			Dictionary<MadnessStepType, Config.MadnessMode.MadnessStep> steps;

			if(!Config.madnessMode.steps.TryGetValue(time, out steps))
				return;

			foreach(var kvp in steps)
			{
				var step = kvp.Value;

				if(step == null || step.usedCount < 1)
					continue;

				PrepareForProcessMadnessStep(step);
			}
		}

		//

		private void UpdateMadnessStepDispatch(int time)
		{
			if(!PhotonNetwork.isMasterClient)
				return;

			Dictionary<MadnessStepType, Config.MadnessMode.MadnessStep> steps;

			if(!Config.madnessMode.steps.TryGetValue(time, out steps))
				return;

			foreach(var kvp in steps)
			{
				var step = kvp.Value;

				if(step == null || step.usedCount < 1)
					continue;

				ProcessMadnessStep(step, true);
			}
		}

		#endregion

		#region Processing

		public bool IsMadnessStepActive(MadnessStepType stepType)
		{
			IMadnessModeStepDispatch dispatcher = GetMadnessStepDispatcher(stepType);

			return dispatcher != null && dispatcher.isActive;
		}

		//

		public T GetMadnessStepDispatcher<T>(MadnessStepType stepType) where T : IMadnessModeStepDispatch
		{
			return GetMadnessStepDispatcher(stepType) as T;
		}

		private IMadnessModeStepDispatch GetMadnessStepDispatcher(MadnessStepType stepType)
		{
			IMadnessModeStepDispatch dispatcher = null;

			madnessStepImpls.TryGetValue(stepType, out dispatcher);

			if(dispatcher == null)
				Debug.LogError("Failed to retrieve IMadnessModeStepDispatch for stepType " + stepType + ", returnin null");

			return dispatcher;
		}

		//

		private void PrepareForProcessMadnessStep(Config.MadnessMode.MadnessStep step)
		{
			if(step == null)
				return;

			if(hud != null)
				hud.OnMadnessStepPrepareForDispatch(step, Config.madnessMode.prepareForDispatchTime+1); // +-1s to nesedelo
		}

		public void ProcessMadnessStep_Tutorial(Config.MadnessMode.MadnessStep step)
		{
			if(!Tutorial.TutorialManager.Instance.isActive)
				return;

			ProcessMadnessStep(step, false);
		}

		private void ProcessMadnessStep(Config.MadnessMode.MadnessStep step, bool sendRPC, double timestamp = -1)
		{
			if(step == null)
				return;

			IMadnessModeStepDispatch dispatcher = GetMadnessStepDispatcher(step.stepType);

			#if UNITY_EDITOR
			Debug.Log("ProcessMadnessStep stepType: " + step.stepType + ", usedCount: " + step.usedCount + ", dispatchTime: " + step.dispatchTime + ", sendRPC: " + sendRPC);
			#endif

			if(dispatcher != null)
			{
				if(step.dispatchTime > 0f)
				{
					dispatcher.Dispatch(this, step, step.dispatchTime, timestamp);
				}
				else
				{
					for(int i = 0; i < step.usedCount; i++)
						dispatcher.Dispatch(this, step, step.dispatchTime, timestamp);
				}

				OnMadnessStepStartDispatch(step, sendRPC);
			}
		}

		//

		public void RemoteProcessMadnessStep(Config.MadnessMode.MadnessStep receivedStep, double timestamp)
		{
			if(receivedStep == null)
			{
				Debug.LogError("Failed to process receivedStep :: RemoteProcessMadnessStep");
				return;
			}

			Config.MadnessMode.MadnessStep step = null;

			Config.madnessMode.allSteps.TryGetValue(receivedStep.stepType, out step);

			if(step != null)
			{
				ProcessMadnessStep(receivedStep, false, timestamp);
			}
			else
			{
				Debug.LogWarning("Unable to find step to process" + receivedStep);
			}
		}

		#endregion

		#region Events

		private void OnMadnessStepStartDispatch(Config.MadnessMode.MadnessStep step, bool sendRPC)
		{
			if(sendRPC)
				arenaEventDispatcher.DispatchMadnessStepOnRemotes(step);

			if(hud != null)
				hud.OnMadnessStepStartDispatch(step);
			//

			gameMessage.SetMessage(step);
		}


		public void OnTimedMadnessStepDispatched(Config.MadnessMode.MadnessStep step)
		{
			if(hud != null)
				hud.OnTimedMadnessStepDispatched(step);
		}

		#endregion

		//

		#region Networking

		//

		public void RefreshNetworkState(MadnessModeMessageStruct mmms)
		{
			rpc.Set(Madness.MadnessModeController.networkPropsId, mmms.Serialize());
		}

		public void HandleMadnessModeNetworkData(object value)
		{
			var mmmms = MadnessModeMessageStruct.Deserialize((byte[])value);

			if(mmmms == null)
			{
				Debug.LogError("Failed to deserialize MadnessModeMessageStruct from " + value);
				return;
			}

			var step = GetMadnessStepDispatcher(mmmms.stepType);

			if(step == null)
			{
				Debug.LogError("Failed to query madness step of type " + mmmms.stepType);
				return;
			}

			step.HandleMadnessModeNetworkData(mmmms.data);
		}

		#endregion

		//

		public void RestoreAllMadnessSteps()
		{
			foreach(var kvp in madnessStepImpls)
			{
				var impl = kvp.Value;

				if(impl != null && impl.isActive)
					impl.RestoreState();
			}
		}

		#region Implementation dependent props

		public bool isSilenceEnabled
		{
			get
			{
				//TODO: tady by stalo za uvahu to cachovat
				var dispatcher = GetMadnessStepDispatcher(MadnessStepType.Silence);


				return dispatcher != null && dispatcher.isActive;
			}
		}

		//

		public float diarrheaShootTime
		{
			get
			{ 
				//TODO: tady by stalo za uvahu to cachovat
				var dispatcher = GetMadnessStepDispatcher(MadnessStepType.Diarrhea) as Madness.Diarrhea_MadnessModeImpl;

				return dispatcher != null ? dispatcher.diarrheaShootTime : -1f;
			
			}
		}

		//

		public int explosiveCratesUseCount
		{
			get
			{
				//TODO: tady by stalo za uvahu to cachovat
				var dispatcher = GetMadnessStepDispatcher(MadnessStepType.ExplosiveCrates);

				return dispatcher != null ? dispatcher.useCount : 0;
			}
		}

		//

		public bool isStopCratesAutoRespawnActive 
		{
			get
			{
				//TODO: tady by stalo za uvahu to cachovat
				var dispatcher = GetMadnessStepDispatcher(MadnessStepType.StopCratesAutoRespawn);

				return dispatcher != null && dispatcher.isActive;
			}
		}

		//

		public bool isNoDeathPenaltyActive 
		{
			get
			{
				//TODO: tady by stalo za uvahu to cachovat
				var dispatcher = GetMadnessStepDispatcher(MadnessStepType.NoDeathPenalty);

				return dispatcher != null && dispatcher.isActive;
			}
		}

		#endregion
	}
}
