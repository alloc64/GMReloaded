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

	public class RobotEmilViewLocalClient : IRobotEmilView
	{
		private RobotEmilViewObserver.PlayerLimitations movementLimitations = RobotEmilViewObserver.PlayerLimitations.None;

		private float thirdPersonCamRadius = 0f;

		private Vector3 input;

		public override Ray viewRay
		{
			get
			{
				var t = o.cameraTransform;

				if(t == null)
					return new Ray();

				var lp = t.localPosition;
				lp.z += thirdPersonCamRadius * 2;

				return new Ray(t.TransformPoint(lp), o.direction);
			}
		}

		private Vector3 cameraStrafeDir;

		protected Vector3 controlDirection;

		protected RobotEmilViewObserver.Direction directionState = RobotEmilViewObserver.Direction.Stand;

		protected bool running = false;

		//

		private float diarrheaShootTimer = 0.0f;

		//

		private GMReloaded.UI.Final.KBMenuRenderer menuRenderer { get { return GMReloaded.UI.Final.KBMenuRenderer.IsNull ? null : GMReloaded.UI.Final.KBMenuRenderer.Instance; } }

		//

		protected GMReloaded.Tutorial.TutorialManager tutorial { get { return GMReloaded.Tutorial.TutorialManager.Instance; } }

		//

		private ArenaEventDispatcher arenaEventDispatcher { get { return ArenaEventDispatcher.Instance; } }

		//

		#region IRobotEmilView implementation

		public RobotEmilViewLocalClient(RobotEmilViewObserver observer) : base(observer)
		{
			
		}

		protected override void Awake()
		{
			thirdPersonCamRadius = o.maxViewDistance;
			
		}

		public override void Update()
		{
			UpdateMovement();
			UpdateWeaponControls();
			UpdateActiveBonusControls();
			UpdateCameraDistance();

			float t = Time.deltaTime * 10f;

			if(directionState != RobotEmilViewObserver.Direction.Stand)
			{
				float d = directionState == RobotEmilViewObserver.Direction.Front ? 1f : -1f;
				controlDirection.x = Mathf.Lerp(controlDirection.x, 1f * input.x * d, t);
				controlDirection.y = Mathf.Lerp(controlDirection.y, 1f * input.z, t);
			}
			else
			{
				float lastCDz = controlDirection.z;
				controlDirection = Vector3.Lerp(controlDirection, Vector3.zero, t);
				controlDirection.z = lastCDz;
			}
		}

		#endregion

		#region Camera Distance Check

		private void UpdateCameraDistance()
		{
			var eyePosition = o.robotEyeTransform.position;
			var camPosition = o.cameraDefaultPosTransform.position;

			RaycastHit collisionHit;
			float d = Mathf.Infinity;

			int mask = ~((1 << Layer.EntityContainerEmpty) | (1 << Layer.EntityContainerOccupied) | (1 << Layer.Player) | (1 << Layer.Ragdoll));
			if(Physics.Linecast(eyePosition, camPosition, out collisionHit, mask))
			{
				d = Mathf.Clamp(Vector3.Distance(eyePosition, collisionHit.point) - o.viewOffset, o.minViewDistance, o.maxViewDistance);
			}

			if(d < thirdPersonCamRadius)
			{
				thirdPersonCamRadius = d;
			}
			else
			{
				thirdPersonCamRadius += Timer.unscaledDeltaTime * o.zoomTime;

				if(thirdPersonCamRadius >= d)
				{
					thirdPersonCamRadius = d;
				}
				else if(thirdPersonCamRadius >= o.maxViewDistance)
				{
					thirdPersonCamRadius = o.maxViewDistance;
				}
			}

			Vector3 camLP = o.cameraTransform.localPosition;

			camLP.y = Mathf.Abs(collisionHit.normal.y) * o.viewOffset;
			camLP.z = o.maxViewDistance - thirdPersonCamRadius;

			o.cameraTransform.localPosition = camLP;
		}

		private void SetCameraRadiusLocalPosition(float z)
		{
			Vector3 posLP = o.cameraDefaultPosTransform.localPosition;
			posLP.z = z;

			o.cameraDefaultPosTransform.localPosition = posLP;	
		}

		#endregion

		#region Camera Movement

		private void UpdateMovement() 
		{
			if(menuRenderer != null && menuRenderer.isInMenu)
				return;

			directionState = RobotEmilViewObserver.Direction.Stand;

			UpdateMouseMovement();

			if(o.robotParent == null || o.robotParent.state == RobotEmil.State.Unspawned || o.robotParent.state == RobotEmil.State.Dead || o.robotParent.freezed)
				return;

			Vector3 motion = Vector3.zero;

			if(!movementLimitations.HasFlag(RobotEmilViewObserver.PlayerLimitations.Keyboard))
			{
				if(Input.anyKey)
					tutorial.HandleEvent(TutorialEvent.OnAnyKeyPressed);
			
				input = new Vector3(Input.GetAxis(Config.Player.KeyBind.HorizontalAxis), 0f, Input.GetAxis(Config.Player.KeyBind.VerticalAxis));
				running = !Input.GetButton(Config.Player.KeyBind.Walk);

				cameraStrafeDir = Vector3.Cross(o.direction, Vector3.up);

				float speed = running ? o.runSpeed : o.walkSpeed;
				speed *= o.robotParent.currWeaponWeightSpeedMultiplier;

				// Dopredu - W
				if(input.z > 0f)
				{
					motion = o.direction * speed;
					directionState = RobotEmilViewObserver.Direction.Front;

					tutorial.HandleEvent(TutorialEvent.OnMovedForward);
				}
				// Dozadu - S
				else if(input.z < 0f)
				{
					motion = o.direction * -speed;
					directionState = RobotEmilViewObserver.Direction.Back;
				}

				// Doleva - A
				if(input.x < 0f)
				{
					if(directionState == RobotEmilViewObserver.Direction.Stand)
					{
						directionState = RobotEmilViewObserver.Direction.Front;
					}

					motion.x += cameraStrafeDir.x * speed * 0.75f;
					motion.z += cameraStrafeDir.z * speed * 0.75f;
				}
				// Doprava - D
				else if(input.x > 0f)
				{
					if(directionState == RobotEmilViewObserver.Direction.Stand)
					{
						directionState = RobotEmilViewObserver.Direction.Front;
					}

					motion.x -= cameraStrafeDir.x * speed * 0.75f;
					motion.z -= cameraStrafeDir.z * speed * 0.75f;
				}
			}

			o.robotParent.Move(motion);

			switch(directionState)
			{
				case RobotEmilViewObserver.Direction.Stand:

					o.robotParent.SetState(RobotEmil.State.Idle);

				break;

				case RobotEmilViewObserver.Direction.Front:
				case RobotEmilViewObserver.Direction.Back:

					o.robotParent.SetState(RobotEmil.State.Move);

				break;
			}
		}

		private void UpdateMouseMovement()
		{
			if(movementLimitations.HasFlag(RobotEmilViewObserver.PlayerLimitations.Mouse))
				return;

			Vector2 mouseRotOffset = Vector2.Scale(new Vector2(Input.GetAxis(Config.Player.KeyBind.MouseX), Input.GetAxis(Config.Player.KeyBind.MouseY)), o.mouseSensitivity);

			var eyeTransformLR = o.eyeTransform.localEulerAngles;
			eyeTransformLR.x = Mathf.LerpAngle(eyeTransformLR.x, eyeTransformLR.x - mouseRotOffset.y, Time.deltaTime * o.angleLerpSpeed);

			o.robotParent.RotateOffset(mouseRotOffset.x, Time.deltaTime * o.angleLerpSpeed);

			switch(viewType)
			{
				case RobotEmilViewObserver.Type.PlayerView:
					eyeTransformLR.x = Utils.ClampAngle(eyeTransformLR.x, Config.Player.playerViewXAxisLimits.x, Config.Player.playerViewXAxisLimits.y);

					controlDirection.z = Mathf.Lerp(controlDirection.z, eyeTransformLR.x / -10f, Time.deltaTime * 14f);
				break;

				case RobotEmilViewObserver.Type.DeathCam:
					eyeTransformLR.x = Utils.ClampAngle(eyeTransformLR.x, Config.Player.deathCamXAxisLimits.x, Config.Player.deathCamXAxisLimits.y);
				break;
			}

			SetEyeRotation(eyeTransformLR);
		}

		#endregion
	
		#region Bonuses

		private void UpdateWeaponControls()
		{
			if(menuRenderer != null && menuRenderer.isInMenu)
				return;

			if(!movementLimitations.HasFlag(RobotEmilViewObserver.PlayerLimitations.WeaponSwitch))
			{
				/*for(int i = 0; i < Config.weaponsKeys.Length; i++)
				{
					if(Input.GetButtonUp(Config.weaponsKeys[i]))
					{
						robotParent.SetWeapon(i);	
					}
				}*/

				//TODO: fast switch na Q
				if(Input.GetAxis(Config.Player.KeyBind.MouseScrollWheel) < 0)
				{
					o.robotParent.OnWeaponChanged(1);
				}
				else if(Input.GetAxis(Config.Player.KeyBind.MouseScrollWheel) > 0)
				{
					o.robotParent.OnWeaponChanged(-1);
				}

				#if UNITY_WEBPLAYER

				KeyCode [] keys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };

				for(int i = 0; i < keys.Length; i++)
					if(Input.GetKeyUp(keys[i]))
						o.robotParent.SetWeapon(i);

				#endif

				if(Input.GetButtonUp(Config.Player.KeyBind.FastWeaponSwitch))
					o.robotParent.FastWeaponSwitch();
			}

			bool silenceEnabled = false;
			float diarrheaShootTime = -1.0f;

			if(arenaEventDispatcher != null && arenaEventDispatcher.madnessMode != null)
			{
				silenceEnabled = arenaEventDispatcher.madnessMode.isSilenceEnabled;
				diarrheaShootTime = arenaEventDispatcher.madnessMode.diarrheaShootTime;
			}

			//

			if(!silenceEnabled)
			{
				if(diarrheaShootTime >= 0.0f) // pokud je spustena bude cas >= 0
				{
					if(diarrheaShootTimer < diarrheaShootTime)
						diarrheaShootTimer += Time.deltaTime;

					if(diarrheaShootTimer >= diarrheaShootTime)
					{
						diarrheaShootTimer = 0.0f;

						o.robotParent.PrepareForAttack(RobotEmil.AttackType.Primary);
						o.robotParent.AttackBeingHeld(RobotEmil.AttackType.Primary);
						o.robotParent.Attack(RobotEmil.AttackType.Primary);
					}
				}
				else
				{
					// primary attack

					if(!movementLimitations.HasFlag(RobotEmilViewObserver.PlayerLimitations.AttackPrimary))
					{
						if(Input.GetButtonDown(Config.Player.KeyBind.AttackPrimary))
						{
							o.robotParent.PrepareForAttack(RobotEmil.AttackType.Primary);
						}

						if(Input.GetButton(Config.Player.KeyBind.AttackPrimary))
						{
							o.robotParent.AttackBeingHeld(RobotEmil.AttackType.Primary);
						}

						if(Input.GetButtonUp(Config.Player.KeyBind.AttackPrimary))
						{
							o.robotParent.Attack(RobotEmil.AttackType.Primary);

							tutorial.HandleEvent(TutorialEvent.OnPrimaryAttack);
						}
					}

					// secondary attack

					if(!movementLimitations.HasFlag(RobotEmilViewObserver.PlayerLimitations.AttackSecondary))
					{
						if(Input.GetButtonDown(Config.Player.KeyBind.AttackSecondary))
						{
							o.robotParent.PrepareForAttack(RobotEmil.AttackType.Secondary);
						}

						if(Input.GetButton(Config.Player.KeyBind.AttackSecondary))
						{
							o.robotParent.AttackBeingHeld(RobotEmil.AttackType.Secondary);
						}

						if(Input.GetButtonUp(Config.Player.KeyBind.AttackSecondary))
						{
							o.robotParent.Attack(RobotEmil.AttackType.Secondary);
						}
					}
				}
			}
		}

		private void UpdateActiveBonusControls()
		{
			if(menuRenderer != null && menuRenderer.isInMenu)
				return;

			if(o.robotParent != null && o.robotParent.state == RobotEmil.State.Dead)
				return;

			#if UNITY_WEBPLAYER

			if(Input.GetKeyUp(KeyCode.Y) || Input.GetKeyUp(KeyCode.Z))
				o.robotParent.UseActiveBonus(0, false);

			if(Input.GetKeyUp(KeyCode.X))
				o.robotParent.UseActiveBonus(1, false);

			if(Input.GetKeyUp(KeyCode.C))
				o.robotParent.UseActiveBonus(2, false);

			#else

			if(!movementLimitations.HasFlag(RobotEmilViewObserver.PlayerLimitations.Bonus0) && Input.GetButtonUp(Config.Player.KeyBind.bonusKeys[0]))
			{
				o.robotParent.UseActiveBonus(0, false);
			}

			if(!movementLimitations.HasFlag(RobotEmilViewObserver.PlayerLimitations.Bonus1) && Input.GetButtonUp(Config.Player.KeyBind.bonusKeys[1]))
			{
				o.robotParent.UseActiveBonus(1, false);
			}

			if(!movementLimitations.HasFlag(RobotEmilViewObserver.PlayerLimitations.Bonus2) && Input.GetButtonUp(Config.Player.KeyBind.bonusKeys[2]))
			{
				o.robotParent.UseActiveBonus(2, false);
			}
			#endif
		}

		#endregion

		public void SetMovementLimitations(RobotEmilViewObserver.PlayerLimitations limit)
		{
			this.movementLimitations = limit;
		}

		public override void SyncStates(ref RobotEmilViewObserver.Direction directionState, ref Vector3 controlDirection, ref bool running)
		{
			directionState = this.directionState;
			controlDirection = this.controlDirection;
			running = this.running;
		}

		public override void Shake()
		{
			if(o.cameraAnimation == null)
			{
				Debug.Log("mainCamAnimation null");
				return;
			}

			o.cameraAnimation.Play(o.shakeAnimId);
		}

		public override void SetViewType(RobotEmilViewObserver.Type viewType)
		{
			base.SetViewType(viewType);

			if(o.cameraTransform != null)
				o.cameraTransform.localRotation = o.cameraInitRotation;

			switch(viewType)
			{
				case RobotEmilViewObserver.Type.PlayerView:
					SetMaxViewDistance(o.playerViewMaxViewDistance);
					SetEyeXRotation(0f);
				break;

				case RobotEmilViewObserver.Type.DeathCam:
					SetMaxViewDistance(o.deathCamMaxViewDistance);
				break;
			}
		}

		public void SetMaxViewDistance(float d)
		{
			SetCameraRadiusLocalPosition(-d);

			thirdPersonCamRadius = o.maxViewDistance = d;
		}

		private void SetEyeRotation(Vector3 localRotation)
		{
			o.eyeTransform.localEulerAngles = localRotation;
		}

		private void SetEyeXRotation(float xRot)
		{
			var lr = o.eyeTransform.localEulerAngles;
			lr.x = xRot;
			SetEyeRotation(lr);
		}
	}
	
}
