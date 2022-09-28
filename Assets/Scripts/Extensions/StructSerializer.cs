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

public class StructSerializer 
{
	public static byte[] Serialize<T>(T @struct) where T : ISerializableStruct
	{
		if(@struct == null)
		{
			return null;
		}


		byte[] serializedBytes = null;
		try
		{
			using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
			{
				using(System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms))
				{
					@struct.OnSerializeStruct(bw);

					serializedBytes = ms.ToArray();

					//Debug.Log("Serialized " + serializedBytes.Length + "B");
				}
			}
		}
		catch(System.Exception e)
		{
			Debug.LogWarning("Failed to serialize data " + e);
		}

		return serializedBytes;
	}

	public static byte[] Serialize(System.Action<System.IO.BinaryWriter> onSerialize)
	{
		byte[] serializedBytes = null;

		try
		{
			using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
			{
				using(System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms))
				{
					if(onSerialize != null)
						onSerialize(bw);

					serializedBytes = ms.ToArray();

					//Debug.Log("Serialized " + serializedBytes.Length + "B");
				}
			}
		}
		catch(System.Exception e)
		{
			Debug.LogWarning("Failed to serialize data " + e);
		}

		return serializedBytes;
	}

	//
	// Deserialization
	//

	public static T Deserialize<T>(T @struct, byte[] bytes) where T : ISerializableStruct
	{
		try
		{
			using (System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes))
			{
				using(System.IO.BinaryReader br = new System.IO.BinaryReader(ms))
				{
					if(br.PeekChar() != -1)
						@struct.OnDeserializeStruct(br);
				}
			}
		}
		catch(System.Exception e)
		{
			Debug.LogWarning("Failed to deserialize data " + e);
		}

		return @struct;
	}

	public static T Deserialize<T>(byte[] bytes) where T : ISerializableStruct, new()
	{
		return Deserialize<T>(new T(), bytes);
	}

	public static T Deserialize<T>(byte[] bytes, System.Func<System.IO.BinaryReader, T> onDeserialize) 
	{
		T obj = default(T);

		try
		{
			using (System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes))
			{
				using(System.IO.BinaryReader br = new System.IO.BinaryReader(ms))
				{
					if(onDeserialize != null && br.PeekChar() != -1)
						obj = onDeserialize(br);
				}
			}
		}
		catch(System.Exception e)
		{
			Debug.LogWarning("Failed to deserialize data " + e);
		}

		return obj;
	}
}
