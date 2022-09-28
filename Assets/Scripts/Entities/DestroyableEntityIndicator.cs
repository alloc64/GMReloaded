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

namespace GMReloaded.Entities
{
	public class DestroyableEntityIndicator : MonoBehaviourTO
	{
		private Transform lastTransform;
		private Transform defaultParent;

		public void SetDefaultParent(Transform defaultParent)
		{
			this.defaultParent = defaultParent;
		}

		public virtual void SetParent(EntityContainer ec)
		{
			if(ec != null)
			{
				this.parent = ec.transform;

				localPosition = Vector3.zero;
				localScale = Vector3.one;
				localRotation = Quaternion.identity;
			}
			else
			{
				this.parent = defaultParent;
			}

			SetActive(ec != null);
		}

		public virtual void Process(Transform objToProcessTransform)
		{
			lastTransform = transform;

			TraverseHierarchy(objToProcessTransform);

			lastTransform = null;
		}

		// toto nefunguje idealne ale pro nase pripady to staci
		private void TraverseHierarchy(Transform root) 
		{
			int childCount = root.childCount;

			for(int i = 0; i < childCount; i++)
			{
				var child = root.GetChild(i);

				if(!child.gameObject.activeSelf || child.GetComponent<ParticleSystem>() != null)
					continue;
				
				var mfGO = CreateGameObject(child, lastTransform);

				var mf = child.GetComponent<MeshFilter>();

				if(mf != null)
				{
					var newMF = mfGO.AddComponent<MeshFilter>();
					newMF.mesh = mf.mesh;

					var newMR = mfGO.AddComponent<MeshRenderer>();

					var mr = child.GetComponent<MeshRenderer>();

					if(mr != null)
					{
						Material[] newMaterials = new Material[mr.materials.Length];

						var oldMaterials = mr.materials;

						for(int j = 0; j < oldMaterials.Length; j++)
						{
							var newMat = oldMaterials[j];

							newMat.SetFloat("_Mode", 2);
							newMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
							newMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
							newMat.SetInt("_ZWrite", 0);
							newMat.DisableKeyword("_ALPHATEST_ON");
							newMat.EnableKeyword("_ALPHABLEND_ON");
							newMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
							newMat.renderQueue = 3000;

							var c = newMat.GetColor("_Color");

							c.a = 0.45f;

							newMat.SetColor("_Color", c);

							newMaterials[j] = newMat;
						}

						newMR.materials = newMaterials;
					}
				}

				if(child.childCount > 0 && child.GetComponentsInChildren<MeshFilter>() != null)
				{
					lastTransform = mfGO.transform;
					TraverseHierarchy(child);
				}
			}
		}

		private GameObject CreateGameObject(Transform currentTransform, Transform lastTransform)
		{
			GameObject go = new GameObject(currentTransform.name);
			go.transform.parent = lastTransform;
			go.transform.localPosition = currentTransform.transform.localPosition;
			go.transform.localRotation = currentTransform.transform.localRotation;
			go.transform.localScale = currentTransform.transform.localScale;

			return go;
		}

		/*/ Mesh processing - wireframe

		public void Process(MeshFilter[] meshesToCombine)
		{
			var combinedMesh = CombineMeshes(meshesToCombine);

			if(combinedMesh == null)
				return;

			CreateLineMeshFromMesh(combinedMesh);
		}

		private Mesh CombineMeshes(MeshFilter[] meshesToCombine)
		{
			if(meshesToCombine == null || meshesToCombine.Length < 1)
				return null;
			
			CombineInstance[] combine = new CombineInstance[meshesToCombine.Length];

			int i = 0;
			foreach(var mf in meshesToCombine)
			{
				combine[i].mesh = mf.sharedMesh;
				combine[i].transform = mf.transform.localToWorldMatrix;
				i++;
			}

			var finalMesh = new Mesh();
			finalMesh.CombineMeshes(combine);

			return finalMesh;
		}

		private void CreateLineMeshFromMesh(Mesh inputMesh)
		{
			if(inputMesh == null)
				return;

			var mesh = ProcessMeshToLines(inputMesh);

			MeshFilter newMeshFilter = gameObject.AddComponent<MeshFilter>();

			newMeshFilter.mesh = mesh;
			var meshRenderer = gameObject.AddComponent<MeshRenderer>();
		}

		private Mesh ProcessMeshToLines(Mesh meshToProcess)
		{
			if(meshToProcess == null)
				return null;

			Mesh mesh = new Mesh();

			mesh.vertices = meshToProcess.vertices;

			int[] oldTris = meshToProcess.triangles;
			int[] indexes = new int[oldTris.Length*2];

			for (int i = 0, a = 0; i < oldTris.Length; i+=3)
			{
				indexes[a++] = oldTris[i];
				indexes[a++] = oldTris[i + 1];
				indexes[a++] = oldTris[i + 1];
				indexes[a++] = oldTris[i + 2];
				indexes[a++] = oldTris[i + 2];
				indexes[a++] = oldTris[i];
			}

			mesh.SetIndices(indexes, MeshTopology.Lines, 0);

			return mesh;
		}
		*/
	}
}