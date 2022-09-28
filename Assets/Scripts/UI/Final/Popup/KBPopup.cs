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

namespace GMReloaded.UI.Final.Popup
{
	public class KBPopup : KBFocusableSuccessorsGUI
	{
		public enum Type
		{
			OK,
			YesNO,
			Custom
		}

		[SerializeField]
		private tk2dTextMesh textTextMesh;

		//

		[SerializeField]
		protected KBFocusableGUIItem positiveButton;

		[SerializeField]
		protected KBFocusableGUIItem negativeButton;

		[SerializeField]
		protected KBFocusableGUIItem okButton;

		//

		public Action OnPositiveButtonClicked;

		public Action OnNegativeButtonClicked;

		//

		private bool hideAfterButtonClick = true;

		//

		#region Events

		public override void OnClick(KBFocusableGUIItem guiItem)
		{
			base.OnClick(guiItem);

			switch(guiItem.name)
			{
				case "Positive":
				case "OKPositive":

					_OnPositiveButtonClicked();

				break;
					
				case "Negative":

					_OnNegativeButtonClicked();

				break;
			}
		}

		protected virtual void _OnPositiveButtonClicked()
		{
			if(OnPositiveButtonClicked != null)
				OnPositiveButtonClicked();

			if(hideAfterButtonClick)
				Hide();
		}

		protected virtual void _OnNegativeButtonClicked()
		{
			if(OnNegativeButtonClicked != null)
				OnNegativeButtonClicked();

			if(hideAfterButtonClick)
				Hide();
		}

		#endregion

		#region Alertbox settings

		public virtual void SetAlertType(Type alertType)
		{
			switch(alertType)
			{
				case Type.OK:

					if(positiveButton != null)
						positiveButton.SetActive(false);

					if(negativeButton != null)
						negativeButton.SetActive(false);
					
					if(okButton != null)
						okButton.SetActive(true);

				break;
					
				case Type.Custom:
				break;
					
				case Type.YesNO:
				default:
					
					if(positiveButton != null)
						positiveButton.SetActive(true);

					if(negativeButton != null)
						negativeButton.SetActive(true);

					if(okButton != null)
						okButton.SetActive(false);
				break;
			}
		}

		public void SetHideAfterButtonClick(bool hideAfterButtonClick)
		{
			this.hideAfterButtonClick = hideAfterButtonClick;
		}

		public void SetText(string localizationId)
		{
			if(textTextMesh != null)
				textTextMesh.text = localization.GetValue(localizationId);
		}

		#endregion

		protected override void GoBack()
		{
			if(hideAfterButtonClick)
				Hide();
		}

		public override void Hide()
		{
			base.Hide();

			menuRenderer.RestoreLastFocusedGUI();
		}
	}
}