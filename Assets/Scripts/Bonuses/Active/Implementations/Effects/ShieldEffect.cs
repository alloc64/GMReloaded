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
using TouchOrchestra;

namespace GMReloaded.Bonuses.Effects
{
	public class ShieldEffect : IBonusEffect
	{
		[SerializeField]
		private new Renderer renderer;

		private Material _material;
		private Material material
		{
			get
			{
				if(_material == null)
					_material = renderer.material;

				return _material;
			}
		}

		public override void Show()
		{
			SetActive(true);
			Ease.Instance.Alpha(-2.35f, 0.2f, 0.35f, EaseType.In, (a) => material.SetFloat("_Alpha", a));
		}

		public override void Hide()
		{
			Ease.Instance.Alpha(0.2f, -2.35f, 0.35f, EaseType.Out, (a) => material.SetFloat("_Alpha", a), () => SetActive(false));
		}

		protected override void OnDestroy()
		{
			_material = null;
			base.OnDestroy();
		}
	}
}
