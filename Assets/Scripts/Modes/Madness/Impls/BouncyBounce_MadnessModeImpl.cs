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
using GMReloaded.Entities;

namespace GMReloaded.Madness
{
	public class BouncyBounce_MadnessModeImpl : IMadnessModeStepDispatch
	{
		private class NetworkProperties : MadnessStepProperties
		{
			public float bounciness;

			#region ISerializableStruct implementation

			public override void OnSerializeStruct(System.IO.BinaryWriter bw)
			{
				bw.Write(bounciness);
			}

			public override bool OnDeserializeStruct(System.IO.BinaryReader br)
			{
				bounciness = br.ReadSingle();

				return bounciness >= 0.0f && bounciness <= 1.0f;
			}

			#endregion
			
		}

		//


		private float _bounciness = Config.MadnessMode.BouncyBounce_Bounciness_Min;

		public float bounciness
		{
			get { return _bounciness; }
			private set 
			{ 
				//Debug.Log("Set_bounciness" + value);
				_bounciness = value;
			}
		}

		//

		public override void Dispatch(MadnessModeController mmc, Config.MadnessMode.MadnessStep step, float dispatchTime, double timestamp)
		{
			base.Dispatch(mmc, step, dispatchTime, timestamp);

			if(PhotonNetwork.isMasterClient)
			{
				if(bounciness >= Config.MadnessMode.BouncyBounce_Bounciness_Max) // mame maximum
					return;

				bounciness = Mathf.Clamp(bounciness + Config.MadnessMode.BouncyBounce_Bounciness_Progress, Config.MadnessMode.BouncyBounce_Bounciness_Min, Config.MadnessMode.BouncyBounce_Bounciness_Max);

				RefreshNetworkBounciness(bounciness);
			}
		}

		public override void RestoreState()
		{
			base.RestoreState();

			if(PhotonNetwork.isMasterClient)
			{
				RefreshNetworkBounciness(Config.MadnessMode.BouncyBounce_Bounciness_Min);
			}
		}

		//

		public override void HandleMadnessModeNetworkData(byte[] data)
		{
			base.HandleMadnessModeNetworkData(data);

			//

			var np = StructSerializer.Deserialize<NetworkProperties>(data);

			bounciness = np.bounciness;
		}

		//

		private void RefreshNetworkBounciness(float bounciness)
		{
			var mmms = CreateMessageStruct();

			var np = new NetworkProperties();
			np.bounciness = bounciness;

			mmms.SerializeData<NetworkProperties>(np);

			mmc.RefreshNetworkState(mmms);
		}
	}
}
