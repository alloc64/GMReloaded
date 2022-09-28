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

namespace GMReloaded.Achievements
{
	public class AchievableItemBase 
	{
		public string name;

		private string _key = "";
		public string key
		{
			get { return _key; }
			private set { _key = value; }
		}

		private string activatedKey;

		public int threshold = 0;

		protected int reportStep = 1;

		private string lastReportedProgressKey;
		private int _lastReportedProgress = -1;
		public int lastReportedProgress
		{
			get
			{ 
				if(_lastReportedProgress == -1)
					_lastReportedProgress = Mathf.Clamp(Cloud.CloudSyncedPlayerPrefs.GetInt(lastReportedProgressKey, reportStep), 0, threshold);
			
				return _lastReportedProgress;
			}

			set
			{ 

				if(_lastReportedProgress == value)
					return;

				_lastReportedProgress = Mathf.Clamp(value, 0, threshold);

				Cloud.CloudSyncedPlayerPrefs.SetInt(lastReportedProgressKey, _lastReportedProgress);
			}
		}

		private int _progress = -1;
		public int progress
		{
			get 
			{ 
				if(_progress == -1)
					_progress =  Mathf.Clamp(Cloud.CloudSyncedPlayerPrefs.GetInt(key), 0, threshold); 

				return _progress;
			}

			set 
			{ 
				_progress = Mathf.Clamp(value, 0, threshold);

				if(_progress >= threshold && !activated)
				{
					activated = true;

					if(callback != null)
						callback.ActivateItem(this);
				}
				else
				{
					if(_progress - lastReportedProgress >= reportStep)
					{
						if(callback != null)
							callback.IndicateItemProgress(key, _progress, threshold);
						
						lastReportedProgress = _progress;
					}
				}

				if(_progress > threshold)
					return;

				Cloud.CloudSyncedPlayerPrefs.SetInt(key, _progress); 
			}
		}

		public float progressPercent
		{
			get { return threshold == 0 ? 0f : (((float)progress) / ((float)threshold)); }
		}

		private bool? _activated;
		public bool activated
		{
			get 
			{ 
				if(!_activated.HasValue)
				{
					_activated = Cloud.CloudSyncedPlayerPrefs.GetInt(activatedKey) == 1;
				}

				return _activated.Value; 
			}
			set 
			{ 
				_activated = value;
				Cloud.CloudSyncedPlayerPrefs.SetInt(activatedKey, _activated.Value ? 1 : 0); 
			}
		}

		private IAchievableItemCallback callback;

		public AchievableItemBase()
		{
			
		}

		public AchievableItemBase(IAchievableItemCallback callback)
		{
			SetCallback(callback);
		}

		public void SetCallback(IAchievableItemCallback callback)
		{
			this.callback = callback; 
		}

		public void Setup(string key, int threshold, int reportStep)
		{
			this.key = key;
			this.activatedKey = this.key + "_Activated";
			this.lastReportedProgressKey = this.key + "_LastReport";
			this.threshold = threshold;
			this.reportStep = reportStep;
		}

		public void ResetMission()
		{
			_lastReportedProgress = -1;
			_progress = -1;
			progress = 0;
			activated = false;
		}
	}
}
