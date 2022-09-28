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

namespace GMReloaded.Madness
{
	public abstract class IMadnessModeStepDispatch
	{
		public abstract class MadnessStepProperties : ISerializableStruct
		{
			#region ISerializableStruct implementation

			public abstract void OnSerializeStruct(System.IO.BinaryWriter bw);

			public abstract bool OnDeserializeStruct(System.IO.BinaryReader br);

			#endregion
		}

		//

		public bool isActive { get; private set; }

		public int useCount { get; private set; }

		//

		private Config.MadnessMode.MadnessStep step;

		//

		protected MadnessModeController mmc;

		//

		protected SoundManager snd { get { return SoundManager.GetInstance(); } }

		//

		protected RobotEmilNetworked robotParent
		{
			get
			{ 
				var lcre = LocalClientRobotEmil.Instance;

				return lcre != null ? lcre.parentRobot : null;
			}
		}

		//

		public virtual void Dispatch(MadnessModeController mmc, Config.MadnessMode.MadnessStep step, float dispatchTime, double timestamp)
		{
			this.mmc = mmc;

			this.step = step;

			//

			isActive = true;
			useCount++;

			if(dispatchTime > 0f)
				Independent.Coroutine.Instance.ProcessCoroutine(UpdateCoroutine(dispatchTime, timestamp));

			Debug.Log("Started dispatch of madness step " + this + ", useCount " + useCount);
		}

		private IEnumerator UpdateCoroutine(float dispatchTime, double timestamp)
		{
			double t = (timestamp > 0) ? (PhotonNetwork.time - timestamp) : 0;

			Debug.Log(t + " >= " + dispatchTime);

			if(t >= dispatchTime)
			{
				#if UNITY_EDITOR
				Debug.Log("Madness event " + step.stepType + " already timeouted " + t);
				#endif
			}

			do
			{
				t += Time.deltaTime;

				Update(Time.deltaTime);
				yield return null;
			}
			while(t < dispatchTime);

			RestoreState();

			mmc.OnTimedMadnessStepDispatched(step);
		}

		protected virtual void Update(float dt)
		{
			
		}

		public virtual void RestoreState()
		{
			if(useCount > 0)
				useCount--;
			
			isActive = useCount > 0;

			Debug.Log("Restored state of madness step " + this + ", useCount " + useCount);
		}

		#region Data Handling

		public virtual void HandleMadnessModeNetworkData(byte[] data)
		{
		}

		#endregion

		protected MadnessModeMessageStruct CreateMessageStruct()
		{
			MadnessModeMessageStruct mmms = new MadnessModeMessageStruct();

			mmms.stepType = step.stepType;

			return mmms;
		}
	}
	
}
