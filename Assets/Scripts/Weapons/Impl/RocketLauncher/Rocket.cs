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
using System.Collections.Generic;

namespace GMReloaded
{
	public class Rocket : IProjectileObjectWithExplosion
	{
		private Vector3 fireForce;

		[SerializeField]
		private float flyForceMultiplier = 1000f;

		[SerializeField]
		private float impulseForceMultiplier = 4f;

		[SerializeField]
		private ParticleSystem jetFireParticle;

		private ISound jetFireSound;

		protected virtual void Start()
		{
			var _explosionSound = new SoundContainer("SNDC_Rocket_Explosion");

			for(int i = 0; i < 4; i++)
				_explosionSound.AddSound(Config.Sounds.grenadeExplosion + i);

			SetExplosionSound(_explosionSound);

			jetFireSound = snd.Load(Config.Sounds.rocketJetFireSound);
		}

		protected virtual void FixedUpdate()
		{
			switch(state)
			{
				case State.Triggered:
					FixedUpdateStateTriggered();
				break;
			}
		}

		protected virtual void OnCollisionEnter(Collision collision)
		{
			if(state == State.Triggered)
			{
				Explode();
			}
		}

		public void Fire(RocketLauncher rocketLauncher, RobotEmil parentRobot, Vector3 force)
		{
			base.Fire(rocketLauncher, parentRobot);

			this.fireForce = force;

			ResetParent();

			SetKinematic(false);
			rigidbody.AddForce(force * impulseForceMultiplier, ForceMode.Impulse);

			SetState(State.Triggered);
		}

		protected virtual void FixedUpdateStateTriggered()
		{
			rigidbody.AddForce(fireForce * flyForceMultiplier, ForceMode.Force);
		}

		protected override void OnSetStateTriggered()
		{
			if(jetFireParticle != null)
				jetFireParticle.Play();
			
			if(jetFireSound != null)
				jetFireSound.Play(transform);
		}

		protected override void OnSetStateExploded()
		{
			base.OnSetStateExploded();

			if(jetFireParticle != null)
				jetFireParticle.Stop();

			if(jetFireSound != null)
				jetFireSound.Stop();
			
			entityController.HitObjectsInRadius(this, explosionRadius, explosionDamage, hitFlags);
		}
	}
	
}