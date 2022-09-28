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

using Input = TeamUtility.IO.InputManager;

namespace GMReloaded
{
	public class RobotEmilViewObserver : MonoBehaviourTO
	{
		public enum Type
		{
			PlayerView,
			DeathCam
		}

		public enum Direction : byte
		{
			Stand,
			Front,
			Back,
		}

		public enum PlayerLimitations : int
		{
			None 					= 0, 
			Keyboard				= 1 << 0,
			Mouse					= 1 << 1,
			Bonus0					= 1 << 2,
			Bonus1					= 1 << 3,
			Bonus2					= 1 << 4,
			Bonuses					= Bonus0 | Bonus1 | Bonus2,
			AttackPrimary			= 1 << 5,
			AttackSecondary			= 1 << 6,
			WeaponSwitch			= 1 << 7,

			All = Keyboard | Mouse | Bonuses | AttackPrimary | AttackSecondary | WeaponSwitch
		}

		[SerializeField]
		public RobotEmil robotParent;

		[SerializeField]
		private Type viewType;

		[SerializeField]
		public float walkSpeed = 2f;

		[SerializeField]
		public float runSpeed = 4f;

		public float speed = 0f;

		[SerializeField]
		public float playerViewMaxViewDistance = 1.5f;

		[SerializeField]
		public float deathCamMaxViewDistance = 5f;

		public float maxViewDistance = 1.5f;

		[SerializeField]
		public float minViewDistance = 0.1f;

		[SerializeField]
		public float viewOffset = 0.3f;

		[SerializeField]
		public float zoomTime = 2f;

		[SerializeField]
		protected Vector3 controlDirection;

		[SerializeField]
		public Direction directionState = Direction.Stand;

		[SerializeField]
		public bool running = false;

		[SerializeField]
		public float angleLerpSpeed = 10f;

		[SerializeField]
		private Vector2 _mouseSensitivity = new Vector2(2.2f, 1.5f);

		public Vector2 mouseSensitivity { get { return _mouseSensitivity * (Config.ClientPersistentSettings.mouseSensitivity * 6f); } }

		[SerializeField]
		public new Camera camera;

		[SerializeField]
		public Animation cameraAnimation;

		public Transform cameraTransform;
		public Quaternion cameraInitRotation;

		public Ray viewRay { get { return view == null ? new Ray() : view.viewRay; } }

		[SerializeField]
		public string shakeAnimId;

		[SerializeField]
		public Transform eyeTransform;

		[SerializeField]
		public Transform robotEyeTransform;

		[SerializeField]
		public Transform cameraDefaultPosTransform;

		public Vector3 direction;

		//

		public IRobotEmilView view { get; private set; }

		//

		protected virtual void Awake()
		{
			Assert.IsAssigned(camera);
			Assert.IsAssigned(robotParent);

			cameraTransform = camera.transform;
			cameraInitRotation = cameraTransform.localRotation;
		}

		protected virtual void Start()
		{
			// TODO: kontrolovat jestli sou zapnuty soft particles
			camera.depthTextureMode = DepthTextureMode.Depth;
		}

		protected virtual void Update() 
		{	
			direction = robotParent.direction;

			if(view != null)
			{
				view.Update();
				view.SyncStates(ref directionState, ref controlDirection, ref running);

				robotParent.directionState = directionState;
				robotParent.controlDirection = controlDirection;
				robotParent.running = running;
			}
		}

		public void Shake()
		{
			if(view != null)
				view.Shake();
		}

		public void SetViewType(Type type)
		{
			if(this == null)
				return;

			this.viewType = type;

			if(view != null)
				view.SetViewType(type);
		}


		public void SetClientType(RobotEmil.ClientType clientType)
		{
			switch(clientType)
			{
				case RobotEmil.ClientType.RemoteClient:

					SetActive(false);
					camera.gameObject.SetActive(false);

					//view == null

				break;
					
				case RobotEmil.ClientType.LocalClient:

					view = new RobotEmilViewLocalClient(this);

				break;
					
				case RobotEmil.ClientType.BotClient:
					
					camera.gameObject.SetActive(false);

					view = new AI.Bots.RobotEmilViewBotClient(this);

				break;
			}
		}

		//

		public void SetMovementLimitations(PlayerLimitations limit)
		{
			if(!tutorial.isActive)
				return;

			var v = view as RobotEmilViewLocalClient;

			if(v != null)
				v.SetMovementLimitations(limit);
		}

		//

		public override void SetActive(bool active)
		{
			enabled = active;
		}
	}
	
}
