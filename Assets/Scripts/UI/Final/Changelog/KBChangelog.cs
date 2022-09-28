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
using CodeStage.AntiCheat.ObscuredTypes;

namespace GMReloaded.UI.Final.Changelog
{
	public class KBChangelog : KBFocusableSuccessorsGUI
	{
		private const string CanShowKey = "KBChangelog_CanShow";
		public static bool CanShow
		{
			get
			{ 
				return Cloud.CloudSyncedPlayerPrefs.GetString(CanShowKey) != Config.clientVersion;
			}

			set
			{ 
				Cloud.CloudSyncedPlayerPrefs.SetString(CanShowKey, value ? null : Config.clientVersion);
			}
		}

		//

		[SerializeField]
		private tk2dUIScrollableArea scrollableArea;

		[SerializeField]
		private tk2dTextMesh changelogTextMesh;

		//

		public override void Show(object bundle)
		{
			base.Show(bundle);

			if(changelogTextMesh == null)
				return;

			TextAsset asset = Resources.Load<TextAsset>("changelog");

			if(asset == null)
			{
				Debug.LogError("failed to load changelog text - asset not exists...");
				return;
			}

			changelogTextMesh.text = asset.text;

			Timer.DelayAsync(0.2f, CalculateContentLength);

			CanShow = false;
		}

		private void CalculateContentLength()
		{
			if(scrollableArea == null)
				return;

			Bounds b = tk2dUIItemBoundsHelper.GetRendererBoundsInChildren(scrollableArea.contentContainer.transform, scrollableArea.contentContainer.transform);
			b.Encapsulate(Vector3.zero);

			float contentSize = (scrollableArea.scrollAxes == tk2dUIScrollableArea.Axes.XAxis) ? b.size.x : b.size.y;

			scrollableArea.ContentLength = contentSize;
		}
	}
}
