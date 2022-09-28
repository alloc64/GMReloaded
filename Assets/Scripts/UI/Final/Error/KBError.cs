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

namespace GMReloaded.UI.Final.Error
{
	public class KBError : MonoBehaviourTO
	{
		[SerializeField]
		private tk2dTextMesh errorText;

		private double timeToClear;

		#region Unity

		private void Update()
		{
			if(timeToClear >= 0 && timeToClear < Time.time)
			{
				SetError(null);
			}
		}

		#endregion

		public void SetErrorLocalized(string locId, params object[] p)
		{
			string text = null;

			if(locId != null)
				text = localization.GetValue(locId, p);

			SetError(text);
		}

		public void SetError(string text)
		{
			SetActive(text != null);

			if(errorText != null)
				errorText.text = text == null ? "" : text;

			timeToClear = Time.time + 8.0;
		}
	}
}
