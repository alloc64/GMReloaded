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
using System.Collections.Generic;
using System;
using System.Collections;

namespace GMReloaded
{	
	public class GrenadeBase : IProjectileObjectWithExplosion
	{
		public enum LightState : byte
		{
			Light,
			None
		}

		public enum Visibility : byte
		{
			Normal,
			Masked
		}

		[SerializeField]
		protected Light pointLight;

		private float initPointLightRange = -1f;

		//

		[SerializeField]
		protected LightState lightState;

		protected float blinkTimer = 0f;

		[SerializeField]
		public float threwTime;

		[SerializeField]
		protected new Renderer renderer;

		[SerializeField]
		protected Visibility visibility;

		[SerializeField]
		protected Material normalVisiblityMaterial;

		[SerializeField]
		protected Material maskedVisiblityMaterial;

		[SerializeField]
		public float explosionDelay = 4f;

		protected float explosionTimer = 0f;

		//

		protected float damageMultiplier = 1f;


		[SerializeField]
		protected ParticleSystem grenadeFireParticles; 

		[SerializeField]
		protected Light explosionLight;

		[SerializeField]
		private float flashLightTime = 0.15f;

		[SerializeField]
		private float finalLightIntensity = 0f;

		private float baseLightIntensity;

		//Bouncy - 0, 116, 10
		//Flash - 255, 174, 0
		//Smoke - 0,0,0
		//Sticky - 255, 0, 0
		//Tesla - 0, 79, 255
		//StickMine - 255, 0, 0

		protected ProjectileType weaponType;

		private ISound beepSound;
		private ISound bounceSound;

		#region Unity

		protected override void Awake()
		{
			base.Awake();
			Assert.IsAssigned(rigidbody);
			Assert.IsAssigned(renderer);

			Assert.IsAssigned(explosion);
			Assert.IsAssigned(explosionLight);
			Assert.IsAssigned(objectModel);

			ProjectileRecycler.GetPrefabWeaponType(this, out weaponType);

			if(pointLight != null)
				pointLight.enabled = false;
		
			SetExplosionLightActive(false);
			baseLightIntensity = explosionLight.intensity;

			SetVisiblity(visibility);
		}


		protected virtual void Start()
		{
			beepSound = snd.Load(Config.Sounds.grenadeBeep);
		}


		protected virtual void OnDrawGizmosSelected()
		{
		}

		protected virtual void Update()
		{
			switch(state)
			{
				case State.Idle:
				break;

				case State.Triggered:
					UpdateStateTriggered();
				break;

				case State.Exploded:
				break;
			}

			if(threwTime > 0f)
			{
				UpdateScaleUp((Time.time - threwTime) * 8f);
			}

		}

		protected float lastCollisionNormalPacked;

		protected virtual void OnCollisionEnter(Collision c)
		{
			var vel = rigidbody.velocity;

			var cp = c.contacts[0];

			lastCollisionNormalPacked = PackExtensions.PackToFloat(cp.normal);

			if(vel.y < 0.5f)
				return;

			//if(Time.time > nextBounceTime)
			//{

			if(bounceSound != null)
				bounceSound.Play(transform);

			//	nextBounceTime = Time.time + 0.2f;
			//}
		}

		protected virtual void UpdateStateTriggered()
		{
			if(explosionTimer < explosionDelay)
			{
				explosionTimer += Time.deltaTime;

				float blinkSpeed = 3f + (explosionTimer / explosionDelay) * 8f;

				blinkTimer += Time.deltaTime * blinkSpeed;
				if(blinkTimer >= 1f)
				{
					OnBlink();
					blinkTimer = 0f;
				}
			}

			if(explosionTimer >= explosionDelay)
			{
				Explode();
			}
		}

		#endregion

		public override void Reinstantiate()
		{
			base.Reinstantiate();

			SetExplosionLightActive(false);
			SetGrenadeFireParticlesActive(false);

			Scale(1f);

			if(pointLight != null)
			{
				pointLight.enabled = false;

				if(initPointLightRange < 0f)
					initPointLightRange = pointLight.range;

				SetLightRange(initPointLightRange);
			}
			
			assignedId = 0;
			damageMultiplier = 1f;
			threwTime = 0f;
		}

		private float grenadeExplosionSpeedUp = 0f;

		public virtual void Fire(GrenadeLauncherBase grenadeLauncher, Vector3 direction, RobotEmil parentRobot, float damageMultiplier, float radiusMultiplier, float grenadeExplosionSpeedUp, double timestamp, bool setTriggeredState = true)
		{
			base.Fire(grenadeLauncher, parentRobot);

			Fire(direction, parentRobot, damageMultiplier, radiusMultiplier, grenadeExplosionSpeedUp, timestamp, setTriggeredState);
		}

		public virtual void Fire(Config.Weapons.WeaponConfig weaponConfig, Vector3 direction, RobotEmil parentRobot, float damageMultiplier, float radiusMultiplier, float grenadeExplosionSpeedUp, double timestamp, bool setTriggeredState = true)
		{
			base.Fire(weaponConfig);

			Fire(direction, parentRobot, damageMultiplier, radiusMultiplier, grenadeExplosionSpeedUp, timestamp, setTriggeredState);
		}

		public virtual void Fire(Vector3 direction, RobotEmil parentRobot, float damageMultiplier, float radiusMultiplier, float grenadeExplosionSpeedUp, double timestamp, bool setTriggeredState = true)
		{
			this.threwTime = Time.time;

			ResetParent();

			SetKinematic(false);
			rigidbody.AddForce(direction, ForceMode.Impulse);
			rigidbody.AddRelativeTorque(new Vector3(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f)) * 1f);

