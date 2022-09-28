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
	public class KBFocusableCheckBox : KBFocusableChooser
	{
		public Action<bool> OnCheckedStateChanged;

		[SerializeField]
		private bool @checked;

		//

		private bool _initialized = false;

		//

		public bool IsOn
		{
			get
			{ 
				return @checked;
			}

			set
			{ 
				Initialize();
				SetListIndex(value ? 1 : 0);
			}
		}

		//

		protected override void Awake()
		{
			base.Awake();

			Initialize();

			IsOn = @checked;
		}

		private void Initialize()
		{
			if(_initialized)
				return;

			_initialized = true;

			AddItemLocalized("Disabled");
			AddItemLocalized("Enabled");
		}

		protected override void _OnItemChanged(int index)
		{
			base._OnItemChanged(index);

			@checked = index == 1;

			_OnCheckedStateChanged(@checked);
		}

		protected virtual void _OnCheckedStateChanged(bool @checked)
		{
			if(OnCheckedStateChanged != null)
				OnCheckedStateChanged(@checked);
		}
	}
}