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

using GMReloaded.Entities;

namespace GMReloaded
{
	public class StickableGrenade : GrenadeBase
	{
		private bool alreadySticked = false;

		//

		private DestroyableEntity stickedDestroyableEntity;

		//

		#region Unity

		protected override void Start()
		{
			base.Start();

			var _explosionSound = new SoundContainer("SNDC_Grenade_Explosion");

			for(int i = 0; i < 4; i++)
				_explosionSound.AddSound(Config.Sounds.grenadeExplosion + i);

			SetExplosionSound(_explosionSound);
		}

		//

		protected override void OnCollisionEnter(Collision c)
		{
			base.OnCollisionEnter(c);

			if(alreadySticked)
				return;

			var de = c.collider.GetComponent<DestroyableEntity>();

			if(de == null)
			{
				var cc = c.collider.GetComponent<ChildCollision>();

				if(cc != null)
					de = cc.GetParentComponent<DestroyableEntity>();
			}

			if(de != null)
			{
				stickedDestroyableEntity = de;
				stickedDestroyableEntity.OnBeingRuinedEvent += OnParentBurnableObjectBurning;

				if(parentRobot != null)
					parentRobot.OnStickableGrenadeStickedOnDestroyableBox(this);
			}
			else
			{
				var g = c.collider.GetComponent<GrenadeBase>();

				if(g != null)
					return;
			}

			Stick(c);
		}

		#endregion

		public override void Reinstantiate()
		{
			base.Reinstantiate();

			alreadySticked = false;
		}

		private void RemoveBoxDestroyCallback()
		{
			if(stickedDestroyableEntity != null)
			{
				stickedDestroyableEntity.OnBeingRuinedEvent -= OnParentBurnableObjectBurning;
				stickedDestroyableEntity = null;
			}
		}

		protected virtual void Stick(Collision c)
		{
			SetKinematic(true);
			alreadySticked = true;
		}

		protected virtual void Unstick()
		{
			SetKinematic(false);
			rigidbody.WakeUp();
		}

		public override void Explode()
		{
			base.Explode();
			RemoveBoxDestroyCallback();
		}

		private void OnParentBurnableObjectBurning()
		{
			if(state != State.Exploded)
			{
				Unstick();
			}

			RemoveBoxDestroyCallback();
		}
	}
	
}
