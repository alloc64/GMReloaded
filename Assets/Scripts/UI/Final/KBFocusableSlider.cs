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
using System.Collections.Generic;

namespace GMReloaded.UI.Final
{
	public class KBFocusableSlider : KBFocusableGUIItemWithStateChange
	{
		[SerializeField]
		private tk2dTextMesh titleTextMesh;

		[SerializeField]
		private UIProgressBarClipped progressBar;

		//

		[SerializeField]
		private tk2dUIItem arrowLeftUiItem;

		[SerializeField]
		private tk2dUIItem arrowRightUiItem;

		//

		[SerializeField]
		private string presetTitleLocalizationId;

		//

		[SerializeField]
		private float _progress = 0f;
		public float progress 
		{
			get { return _progress; }

			set
			{ 
				_progress = value;
				_OnProgressChanged(_progress);
			}
		}

		private float step = 0.1f;

		public Action<float> OnProgressChanged;

		#region Unity

		protected override void Awake()
		{
			base.Awake();

			SetTitleLocalized(presetTitleLocalizationId);

			SetProgress(_progress);

			if(arrowLeftUiItem == null)
			{
				Debug.LogError("arrowLeftUiItem == null");
				return;
			}

			if(arrowRightUiItem == null)
			{
				Debug.LogError("arrowRightUiItem == null");
				return;
			}

			arrowLeftUiItem.OnClick += OnLeftArrowClick;
			arrowRightUiItem.OnClick += OnRightArrowClick;
		}

		#endregion

		private void OnLeftArrowClick()
		{
			progress -= step;

			if(progress < 0f)
				progress = 0f;
		}

		private void OnRightArrowClick()
		{
			progress += step;

			if(progress > 1f)
				progress = 1f;
		}

		public void SetTitleLocalized(string titleLocalizationId)
		{
			if(titleTextMesh != null)
				titleTextMesh.text = localization.GetValue(titleLocalizationId);
		}

		public void SetTitle(string title)
		{
			if(titleTextMesh != null)
				titleTextMesh.text = title == null ? "" : title;
		}

		#region Progress

		public void SetStep(float step)
		{
			this.step = step;
		}

		private void SetProgress(float progress)
		{
			if(progressBar != null)
				progressBar.SetProgress(progress);
		}

		private void _OnProgressChanged(float progress)
		{
			SetProgress(progress);

			if(OnProgressChanged != null)
				OnProgressChanged(progress);
		}

		#endregion
	}
	
}