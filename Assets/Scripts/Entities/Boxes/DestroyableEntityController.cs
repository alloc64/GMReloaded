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
using System.Linq;
using GMReloaded.Bonuses;

namespace GMReloaded.Entities
{
	[System.Serializable]
	public class DestroyableEntityRefEntry
	{
		public EntityType entityType;
		public DestroyableEntity entityRef;
	}

	[System.Serializable]
	public class BonusPrefabRefEntry
	{
		public Bonus.Type bonusType;
		public BonusPickable bonusRef;
	}

	public class DestroyableEntityController : MonoSingleton<DestroyableEntityController>
	{
		[SerializeField]
		private DestroyableEntityRefEntry[] destroyableEntitiesList;

		private Dictionary<EntityType, DestroyableEntity> _destroyableEntitiesCache = new Dictionary<EntityType, DestroyableEntity>();

		[SerializeField]
		private BonusPrefabRefEntry [] bonusPrefabs;
		//

		private Dictionary<EntityType, PrefabsRecyclerBase<DestroyableEntity>> entityRecyclers = new Dictionary<EntityType, PrefabsRecyclerBase<DestroyableEntity>>();

		//

		private Dictionary<Bonus.Type, PrefabsRecyclerBase<BonusPickable>> bonusPickableRecyclers = new Dictionary<Bonus.Type, PrefabsRecyclerBase<BonusPickable>>();

		//

		private Dictionary<short, DestroyableEntity> entityInstances = new Dictionary<short, DestroyableEntity>();

		//

		#region Unity

		private void Awake()
		{
			CacheDestroyableEntities();

			// krabice

			PreinstantiatePrefabOfType(EntityType.BoxDestroyable, 10);

			// explozivni barely

			PreinstantiatePrefabOfType(EntityType.ExplosiveBarrel, 10);

			// bonusy

			foreach(var bp in bonusPrefabs) 
			{
				if(bp == null || bp.bonusRef == null)
					continue;

				var recycler0 = new PrefabsRecyclerBase<BonusPickable>(bp.bonusRef, transform);
				recycler0.Preinstantiate(20);

				bonusPickableRecyclers[bp.bonusType] = recycler0;
			}
		}

		#endregion

		#region Entity Cache

		private void CacheDestroyableEntities()
		{
			foreach(var deRef in destroyableEntitiesList)
			{
				if(deRef != null)
				{
					_destroyableEntitiesCache[deRef.entityType] = deRef.entityRef;
				}
			}
		}

		private DestroyableEntity GetDestroyableEntityPrefab(EntityType entityType)
		{
			DestroyableEntity destroyableEntity = null;

			_destroyableEntitiesCache.TryGetValue(entityType, out destroyableEntity);

			if(destroyableEntity == null)
				Debug.LogWarning("Failed to get DestroyableEntity prefab of " + entityType);

			return destroyableEntity;
		}

		private T GetDestroyableEntityPrefab<T>(EntityType entityType) where T : DestroyableEntity
		{
			return GetDestroyableEntityPrefab(entityType) as T;
		}

		private void PreinstantiatePrefabOfType(EntityType entityType, int count)
		{
			var prefab = GetDestroyableEntityPrefab(entityType);

			var recycler = new PrefabsRecyclerBase<DestroyableEntity>(prefab, transform);
			recycler.Preinstantiate(count);

			entityRecyclers[entityType] = recycler;
		}

		#endregion

		#region Entity Management

		//TODO: tady by se to mohlo i odregistrovavat
		public void RegisterEntity(short entityId, DestroyableEntity entity)
		{
			if(entity != null)
				entityInstances[entityId] = entity;
		}

		public T GetDestroyableEntity<T>(short entityId) where T : DestroyableEntity
		{
			DestroyableEntity entity;
			entityInstances.TryGetValue(entityId, out entity);

			return entity as T;
		}

		public BoxDestroyable GetDestroyableEntity(short boxId)
		{
			return GetDestroyableEntity<BoxDestroyable>(boxId);
		}

		public DestroyableEntity DequeueDestroyableEntity(EntityType entityType)
		{
			PrefabsRecyclerBase<DestroyableEntity> recycler = null;

			entityRecyclers.TryGetValue(entityType, out recycler);

			if(recycler != null)
			{
				return recycler.Dequeue();
			}
			else
			{
				Debug.LogWarning("RecycleDestroyableEntity recycler == null for" + entityType);
				return null;
			}
		}

		public void RecycleDestroyableEntity(EntityType entityType, DestroyableEntity entity)
		{
			PrefabsRecyclerBase<DestroyableEntity> recycler = null;

			entityRecyclers.TryGetValue(entityType, out recycler);

			if(recycler != null)
				recycler.Enqueue(entity);
			else
				Debug.LogWarning("RecycleDestroyableEntity recycler == null for " + entityType);
		}

		#endregion

		#region Bonuses

		public BonusPickable DequeueBonusPickable(Bonus.Type bonusType)
		{
			PrefabsRecyclerBase<BonusPickable> recycler = null;
			bonusPickableRecyclers.TryGetValue(bonusType, out recycler);

			if(recycler != null)
				return recycler.Dequeue();

			Debug.LogWarning("DequeueBonusPickable recycler == null for "+ bonusType);

			return null;
		}

