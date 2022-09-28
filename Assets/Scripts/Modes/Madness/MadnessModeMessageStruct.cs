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

namespace GMReloaded.Madness
{
	public class MadnessModeMessageStruct : ISerializableStruct
	{
		public MadnessStepType stepType;

		public byte[] data;

		//

		#region ISerializableStruct implementation

		public void OnSerializeStruct(System.IO.BinaryWriter bw)
		{
			bw.Write((int)stepType);
			bw.Write(data.Length);
			bw.Write(data);
		}

		public bool OnDeserializeStruct(System.IO.BinaryReader br)
		{
			stepType = (MadnessStepType)br.ReadInt32();
			int dataLenght = br.ReadInt32();

			if(dataLenght < 0)
				return false;

			data = br.ReadBytes(dataLenght);

			return true;
		}

		#endregion

		public byte[] Serialize()
		{
			return StructSerializer.Serialize<MadnessModeMessageStruct>(this);
		}

		public static MadnessModeMessageStruct Deserialize(byte[] data)
		{
			return StructSerializer.Deserialize<MadnessModeMessageStruct>(data);
		}

		//

		public void SerializeData<T>(T serializableStruct) where T : ISerializableStruct
		{
			this.data = StructSerializer.Serialize<T>(serializableStruct);
		}
	}
	
}
