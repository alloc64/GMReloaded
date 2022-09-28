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
using UnityEngine.Cloud.Analytics;
using System.Collections.Generic;

namespace GMReloaded.Analytics
{
	public class GAI : MonoSingletonPersistent<GAI> 
	{
		private void OnEnable() 
		{
			Application.logMessageReceived += HandleLog;
		}

		private void OnDisable() 
		{
			Application.logMessageReceived -= HandleLog;
		}


		[SerializeField]
		private GoogleAnalyticsV3 _googleAnalytics;
		public GoogleAnalyticsV3 googleAnalytics
		{
			get
			{ 
				if(_googleAnalytics == null)
					_googleAnalytics = Prefabs.Load<GoogleAnalyticsV3>("Analytics/GAv3", transform);

				if(_googleAnalytics != null)
				{
					_googleAnalytics.bundleVersion = Config.clientVersion + "_" + Config.appID;
				}

				return _googleAnalytics;
			}
		}

		private void Start()
		{
			UnityAnalytics.StartSDK("e877ed2c-812c-4635-8604-01d5d8343a3b");
		}

		public void LogScreen(string title)
		{
			googleAnalytics.LogScreen(title);
		}

		public void LogEvent(string eventCategory, string eventAction, string eventLabel, long value)
		{
			googleAnalytics.LogEvent(eventCategory, eventAction, eventLabel, value);
			UnityAnalytics.CustomEvent(eventCategory, new Dictionary<string, object> { { "action", eventAction }, { "value", value }});
		}

		public void LogException(string exception, bool isFatal = false)
		{
			googleAnalytics.LogException(exception, isFatal);
		}

		public void LogTransaction(string transID, int amount, string currency) 
		{
			googleAnalytics.LogTransaction(transID, "game", (double)amount, 0.0f, 0.0f);
			UnityAnalytics.Transaction(transID, amount, currency);
		}

		private void HandleLog(string condition, string stackTrace, LogType type)
		{
			if (type == LogType.Exception)
			{
				LogException(string.Format("{0}: {1}\n{2}", type, condition, stackTrace));
			}
		}

		protected override void OnDestroy()
		{
			if (googleAnalytics != null) 
			{
				googleAnalytics.Dispose();
				Destroy (googleAnalytics.gameObject);
			}

			base.OnDestroy();
		}
	}
}