		public void RecycleBonusPickable(BonusPickable bonusPickable)
		{
			if(bonusPickable == null)
				return;

			PopBonusBehaviour(bonusPickable.bonusBehaviour);

			PrefabsRecyclerBase<BonusPickable> recycler = null;
			bonusPickableRecyclers.TryGetValue(bonusPickable.bonusType, out recycler);

			if(recycler != null)
				recycler.Enqueue(bonusPickable);
			else
				Debug.LogWarning("DequeueBonusPickable recycler == null for "+ bonusPickable.bonusType);
		}

		// bonus behaviours

		private List<Bonus.Behaviour> bonusBehaviourDistributed;
		private List<Config.Bonuses.Distribution> flatProbabilityBonusDistribution = new List<Config.Bonuses.Distribution>();

		private void RegenerateBonusDistribution()
		{
			if(bonusBehaviourDistributed == null)
				bonusBehaviourDistributed = new List<Bonus.Behaviour>();
			else
				bonusBehaviourDistributed.Clear();

			//

			for(int i = 0; i < Config.Bonuses.bonusesDistribution.Length; i++)
			{
				var bd = Config.Bonuses.bonusesDistribution[i];
				int probability = bd.probabilityInt;

				if(probability > 0)
				{
					for(int j = 0; j < probability; j++)
					{
						bonusBehaviourDistributed.Add(bd.bonusBehaviour);
					}
				}
				else
				{
					flatProbabilityBonusDistribution.Add(bd);
				}
			}

			//for(int k = 0; k < bonusBehaviourDistributed.Count; k++)
			//	Debug.Log("bb " + bonusBehaviourDistributed[k]);
		}

		public Bonus.Behaviour PeekBonusBehaviour()
		{
			if(!PhotonNetwork.isMasterClient)
				return Bonus.Behaviour.None;
			
			Bonus.Behaviour selectedBonus = Bonus.Behaviour.None;

			int entitiesCount = entityInstances.Count;

			if(PRDRandom.Instance.Success(Config.Bonuses.bonusPercentage))
			{
				if(Config.Bonuses.forcedBonus == Bonus.Behaviour.None)
				{
					if(bonusBehaviourDistributed == null)
						RegenerateBonusDistribution();

					//

					bool flatBonusGenerated = false;

					for(int i = 0; i < flatProbabilityBonusDistribution.Count; i++)
					{
						var bd = flatProbabilityBonusDistribution[i];

						if(PRDRandom.Instance.Success(bd.bonusPercentage))
						{
							selectedBonus = bd.bonusBehaviour;

							flatBonusGenerated = true;
							break;
						}
					}

					//

					if(!flatBonusGenerated)
						selectedBonus = bonusBehaviourDistributed.GetRandom();
				}
				else
				{
					selectedBonus = Config.Bonuses.forcedBonus;
				}
			}

			return selectedBonus;
		}

		private void PopBonusBehaviour(Bonus.Behaviour bonusBehaviour)
		{
			if(bonusBehaviour == Bonus.Behaviour.None || !PhotonNetwork.isMasterClient)
				return;

			// tato funkce zatim nic nedela, mozna in future
		}

		#endregion

		public void HitObjectsInRadius(IAttackerObject parent, float radius, float damage)
		{
			Vector3 thisPos = parent.position;

			Collider[] colliders = Physics.OverlapSphere(thisPos, radius, 1 << Layer.DestroyableEntity);

			foreach(var c in colliders)
			{
				var bd = c.GetComponent<DestroyableEntity>();

				if(bd == null)
				{
					var cc = c.GetComponent<ChildCollision>();

					if(cc != null)
						bd = cc.GetParentComponent<DestroyableEntity>();
				}

				if(bd != null)
				{
					Vector3 objPos = c.ClosestPointOnBounds(thisPos);

					RaycastHit hit;
					if(!Physics.Linecast(thisPos, objPos, out hit, (1 << Layer.Default | 1 << Layer.DestroyableEntity)))
					{
						float d = Vector3.Distance(thisPos, objPos);

						if(d <= radius)
						{
							float percentualDamage = 1f - Mathf.Clamp(d / radius, 0f, radius);

							bd.Hit(parent, percentualDamage, thisPos);
						}
					}
				}
			}
		}

		public void BurnAllDestroyableBoxes()
		{
			if(!PhotonNetwork.isMasterClient)
				return;

			ExitGames.Client.Photon.Hashtable values = new ExitGames.Client.Photon.Hashtable();

			foreach(var kvp in entityInstances)
			{
				var box = kvp.Value as BoxDestroyable;

				if(box == null)
					continue;
				
				box.Demolish();
				values.Add(kvp.Key, box.networkProperties);
			}

			RoomPropertiesController.Instance.SetBatch(values);
		}

		//

		public void GetVisibleObjectsInDistance(Vector3 objectPosition, float radius, ref List<IObjectWithPosition> objects)
		{
			if(objects == null)
				objects = new List<IObjectWithPosition>();

			foreach(var kvp in entityInstances)
			{
				var box = kvp.Value;

				if(box != null && box.state == BoxDestroyable.State.Idle && Vector3.Distance(objectPosition, box.position) <= radius)
				{
					RaycastHit h;
					bool hit = Physics.Linecast(objectPosition, box.position, out h, ((1 << Layer.DestroyableEntity) | (1 << Layer.Default)));

					BoxDestroyable b1 = null;

					if(h.collider != null)
						b1 = h.collider.GetComponent<BoxDestroyable>();

					if(!hit || (hit && b1 == box))
					{
						objects.Add(box);
					}
				}
			}
		}
	}
}