			SetDamageMultiplier(damageMultiplier);
			SetExplosionRadiusMultiplier(radiusMultiplier);

			this.grenadeExplosionSpeedUp = grenadeExplosionSpeedUp;

			if(setTriggeredState)
				SetState(State.Triggered);

			if(timestamp > 0f)
			{
				float diff = (float)(PhotonNetwork.time - timestamp);
				explosionTimer += diff;

				Debug.Log("Fire setDispatch " + diff + " / " + explosionTimer);
			}

			eoc.Register(this);
		}

		public bool IsVisible(Camera camera)
		{
			return renderer != null && renderer.IsVisibleFrom(camera);
		}

		/*
		public virtual void ThrowOnNetwork(RobotEmil parentRobot, byte id, Vector3 startPosition)
		{
			//TODO: ThrowOnNetwork
			/*Throw(parentRobot, id, 1f, null, null, false);

			SetKinematic(true);
			SetCollisionsActive(false);

		}*/

		#region Blinking + Beeping + Scale

		private bool blink = false;

		private void OnBlink()
		{
			blink = !blink;

			switch(lightState)
			{
				case LightState.Light:
					if(pointLight != null)
						pointLight.enabled = !pointLight.enabled;
					
					Beep(blink);
				break;

				case LightState.None:
				break;
			}


		}

		private void Beep(bool beep)
		{
			if(beepSound != null)
			{
				if(beep)
					beepSound.Play(transform);
				else
					beepSound.Stop();
			}
		}

		protected virtual void UpdateScaleUp(float scale)
		{
			if(scale <= 2f)
				Scale(scale);
		}

		private void Scale(float uniformScale)
		{
			uniformScale = Mathf.Clamp(uniformScale, 1f, 2.5f);
			
			localScale = Vector3.one * uniformScale;

			if(initPointLightRange > 0f)
				SetLightRange(initPointLightRange * uniformScale * 1.25f);
		}

		private void SetLightRange(float range)
		{
			if(pointLight == null)
				return;

			//Debug.Log("SetLightRange " + range);

			pointLight.range = range;
		}

		#endregion

		#region Explosion Flash

		private void SetExplosionLightActive(bool active)
		{
			if(active)
				explosionLight.intensity = baseLightIntensity;
			
			explosionLight.enabled = active;
		}

		private IEnumerator FlashLightCoroutine()
		{
			SetExplosionLightActive(true);

			float t = 0;
			while(t < flashLightTime)
			{
				t += Time.deltaTime;

				explosionLight.intensity = Mathf.Lerp(baseLightIntensity, finalLightIntensity, t / flashLightTime);
				yield return null;
			}

			SetExplosionLightActive(false);
		}

		#endregion

		public virtual void SetCustomState(byte customState)
		{
			
		}

		protected override void OnSetStateTriggered()
		{
			base.OnSetStateTriggered();

			explosionTimer = 0f + grenadeExplosionSpeedUp;
			blinkTimer = 0f;
			grenadeExplosionSpeedUp = 0f;
		}

		protected override void OnSetStateExploded()
		{
			explosionTimer = explosionDelay;
			base.OnSetStateExploded();

			Beep(false);

			if(explosionLight != null)
				StartCoroutine(FlashLightCoroutine());

			SetGrenadeFireParticlesActive(false);

			/*if(explosionDecalType != Decal.Type.None)
			{
				/*List<Decal.Type> flags = new List<Decal.Type>();

				foreach(var f in explosionDecalType.GetUniqueFlags())
					flags.Add((Decal.Type)f);
				

				var randDecal = flags[UnityEngine.Random.Range(0, flags.Count)];
				//TODO: vymyslet fungujici random

				decalManager.CreateDecal(Decal.Type.Explosion0, PackExtensions.UnPackFloat(lastCollisionNormalPacked), position);
			}*/

			entityController.HitObjectsInRadius(this, explosionRadius, explosionDamage * damageMultiplier, hitFlags);

			eoc.Unregister(this);
		}

		protected void SetBounceSound(string id)
		{
			bounceSound = snd.Load(id);
		}

		private void SetDamageMultiplier(float damageMultiplier)
		{
			this.damageMultiplier = Mathf.Clamp(damageMultiplier, 1f, 1.5f);

			SetGrenadeFireParticlesActive(this.damageMultiplier > 1f);
		}

		protected void SetGrenadeFireParticlesActive(bool active)
		{
			if(grenadeFireParticles != null)
			{
				if(active)
					grenadeFireParticles.PlayWithClear();
				else
					grenadeFireParticles.Stop();
			}
		}

		public void SetCollisioNormalPacked(float n)
		{
			this.lastCollisionNormalPacked = n;
		}

		public virtual void SetMasked(bool masked)
		{
			SetVisiblity(masked ? Visibility.Masked : Visibility.Normal);
		}

		public void SetVisiblity(Visibility visibility)
		{
			this.visibility = visibility;

			switch(visibility)
			{
				case Visibility.Normal:
					//renderer.material = normalVisiblityMaterial;
					lightState = LightState.Light;
				break;

				case Visibility.Masked:
					//renderer.material = maskedVisiblityMaterial;
					lightState = LightState.None;
				break;
			}
		}
	
	}
}
