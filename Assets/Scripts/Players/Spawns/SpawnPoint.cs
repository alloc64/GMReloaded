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
using System;

namespace GMReloaded
{
	public class SpawnPoint : MonoBehaviourTO
	{
		public enum State : byte
		{
			Unused,
			Used
		}

		[SerializeField]
		public State state;

		[SerializeField]
		public float lastUsedTimestamp = -1f;

		[SerializeField]
		private bool autoRegister = true;

		#region Singleton getters

		private SpawnManager spawnManager { get { return SpawnManager.Instance; } }

		private RoomPropertiesController rpc { get { return RoomPropertiesController.Instance; } }

		#endregion

		private void Awake()
		{
			if(autoRegister)
				spawnManager.Register(this);
		}

		private void OnDrawGizmos()
		{
			Vector3 rot = localEulerAngles;

			Vector3 dir = new Vector3(Mathf.Sin(rot.y * Mathf.Deg2Rad), 0f, Mathf.Cos(rot.y * Mathf.Deg2Rad));

			Gizmos.color = state == State.Used ? Color.red : Color.green;
			Gizmos.DrawLine(position, position + dir * 1f);
		}

		public void SetUsed(bool used)
		{
			if(!PhotonNetwork.isMasterClient)
				return;

			if(used)
				lastUsedTimestamp = Time.realtimeSinceStartup;

			SetState(used ? State.Used : State.Unused);
		}

		public void SetState(State state)
		{
			this.state = state;
		}
	}
}
