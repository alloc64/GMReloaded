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

namespace GMReloaded.UI.Final.Charts
{
	public class KBChartsTabContainer : GMReloaded.UI.Final.Tabs.KBTabContainer
	{
		[SerializeField]
		private KBTabRef internalAPITabRef;

		[SerializeField]
		private KBTabRef steamChartsTabRef;

		//

		protected override void OnEnable()
		{
			tabs.Clear();

			#if STEAM_ENABLED
			tabs.Add(steamChartsTabRef);
			#endif

			tabs.Add(internalAPITabRef);

			base.OnEnable();
		}
	}
	
}
