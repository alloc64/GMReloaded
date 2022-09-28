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
using GMReloaded.Bonuses.Effects;
using System.Collections.Generic;
using Independent;
using TouchOrchestra;
using System;

namespace GMReloaded
{
	public abstract class IAnimatorMonoBehaviour : MonoBehaviourTO
	{
		[SerializeField]
		public Animator animator;

		protected virtual void ResetAnimatorLayerWeights()
		{
			if(animator == null)
				return;

			for(int i = 1; i < animator.layerCount; i++)
				SetAnimatorLayerWeight(i, 0f);
		}

		public void SetAnimatorLayerWeight(int layerIndex, float weight)
		{
			if(animator != null)
				animator.SetLayerWeight(layerIndex, weight);
		}
	}
	
}
