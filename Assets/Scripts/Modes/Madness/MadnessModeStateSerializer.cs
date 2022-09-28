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
using System.IO;

namespace GMReloaded.Madness
{
	public class MadnessModeStateSerializer
	{
		public void SerializeToFile(string directoryPath)
		{
			MadnessModeStepsMPStruct madness = new MadnessModeStepsMPStruct();

			File.WriteAllBytes("madness_settings.dat", madness.Serialize());
		}

		public MadnessModeStepsMPStruct DeserializeFromFile(string path)
		{
			byte[] bytes = File.ReadAllBytes(path);

			if(bytes == null || bytes.Length < 1)
			{
				Debug.LogError("Failed to load bytes from file " + path);
				return null;
			}

			return MadnessModeStepsMPStruct.Deserialize(bytes);
		}
	}
}
