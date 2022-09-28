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
using GMReloaded;
using System;

namespace GMReloaded
{

	public abstract class EquipConfigBase
	{
		public enum State
		{
			NotBought,
			Bought,
			Equipped
		}

		// Cena v sroubcich
		public int price;

		public abstract string id { get; }

		private string _stateKey;
		private string stateKey
		{
			get
			{ 
				if(_stateKey == null)
				{
					if(id == null)
						Debug.LogError("EquipConfigBase id not set");
					
					_stateKey = id + "_State";
				}

				return _stateKey;
			}
		}
		public State state
		{
			get
			{
				return (State)PlayerPrefs.GetInt(stateKey, 0);
			}

			set
			{ 
				PlayerPrefs.SetInt(stateKey, (int)value);
			}
		}
	}
	
}