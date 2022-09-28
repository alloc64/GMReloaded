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

namespace GMReloaded
{
	public class TeleportBonusImpl
	{
		public struct TeleportData : ISerializableStruct
		{
			internal int photonPlayerID;
			internal Vector3 position;

			#region ISerializableStruct implementation

			public void OnSerializeStruct(System.IO.BinaryWriter bw)
			{
				bw.Write(photonPlayerID);
				bw.Write(position);
			}

			public bool OnDeserializeStruct(System.IO.BinaryReader br)
			{
				photonPlayerID = br.ReadInt32();
				position = br.ReadVector3();

				return true;
			}

			#endregion

			public override string ToString()
			{
				return string.Format("[TeleportProperties] " + photonPlayerID + " - " + position);
			}
		}

		public struct TeleportProperties : ISerializableStruct
		{
			public byte numActivePlayers;

			public TeleportData [] teleportData;

			public TeleportProperties(TeleportData [] teleportData, byte numActivePlayers)
			{
				this.teleportData = teleportData;
				this.numActivePlayers = numActivePlayers;
			}

			#region ISerializableStruct implementation

			public void OnSerializeStruct(System.IO.BinaryWriter bw)
			{
				bw.Write(numActivePlayers);
				bw.Write(teleportData.Length);

				foreach(var td in teleportData)
					td.OnSerializeStruct(bw);
			}

			public bool OnDeserializeStruct(System.IO.BinaryReader br)
			{
				numActivePlayers = br.ReadByte();

				int teleportDataLength = br.ReadInt32();

				teleportData = new TeleportData[teleportDataLength];

				for(int i = 0; i < teleportDataLength; i++)
				{
					var td = new TeleportData();
					td.OnDeserializeStruct(br);

					teleportData[i] = td;
				}

				return true;
			}

			#endregion
		}

		public byte[] AcquireRandomPositions(Dictionary<int, RobotEmilNetworked> players)
		{
			List<RobotEmilNetworked> activePlayers = new List<RobotEmilNetworked>();

			Debug.Log("AcquireRandomPositions " + players.Count);

			foreach(var kvp in players)
			{
				var p0 = kvp.Value;

				if(p0 != null && p0.state != RobotEmil.State.Dead)
				{
					activePlayers.Add(p0);
				}
			}

			if(activePlayers.Count < 1)
				return null;

			TeleportData[] teleportData = new TeleportData[activePlayers.Count];

			for(int i = 0; i < activePlayers.Count; i++)
			{
				var currPlayer = activePlayers[i];
				var lastPlayer = activePlayers[(activePlayers.Count-1) - i];

				if(currPlayer == null || lastPlayer == null)
					continue;
				
				var td = teleportData[i];

				td.position = lastPlayer.position;
				td.photonPlayerID = currPlayer.photonPlayerId;

				teleportData[i] = td;
			}

			return StructSerializer.Serialize<TeleportProperties>(new TeleportProperties(teleportData, (byte)activePlayers.Count));
		}
	}
	
}
