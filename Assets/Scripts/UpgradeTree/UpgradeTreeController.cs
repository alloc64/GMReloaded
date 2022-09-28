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

namespace GMReloaded.UpgradeTree
{
	public class UpgradeTreeItem
	{
		public string itemId { get; private set; }

		public int defaultPoints { get; private set; }

		public int points { get { return defaultPoints * value; } }

		public int priority { get; private set; }

		//

		private string valueKey;
		private int _value = -1;
		public int value
		{
			get
			{ 
				if(_value == -1)
					_value = Cloud.CloudSyncedPlayerPrefs.GetInt(valueKey, 0);

				return _value;
			}

			set
			{ 
				if(_value == value)
					return;

				_value = value;

				Cloud.CloudSyncedPlayerPrefs.SetInt(valueKey, _value);
			}
		}

		//

		public UpgradeTreeItem(string itemId, int defaultPoints, int priority)
		{
			this.itemId = itemId;

			this.valueKey = "UTI_" + itemId;

			this.defaultPoints = defaultPoints;

			this.priority = priority;
		}
	}

	public class UpgradeTreeController : MonoSingleton<UpgradeTreeController> 
	{
		private Dictionary<string, UpgradeTreeItem> upgradeItems = new Dictionary<string, UpgradeTreeItem>();

		//

		#region Unity

		private void Awake()
		{
			foreach(var item in Config.upgradeTree.upgradeItems)
			{
				RegisterItem(item.itemId, item);
			}
		}

		#endregion

		private void RegisterItem(string itemId, UpgradeTreeItem item)
		{
			upgradeItems.Add(itemId, item);
		}

		private UpgradeTreeItem GetItem(string itemId)
		{
			UpgradeTreeItem item = null;

			upgradeItems.TryGetValue(itemId, out item);
			
			return item;
		}

		public void IncrementItem(string itemId, int amount)
		{
			if(tutorial.isActive)
				return;

			var item = GetItem(itemId);

			if(item == null)
			{
				Debug.LogError("Failed to increment UpgradeTree item - not found");
				return;
			}

			Debug.Log("Increment UpgradeTreeItem " + itemId + " - " + amount);

			item.value += amount;

			GMReloaded.API.Score.Instance.LogRelativeRobotPoints(amount * item.defaultPoints);
		}

		public void Dump()
		{
			Debug.Log("Dumping upgradeTree...");

			float robotPoints = 0;
			foreach(var item in Config.upgradeTree.upgradeItems)
			{
				Debug.Log("UpgradeTreeItem " + item.itemId + " - " + item.value + " - " + item.points);

				robotPoints += item.points;
			}

			Debug.Log("Total robotPoints " + robotPoints);
		}
	}
}
