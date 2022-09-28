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
	[Serializable]
	public class RobotEmilStepsSound
	{
		[SerializeField]
		private float timeBetweenSteps = 0.1f;

		private int ptr = 0;

		private ISound [] activeStepsSounds = null;

		private Materials lastMaterial = Materials.Unknown;

		private Dictionary<Materials, ISound[]> soundsContainer = new Dictionary<Materials, ISound[]>();

		private RobotEmil robotParent;

		private Transform robotTransform;

		private SoundManager snd { get { return SoundManager.GetInstance(); } }

		public void Initialize(RobotEmil robot)
		{
			this.robotParent = robot;
			this.robotTransform = robotParent.transform;

			PreloadSounds();

			HandleMaterialChange(Materials.Concrete);
		}

		private void PreloadSounds()
		{
			foreach (Materials mat in Enum.GetValues(typeof(Materials)))
			{
				if(mat == Materials.Unknown)
					continue;

				ISound[] snds = new ISound[2];

				snds[0] = snd.Load("Robot_Steps_" + mat + "0");
				snds[1] = snd.Load("Robot_Steps_" + mat + "1");

				soundsContainer[mat] = snds;
			}
		}

		private float timer = 0f;

		public void Update(float dt)
		{
			if(activeStepsSounds == null || robotParent == null ||  robotParent.state != RobotEmil.State.Move || robotParent.freezed || !robotParent.viewObserver.running)
				return;

			if(timer < timeBetweenSteps)
				timer += dt * robotParent.speedMultiplier;

			if(timer >= timeBetweenSteps)
			{
				var sound = activeStepsSounds[ptr];

				if(sound != null)
					sound.Play(robotTransform);

				ptr++;
				ptr %= 2;

				timer = 0f;
			}
		}

		public void OnControllerColliderHit(ControllerColliderHit hit)
		{
			if(hit.normal.y < 0.99f)
				return;

			HandleMaterialChange(hit.gameObject.tag);	
		}

		private bool materialNotFound = false;

		private string lastMatId = null;

		private void HandleMaterialChange(string matId)
		{
			if(lastMatId == matId)
				return;

			lastMatId = matId;

			Materials mat = Materials.Concrete;

			if(matId != "Untagged")
			{

				try
				{
					mat = (Materials)Enum.Parse(typeof(Materials), matId.Replace("Material_", ""), true);
				}
				catch
				{
					if(!materialNotFound)
					{
						Debug.LogError("Material of object not found - " + matId);
						materialNotFound = true;
					}

					mat = Materials.Concrete;
				}
			}

			HandleMaterialChange(mat);
		}

		private void HandleMaterialChange(Materials mat)
		{
			if(lastMaterial == mat)
				return;

			lastMaterial = mat;

			ISound[] snds = null;

			soundsContainer.TryGetValue(mat, out snds);

			if(snds == null)
				soundsContainer.TryGetValue(Materials.Concrete, out snds);

			activeStepsSounds = snds;
		}
	}
	
}
