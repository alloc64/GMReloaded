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

namespace GMReloaded.Entities
{
	public class DestroyableEntityCreator : IHandWeaponFiringProjectilesWithBarrelReloadableMagazine
	{
		[SerializeField]
		private DestroyableEntityCreatorDisplay display;

		//

		private Entities.EntityContainer lastEntityContainer;

		private DestroyableEntityIndicator entityIndicator;

		private Dictionary<EntityType, DestroyableEntityIndicator> entityIndicators = new Dictionary<EntityType, DestroyableEntityIndicator>();

		private bool isUsed = false;

		protected EntityType entityType;

		#region Unity

		protected override void Awake()
		{
			base.Awake();
			LoadIndicators();
		}

		protected override void Update()
		{
			if(robotParent == null || robotParent.clientType == RobotEmil.ClientType.RemoteClient)
				return;

			if(isUsed)
			{
				base.Update();

				if(canGrabNewProjectile)
				{
					int mask = (1 << Layer.EntityContainerEmpty);

					var ray = robotParent.viewObserver.viewRay;

					Debug.DrawRay(ray.origin, ray.direction * 1000f);

					RaycastHit[] hits = Physics.RaycastAll(ray, 10f, mask);

					if(hits.Length > 0)
					{
						EntityContainer ec = null;

						var robotPosition = robotParent.position;
						float distance = Mathf.Infinity;

						foreach(var rh in hits)
						{
							var currEntityContainer = rh.collider.GetComponent<EntityContainer>();

							if(currEntityContainer == null)
								continue;

							float d = Vector3.Distance(currEntityContainer.position, robotPosition);

							if(d > 1.5f && d < distance)
							{

								RaycastHit linecastHit;
								bool hit = Physics.Linecast(barrel.position, currEntityContainer.position, out linecastHit);

								if(hit)
								{
									ec = linecastHit.collider.GetComponent<EntityContainer>();

									if(ec == null)
										continue;

									if(ec != null && ec.isOccupied)
									{
										ec = null;
										continue;
									}

									distance = d;
								}
							}
						}

						if(ec != null && !ec.isOccupied)
						{
							if(ec != lastEntityContainer && robotParent.clientType != RobotEmil.ClientType.BotClient)
							{
								lastEntityContainer = ec;

								if(entityIndicator != null)
									entityIndicator.SetParent(ec);
							}
						}
						else
						{
							DeactivateIndicator();
						}
					}
				}
				else
				{
					DeactivateIndicator();
				}
			}
		}

		#endregion

		#region implemented abstract members of IHandWeaponObject

		public override void PrepareForAttack(RobotEmil robotParent, RobotEmil.AttackType attackType)
		{
		}

		public override int Attack(RobotEmil robotParent, RobotEmil.AttackType attackType, double timestamp, int projectileHashId)
		{
			if(robotParent == null || robotParent.clientType == RobotEmil.ClientType.RemoteClient)
				return -1;

			if(entityType == EntityType.None)
			{
				Debug.LogWarning("DestroyableEntityCreator entityType not set");
				return -1;	
			}

			if(lastEntityContainer == null)
			{
				OnNoAmmo();

				Debug.Log("Unable create DestroyableEntity at unknown place");
				return -1;
			}

			if(!canGrabNewProjectile)
			{
				OnNoAmmo();
				return -1;
			}

			OnDequeueMagazineProjectile();

			return 0;
		}

		protected override void OnDequeueMagazineProjectile()
		{
			lastEntityContainer.Create(entityType);

			if(robotParent != null)
			{
				switch(entityType)
				{
					case EntityType.BoxDestroyable:
						robotParent.OnDestroyableBoxCreatedWithBoxCreator();
					break;	
				}
			}

			DeactivateIndicator();

			OnFireProjectileSound();

			base.OnDequeueMagazineProjectile();
		}

		#endregion

		#region RobotEmil interfaces

		public override void GrabToHand(IAnimatorMonoBehaviour robotParent, Transform hand)
		{
			base.GrabToHand(robotParent, hand);

			robotParent.SetAnimatorLayerWeight(AnimatorLayer.GrenadeLauncher_Hands, 1f);
			robotParent.SetAnimatorLayerWeight(AnimatorLayer.SpineBend, 1f);

			DeactivateIndicator();

			isUsed = true;
		}

		public override void OnRemoveFromHand(IAnimatorMonoBehaviour robotParent)
		{
			robotParent.SetAnimatorLayerWeight(AnimatorLayer.GrenadeLauncher_Hands, 0f);
			robotParent.SetAnimatorLayerWeight(AnimatorLayer.SpineBend, 0f);

			isUsed = false;

			DeactivateIndicator();
		}

		public override void OnRobotDeath()
		{
			base.OnRobotDeath();

			isUsed = false;

			DeactivateIndicator();

			reloadTimer = 0f;
		}

		#endregion

		#region Destroyable Entity Creator

		protected void LoadIndicators(params EntityType[] entityTypes)
		{
			foreach(var entityType in entityTypes)
			{
				var ei = GetEntityIndicator(entityType);

				if(ei == null && entityType != EntityType.None)
				{
					if(entityType == EntityType.BoxDestroyable)
					{
						ei = Prefabs.Load<BoxDestroyableIndicator>("Prefabs/Props/Indicators/BoxDestroyableIndicator");
					}
					else
					{
						var destroyableEntity = DestroyableEntityController.Instance.DequeueDestroyableEntity(entityType);

						if(destroyableEntity == null)
							return;

						ei = destroyableEntity.MakeIndicator();
					}

					if(ei != null)
					{
						entityIndicators[entityType] = ei;

						if(ei != null)
						{
							ei.SetDefaultParent(transform);
							ei.SetParent(null);
						}
					}
				}
			}
		}
	
		protected void SetEntityType(EntityType entityType)
		{
			if(this.entityType == entityType)
				return;

			DeactivateIndicator();

			this.entityType = entityType;

			//

			entityIndicator = GetEntityIndicator(entityType);

			if(display != null)
				display.SetEntityType(entityType);
		}

		private DestroyableEntityIndicator GetEntityIndicator(EntityType entityType)
		{
			DestroyableEntityIndicator ind = null;

			entityIndicators.TryGetValue(entityType, out ind);

			return ind;
		}

		private void DeactivateIndicator()
		{
			if(robotParent == null)
				return;

			lastEntityContainer = null;

			if(entityIndicator != null)
				entityIndicator.SetParent(null);
		}

		#endregion


		#region Sounds

		protected override void OnLoadSounds()
		{
			base.OnLoadSounds();

			fireProjectileSound = snd.Load(Config.Sounds.boxCreatorFireProjectile);
		}

		#endregion
	}
}