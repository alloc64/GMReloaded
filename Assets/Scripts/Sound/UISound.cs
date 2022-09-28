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

namespace GMReloaded
{
	public class UISound : MonoBehaviour 
	{
		[HideInInspector]
		public int selectedSoundID;

		[HideInInspector]
		public string soundID;

		public SoundManager sound
		{
			get
			{
				return SoundManager.GetInstance();
			}
		}

		private tk2dUIItem _uiItem;
		protected tk2dUIItem uiItem
		{
			get
			{
				if(_uiItem == null)
					_uiItem = GetComponent<tk2dUIItem>();

				return _uiItem;
			}
		}

		protected virtual void Awake()
		{
			if(string.IsNullOrEmpty(soundID))
			{
				throw new UnassignedReferenceException("soundID");
			}

			if(uiItem == null)
			{
				Debug.LogWarning("Unable to find tk2dUIItem! ::UISound");
				return;
			}

			uiItem.OnClick += OnClick;

			sound.Load(soundID, Sound.Tag.UI);
		}

		protected virtual void OnClick()
		{
			sound.Play(soundID);
		}

		private void OnDestroy()
		{
			_uiItem = null;
		}
	}
}
