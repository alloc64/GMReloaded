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
	public class GizmoIndicator : MonoBehaviourTO
	{
		[SerializeField]	
		private Color color = Color.red;

		[SerializeField]	
		private float radius = 0.05f;

		[SerializeField]	
		private bool onlyOnSelected = true;

		protected virtual void OnDrawGizmosSelected()
		{
			if(onlyOnSelected)
			{
				Gizmos.color = color;
				Gizmos.DrawWireSphere(position, radius); 
			}
		}

		protected virtual void OnDrawGizmos()
		{
			if(!onlyOnSelected)
			{
				Gizmos.color = color;
				Gizmos.DrawWireSphere(position, radius); 
			}
		}
	}
	
}