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
	public class StickyLaserMine : StickableGrenade
	{
		public enum LaserState : byte
		{
			Inactive,
			Active,
		}

		public enum MineState
		{
			Idle,
			Sticked
		}

		[SerializeField]
		private Vector3 _laserDirection = new Vector3(-1f, 0f, 0f);

		private Vector3 laserDirection { get { return localRotation * _laserDirection; } }

		[SerializeField]
		protected LineRenderer laser;

		[SerializeField]
		protected Color laserStartColor;

		[SerializeField]
		protected Color laserEndColor;

		private Transform laserTransform;


		[SerializeField]
		protected LaserState laserState;

		[SerializeField]
		protected MineState mineState;

		[SerializeField]
		private float activationTime = 2f;

		private ISound stickSound;

		protected override void Awake()
		{
			base.Awake();

			Assert.IsAssigned(laser);

			laserTransform = laser.transform;
		}

		protected override void Start()
		{
			base.Start();

			stickSound = snd.Load(Config.Sounds.stickyMineStick);
		}

		protected override void OnDrawGizmosSelected()
		{
			base.OnDrawGizmosSelected();
		

			Gizmos.color = Color.green;
			Gizmos.DrawLine(laser.transform.position, laser.transform.position + laserDirection * 1f);
		}

		private float lastCollisionCheckTime = 0f;
		private float stickTime = 0f;

		protected override void Update()
		{
			base.Update();

			UpdateLaser();
		}

		protected override void UpdateScaleUp(float scale)
		{
			
		}

		#region Laser

		private void UpdateLaser()
		{
			if(laserState == LaserState.Inactive)
				return;
		
			Vector3 rayStart = laserTransform.position;

			RaycastHit hit;
			if(stickTime > 0f && (Time.time - stickTime) > activationTime && Physics.Raycast(rayStart, laserDirection, out hit, Mathf.Infinity, ~((1 << Layer.EntityContainerEmpty) | (1 << Layer.EntityContainerOccupied))))
			{
				if(Time.time > lastCollisionCheckTime)
				{
					Collider c = hit.collider;

					GameObject go = c.gameObject;

					if(go != null)
					{
						int layer = go.layer;

						if(layer == Layer.Player || layer == Layer.Ragdoll)
						{
							Explode();
						}
					}

					lastCollisionCheckTime = Time.time + 0.1f;
				}

				SetLaserPoints(rayStart, hit.point);
			}
			else
			{
				// schovam laser
				HideLaser();
			}
		}
	
		private void SetLaserPoints(Vector3 start, Vector3 end)
		{
			if(laser == null)
				return;

			laser.SetPosition(0, start);
			laser.SetPosition(1, end);
		}

		private void HideLaser()
		{
			Vector3 rayStart = laserTransform.position;

			SetLaserPoints(rayStart, rayStart);
		}

		#endregion

		protected override void UpdateStateTriggered()
		{
			
		}

		protected override void OnSetStateExploded()
		{
			base.OnSetStateExploded();

			SetLaserState(LaserState.Inactive);
		}

		public override void Reinstantiate()
		{
			base.Reinstantiate();

			HideLaser();

			SetMineState(MineState.Idle);

			stickTime = 0f;
			SetLaserState(LaserState.Inactive);
		}

		protected override void Stick(Collision c)
		{
			base.Stick(c);

			Quaternion q = Quaternion.LookRotation(c.contacts[0].normal);
			var r = q.eulerAngles;
			r.y += 90f;
			q.eulerAngles = r;

			rigidbody.rotation = q;

			SetMineState(MineState.Sticked);
		}

		public override void SetCustomState(byte customState)
		{
			SetMineState((MineState)customState);
		}

		private void SetMineState(MineState mineState)
		{
			if(this.mineState == mineState)
				return;

			this.mineState = mineState;

			switch(mineState)
			{
				case MineState.Idle:
				break;
					
				case MineState.Sticked:

					if(stickSound != null)
						stickSound.Play(transform);

					stickTime = Time.time;
					SetLaserState(LaserState.Active);

				break;
			}
		}

		private void SetLaserState(LaserState laserState)
		{
			this.laserState = laserState;
		}

		public override void SetMasked(bool masked)
		{
			base.SetMasked(masked);

			SetLaserAlpha(masked ? 0.5f : 1f);
		}

		protected override void SetModelActive(bool active)
		{
			base.SetModelActive(active);

			if(laser != null)
			{
				laser.enabled = active;

				if(!active)
					HideLaser();
			}
		}

		public override bool Hit(IAttackerObject attacker, float percentualDamage, float damage)
		{
			if(state == State.Exploded || percentualDamage < 0.3f)
				return false;
			
			Explode();

			return true;
		}

		private void SetLaserColors(Color start, Color end)
		{
			if(laser == null)
				return;

			this.laserStartColor = start;
			this.laserEndColor = end;

			laser.SetColors(start, end);
		}

		private void SetLaserAlpha(float a)
		{
			if(laser == null)
				return;

			Color c0 = laserStartColor;
			c0.a = a;

			Color c1 = laserEndColor;
			c1.a = a;

			SetLaserColors(c0, c1);
		}
	}
	
}
