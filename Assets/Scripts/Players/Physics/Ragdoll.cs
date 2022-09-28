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
	public interface IRagdollExplosiveInfluencer : IObjectWithPosition
	{
		// exploze
		float explosionForceRadius { get; }

		float explosionForce { get; }

	}

	public interface IRagdollDirectionalInfluencer : IObjectWithPosition
	{
		// one time directional hit
		Vector3 hitForce { get; }
	}

	public class Ragdoll : MonoBehaviourTO
	{
		[SerializeField]
		private Animator animator;

		[SerializeField]
		private Rigidbody [] rigidbodies;

		[SerializeField]
		private Collider [] colliders;

		[SerializeField]
		private Transform [] transforms;

		[SerializeField]
		private CharacterController characterControllerDisabledOnUse;

		private Dictionary<string, Vector3> originalPositions = new Dictionary<string, Vector3>();
		private Dictionary<string, Quaternion> originalRotations = new Dictionary<string, Quaternion>(); 

		[SerializeField]
		private bool use = false;

		private bool oldUseFlag = true;

		private void Awake()
		{
			rigidbodies = GetComponentsInChildren<Rigidbody>(false);
			colliders = GetComponentsInChildren<Collider>(false);

			transforms = GetComponentsInChildren<Transform>(false);

			foreach(var t in transforms)
			{
				if(t != null && t.name.Contains("Bip01"))
				{
					originalPositions[t.name] = t.localPosition;
					originalRotations[t.name] = t.localRotation;
				}
			}
		}

		#if UNITY_EDITOR

		private void Update()
		{
			if(oldUseFlag != use)
			{
				SetRagdollActive(use);
				oldUseFlag = use;
			}
		}

		#endif

		public void SetRagdollActive(bool active)
		{
			this.use = active;

			if(animator != null)
				animator.enabled = !active;
				
			foreach(var r in rigidbodies)
			{
				if(r != null)
					r.isKinematic = !active;
			}

			foreach(var c in colliders)
			{
				if(c != null)
				{
					c.isTrigger = !active;
				}
			}

			if(characterControllerDisabledOnUse != null)
				characterControllerDisabledOnUse.enabled = !active;

			ResetToBasePose();
		}

		private void ResetToBasePose()
		{
			foreach(var t in transforms)
			{
				if(t != null)
				{
					Vector3 pos;

					if(originalPositions.TryGetValue(t.name, out pos))
						t.localPosition = pos;

					Quaternion quat;

					if(originalRotations.TryGetValue(t.name, out quat))
						t.localRotation = quat;
				}
			}
		}

		private void AddExplosionForce(float explosionForce, Vector3 explosionPosition, float explosionRadius)
		{
			if(explosionForce <= 0f || explosionRadius <= 0f)
				return;

			foreach(var r in rigidbodies)
			{
				if(r != null)
				{
					r.AddExplosionForce(explosionForce, explosionPosition, explosionRadius);
				}
			}
		}

		private void AddForce(Vector3 hitForce, ForceMode forceMode)
		{
			foreach(var r in rigidbodies)
			{
				if(r != null)
				{
					r.AddForce(hitForce, forceMode);
				}
			}
		}

		public void SetParent(Transform transform)
		{
			parent = transform;
			localScale = Vector3.one;
		}

		public void HitByObject(IAttackerObject attacker)
		{
			//Debug.Log("HitByObject " + attacker.GetType().Name);

			if(attacker is IRagdollExplosiveInfluencer)
			{
				IRagdollExplosiveInfluencer explosionInfluencer = attacker as IRagdollExplosiveInfluencer;

				HitByIRagdollExplosiveInfluencer(explosionInfluencer.explosionForce, explosionInfluencer.position, explosionInfluencer.explosionForceRadius);
			}
			else if(attacker is IRagdollDirectionalInfluencer)
			{
				IRagdollDirectionalInfluencer dirInfluencer = attacker as IRagdollDirectionalInfluencer;

				HitByIRagdollDirectionalInfluencer(dirInfluencer.hitForce);
			}
		}

		public void HitByIRagdollExplosiveInfluencer(float explosionForce, Vector3 hitPosition, float explosionForceRadius)
		{
			//Debug.Log("HitByIRagdollExplosiveInfluencer " + explosionForce + " / " + hitPosition + " / " + explosionForceRadius);
			AddExplosionForce(explosionForce, hitPosition, explosionForceRadius);
		}

		public void HitByIRagdollDirectionalInfluencer(Vector3 hitForce)
		{
			//Debug.Log("HitByIRagdollDirectionalInfluencer " + hitForce);
			AddForce(hitForce, ForceMode.Impulse);
		}
	}
}
