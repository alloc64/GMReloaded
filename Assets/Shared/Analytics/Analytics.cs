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
using System;

namespace TouchOrchestra
{
	public class Analytics : MonoSingletonPersistent<Analytics> 
	{
		[SerializeField]
		private GoogleAnalyticsV3 _googleAnalytics;
		public GoogleAnalyticsV3 googleAnalytics
		{
			get
			{ 
				if(_googleAnalytics == null)
					_googleAnalytics = Prefabs.Load<GoogleAnalyticsV3>("Analytics/GAv3", transform);

				return _googleAnalytics;
			}
		}

		public void LogScreen(string title)
		{
			googleAnalytics.LogScreen(title);
		}

		public void LogEvent(string eventCategory, string eventAction, string eventLabel, long value)
		{
			googleAnalytics.LogEvent(eventCategory, eventAction, eventLabel, value);
		}

		public void LogSocial(string socialNetwork, string socialAction, string socialTarget)
		{
			googleAnalytics.LogSocial(socialNetwork, socialAction, socialTarget);
		}

		protected override void OnDestroy()
		{
			googleAnalytics.Dispose();

			if(googleAnalytics != null)
				Destroy(googleAnalytics.gameObject);

			base.OnDestroy();
		}
	}
}
