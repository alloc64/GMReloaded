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

	public class BouncyGrenade : GrenadeBase
	{
		private int bounceCount = 0;

		private float initBounciness = -1f;

		//

		#region Unity

		protected override void Start()
		{
			base.Start();

			SetBounceSound(Config.Sounds.grenadeBounce);

			//TODO: tenhle granat by měl mit trošku jiny exploze než sticky
			var _explosionSound = new SoundContainer("SNDC_Grenade_Explosion");

			for(int i = 0; i < 4; i++)
				_explosionSound.AddSound(Config.Sounds.grenadeExplosion + i);

			SetExplosionSound(_explosionSound);
		}

		protected override void OnCollisionEnter(Collision c)
		{
			base.OnCollisionEnter(c);

			bounceCount++;

			if(bounceCount >= 3)
				Explode();
		}

		#endregion

		public override void Reinstantiate()
		{
			base.Reinstantiate();

			bounceCount = 0;

			if(initBounciness >= 0f)
				SetBounciness(initBounciness);
		}

		//

		public override void Fire(GrenadeLauncherBase grenadeLauncher, Vector3 direction, RobotEmil parentRobot, float damageMultiplier, float radiusMultiplier, float grenadeExplosionSpeedUp, double timestamp, bool setTriggeredState = true)
		{
			base.Fire(grenadeLauncher, direction, parentRobot, damageMultiplier, radiusMultiplier, grenadeExplosionSpeedUp, timestamp, setTriggeredState);

			var dispatcher = arenaEventDispatcher.madnessMode.GetMadnessStepDispatcher<Madness.BouncyBounce_MadnessModeImpl>(MadnessStepType.BouncyBounce);

			if(dispatcher != null)
			{
				float bounciness = dispatcher.bounciness;

				if(bounciness >= 0f)
					SetBounciness(bounciness);
			}
		}

		public override void Explode()
		{
			base.Explode();

			if(parentRobot != null)
			{
				parentRobot.OnBouncyGrenadeExploded(this);
			}
		}

		//

		public void SetBounciness(float bounciness)
		{
			bounciness = Mathf.Clamp01(bounciness);

			if(collider == null)
			{
				Debug.LogError("Failed to Set Bounciness - collider == null");
				return;
			}

			var physicsMaterial = collider.material;

			if(Mathf.Approximately(bounciness, physicsMaterial.bounciness))
				return;

			if(initBounciness < 0f)
			{
				initBounciness = physicsMaterial.bounciness;
			}

			Debug.Log("Setting bounciness for grenade " + name + " - " + bounciness);

			physicsMaterial.bounciness = bounciness;
		}
	}

}
