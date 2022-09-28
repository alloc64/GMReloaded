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
using GMReloaded.Bonuses;

namespace GMReloaded.Entities
{
	public class BonusPickable : IRecyclablePrefab<BonusPickable>, IAliveObject
	{
		public enum State : byte
		{
			Undefined,
			Create,
			Created,
			NormalPickUp,
			SilentPickUp,
			Destroyed,
			Denied,
		}

		[SerializeField]
		private Bonus.Type _bonusType;
		public Bonus.Type bonusType { get { return _bonusType; } }

		[SerializeField]
		public Bonus.Behaviour bonusBehaviour;

		[SerializeField]
		private new SphereCollider collider;

		[SerializeField]
		private float rotationSpeed;

		[SerializeField]
		private Transform model;

		[SerializeField]
		private Renderer iconsRenderer;

		[SerializeField]
		private ParticleSystem showParticleSystem;

		//

		public float damageRadius { get { return (collider != null) ? collider.radius : 0.1f; } }

		//

		public EntityContainer entityContainer { get; private set; }

		private Vector3 modelInitialLocalScale;

		private float rotationTimer = 0f;

		private Entities.BoxDestroyable _boxDestroyable;
		public Entities.BoxDestroyable boxDestroyable { get { return _boxDestroyable; } private set { _boxDestroyable = value; } }

		//

		private Bonuses.BonusController bonusController { get { return Bonuses.BonusController.Instance; } }

		private IAliveObjectsController aoc { get { return IAliveObjectsController.Instance; } }

		//

		private ISound pickUpSound;

		//

		#region Unity

		private void Awake()
		{
			modelInitialLocalScale = model.localScale;
		}

		private void Start()
		{
			pickUpSound = snd.Load(Config.Sounds.bonusPickUp);
		}

		private void Update()
		{
			rotationTimer += Time.deltaTime * rotationSpeed;

			if(rotationTimer >= 360f)
				rotationTimer = 0f;

			Vector3 lr = model.localEulerAngles;
			lr.z = rotationTimer;
			model.localEulerAngles = lr;
		}

		#endregion

		#region Setup

		public override void Reinstantiate()
		{
			SetActive(true);

			SetModelScale(Vector3.zero);

			collider.enabled = true;

			if(showParticleSystem != null)
				showParticleSystem.Stop();
		}

		public void Setup(EntityContainer entityContainer, Bonus.Behaviour bonusBehaviour)
		{
			if(entityContainer == null)
				return;

			this.entityContainer = entityContainer;

			//

			parent = entityContainer.transform;

			localRotation = Quaternion.identity;
			localPosition = Vector3.zero;
			localScale = Vector3.one;

			SetBonusBehaviour(bonusBehaviour);
		}

		private void SetBonusBehaviour(Bonus.Behaviour bonusBehaviour)
		{
			this.bonusBehaviour = bonusBehaviour;

			if(iconsRenderer != null)
			{
				var tex = bonusController.GetBonusIconTexture(bonusBehaviour);

				if(tex != null)
				{
					iconsRenderer.enabled = true;

					var material = iconsRenderer.material;

					material.SetTexture("_MainTex", tex);
					material.SetTexture("_MKGlowTex", tex);
				}
				else
				{
					iconsRenderer.enabled = false;
				}
			}
		}

		#endregion

		#region Hits

		public bool Hit(IAttackerObject attacker, float percentualDamage, float damage)
		{
			if(percentualDamage > 0.25f)
			{
				if(entityContainer != null)
					entityContainer.PickUpBonus(State.Destroyed);

				return true;
			}

			return false;
		}

		#endregion

		#region Visiblity

		public void Show()
		{
			//Debug.Log(entityContainer.name +  " BonusPickable Show", this);

			if(showParticleSystem != null)
				showParticleSystem.Play();

			Ease.Instance.Vector3(Vector3.zero, modelInitialLocalScale, 0.14f, EaseType.None, (ls) => SetModelScale(ls), () => 
			{
				if(aoc != null)
					aoc.Register(this);
			});
		}

		public virtual void PickUpBonus(State pickupType)
		{
			//Debug.Log(entityContainer.name + " BonusPickable PickUpBonus " + pickupType, this);

			if(pickupType == State.NormalPickUp && pickUpSound != null)
				pickUpSound.Play(transform);
			
			if(collider != null)
				collider.enabled = false;

			Ease.Instance.Vector3(modelInitialLocalScale, Vector3.zero, 0.14f, EaseType.None, (ls) => SetModelScale(ls), () => 
			{ 
				aoc.Unregister(this);

				if(entityContainer != null && entityContainer.bonusImpl != null)
				{
					entityContainer.bonusImpl.RecycleBonus();
				}
			});
		}

		private void SetModelScale(Vector3 scale)
		{
			if(model != null)
				model.localScale = scale;
		}

		#endregion
	}
}
