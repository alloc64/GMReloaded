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

namespace GMReloaded.UI
{
	public class UIInfiniteLoadingIndicator : MonoBehaviourTO
	{
		private float loadingRotation = 0f;

		[SerializeField]
		private float loadingRotationSpeed = 160f;

		private void Update()
		{
			Quaternion q = localRotation;
			q.eulerAngles = new Vector3(0f, 0f, loadingRotation);
			localRotation = q;

			loadingRotation += Time.deltaTime * loadingRotationSpeed;
		}
	}
	
}
