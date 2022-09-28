﻿/************************************************************************
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

namespace GMReloaded
{
	public static class RoomInfoExtensions 
	{
		public static string GetLevelId(this RoomInfo roomInfo)
		{
			if(roomInfo == null)
				return null;

			var props = roomInfo.customProperties;

			object level = null;
			props.TryGetValue(ArenaEventDispatcher.PropsConstants.LevelKey, out level);

			return level == null ? null : level.ToString();
		}
	}
}