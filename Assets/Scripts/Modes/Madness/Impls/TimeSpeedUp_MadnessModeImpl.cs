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
	public class TimeSpeedUp_MadnessModeImpl : IMadnessModeStepDispatch
	{
		private class NetworkProperties : MadnessStepProperties
		{
			public float timeMultiplier;

			#region ISerializableStruct implementation

			public override void OnSerializeStruct(System.IO.BinaryWriter bw)
			{
				bw.Write(timeMultiplier);
			}

			public override bool OnDeserializeStruct(System.IO.BinaryReader br)
			{
				timeMultiplier = br.ReadSingle();

				return timeMultiplier >= 1.0f && timeMultiplier <= 2.0f;
			}

			#endregion

		}

		//

		private float _timeMultiplier = Config.MadnessMode.TimeSpeedUp_Min;

		private float timeMultiplier
		{
			get { return _timeMultiplier; }
			set
			{ 
				this._timeMultiplier = Time.timeScale = value;
				Debug.Log("Set_timeMultiplier " + _timeMultiplier);
			}
		}

		//

		public override void Dispatch(MadnessModeController mmc, Config.MadnessMode.MadnessStep step, float dispatchTime, double timestamp)
		{
			base.Dispatch(mmc, step, dispatchTime, timestamp);

			if(PhotonNetwork.isMasterClient)
			{
				RefreshNetworkTimeScale(Mathf.Clamp(timeMultiplier + Config.MadnessMode.TimeSpeedUp_Progress, Config.MadnessMode.TimeSpeedUp_Min, Config.MadnessMode.TimeSpeedUp_Max), true);
			}

			snd.speech.GrenadeMadness();
		}

		public override void RestoreState()
		{
			base.RestoreState();

			if(PhotonNetwork.isMasterClient)
			{
				RefreshNetworkTimeScale(Config.MadnessMode.TimeSpeedUp_Min, true);
			}
		}

		//

		public override void HandleMadnessModeNetworkData(byte[] data)
		{
			base.HandleMadnessModeNetworkData(data);

			var np = StructSerializer.Deserialize<NetworkProperties>(data);

			RefreshNetworkTimeScale(np.timeMultiplier, false);
		}

		//

		private void RefreshNetworkTimeScale(float timeMultiplier, bool refreshNetwork)
		{
			this.timeMultiplier = timeMultiplier;

			if(PhotonNetwork.isMasterClient && refreshNetwork)
			{
				var mmms = CreateMessageStruct();

				var np = new NetworkProperties();
				np.timeMultiplier = timeMultiplier;

				mmms.SerializeData<NetworkProperties>(np);

				mmc.RefreshNetworkState(mmms);
			}
		}
	}
}
