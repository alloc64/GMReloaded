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
	public class MadnessModeStepsMPStruct : ISerializableStruct
	{
		//

		private Dictionary<int, Dictionary<MadnessStepType, byte>> _steps = new Dictionary<int, Dictionary<MadnessStepType, byte>>();
		public Dictionary<int, Dictionary<MadnessStepType, byte>> steps { get { return _steps; } }

		//

		public MadnessModeStepsMPStruct() : this(Config.madnessMode.steps)
		{
			
		}

		public MadnessModeStepsMPStruct(Dictionary<int, Dictionary<MadnessStepType, Config.MadnessMode.MadnessStep>> aSteps)
		{
			foreach(var aKvp in aSteps)
			{
				int time = aKvp.Key;

				Dictionary<MadnessStepType, byte> finalSteps = null;
				steps.TryGetValue(time, out finalSteps);

				if(finalSteps == null)
					finalSteps = new Dictionary<MadnessStepType, byte>();

				foreach(var bKvp in aKvp.Value)
				{
					var step = bKvp.Value;

					if(step.usedCount > 0)
					{
						finalSteps[step.stepType] = step.usedCount;
					}
				}

				if(finalSteps.Count > 0)
					steps[time] = finalSteps;
			}
		}

		#region ISerializableStruct implementation

		public void OnSerializeStruct(System.IO.BinaryWriter bw)
		{
			bw.Write(steps.Count);

			//

			foreach(var kvpSteps in steps)
			{
				int time = kvpSteps.Key;
				var oneTimeSteps = kvpSteps.Value;

				//

				bw.Write(time); 
				bw.Write(oneTimeSteps.Count);

				//

				foreach(var step in oneTimeSteps)
				{
					MadnessStepType stepType = step.Key;
					byte useCount = step.Value;

					//

					bw.Write((int)stepType);
					bw.Write(useCount);
				}
			}
		}

		public bool OnDeserializeStruct(System.IO.BinaryReader br)
		{
			int stepsCount = br.ReadInt32();

			for(int i = 0; i < stepsCount; i++)
			{
				int time = br.ReadInt32();
				int oneTimeStepsCount = br.ReadInt32();

				Dictionary<MadnessStepType, byte> oneTimeSteps = new Dictionary<MadnessStepType, byte>();

				for(int j = 0; j < oneTimeStepsCount; j++)
				{
					MadnessStepType stepType = (MadnessStepType)br.ReadInt32();
					byte useCount = br.ReadByte();

					oneTimeSteps[stepType] = useCount;
				}

				steps[time] = oneTimeSteps;
			}

			return true;
		}

		#endregion

		public byte[] Serialize()
		{
			return StructSerializer.Serialize<Madness.MadnessModeStepsMPStruct>(this);
		}

		public static MadnessModeStepsMPStruct Deserialize(byte[] bytes)
		{
			return StructSerializer.Deserialize<Madness.MadnessModeStepsMPStruct>(bytes);
		}

		#if UNITY_EDITOR
		public void Dump()
		{
			Debug.Log("Dump " + steps.Count);

			foreach(var kvpSteps in steps)
			{
				int time = kvpSteps.Key;
				var oneTimeSteps = kvpSteps.Value;

				Debug.Log("time " + time);

				foreach(var step in oneTimeSteps)
				{
					MadnessStepType stepType = step.Key;
					byte useCount = step.Value;

					Debug.Log(stepType + " - " + useCount);
				}
			}
		}
		#endif
	}
}
