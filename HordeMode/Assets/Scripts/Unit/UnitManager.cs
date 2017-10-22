using UnityEngine;
using UE = UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed partial class UnitManager : UnitManagerBase
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
#pragma warning restore 0649
	#endregion // Serialized Types
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
#pragma warning restore 0649
	#endregion // Serialized Fields
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Methods
	#region Interface
	public void SetOwner(Unit unit, Player player)
	{
		unit.manState.owner = player;
		if(unit.parts.camera != null)
		{
			unit.parts.camera.targetDisplay = player.index;
		}
    }
	#endregion // Interface

	public void SystemUpdate()
	{
		for(int i = 0; i < allUnits.Count; i++)
		{
			Unit unit = allUnits[i];
			if(unit.isActiveAndEnabled)
			{
				UpdateUnit(unit);
			}

			UpdateMovement(unit);
        }
	}

	void UpdateUnit(Unit unit)
	{
		List<UnitQuirk> quirks = unit.parts.quirks;
		for(int i = 0; i < quirks.Count; i++)
		{
			UnitQuirk quirk = quirks[i];
			quirk.DoUpdate();
		}
	}

	void UpdateMovement(Unit unit)
	{
		Unit.State state = unit.state;
		Unit.ManagerState manState = unit.manState;

		float acc = unit.def.moveData.acceleration;
		float dec = unit.def.moveData.deceleration;
		float moveSpeed = unit.def.moveData.moveSpeed;

		Vector3 vel = state.persistent.velocity;

		if(state.momentary.obeyGravity && !state.persistent.onGround)
		{
			vel += Physics.gravity * Time.deltaTime;
		}

		Vector2 xzVel = vel.XZ();
		Vector2 xzDir = xzVel.normalized;
		float xzSpeed = xzVel.magnitude;

		float yVel = vel.y;

		Vector2 inputMoveVector = state.momentary.moveInput;

		if(inputMoveVector == Vector2.zero)
		{
			xzSpeed = Mathf.MoveTowards(xzSpeed, 0.0f, dec * Time.deltaTime);
		}
		else
		{
			Vector3 moveDirLocal = inputMoveVector.ToXZ().normalized;
			xzDir = unit.transform.TransformDirection(moveDirLocal).XZ();

			moveSpeed *= inputMoveVector.magnitude;

			if(xzSpeed < moveSpeed)
			{
				xzSpeed = Mathf.MoveTowards(xzSpeed, moveSpeed, acc * Time.deltaTime);
			}
		}

		DbgValues.Set(unit, "xzSpeed", xzSpeed);
		DbgValues.Set(unit, "moveInput", inputMoveVector);
		DbgValues.Set(unit, "aimInput", inputMoveVector);
		DbgValues.Set(unit, "jumpInput", state.momentary.jumpInput);
		DbgValues.Set(unit, "fireInput", state.momentary.fireInput);
		DbgValues.Set(unit, "rot", unit.transform.rotation);

		Vector2 moveVector = xzDir * xzSpeed;

		bool wantsJump = state.momentary.jumpInput;

		if(wantsJump && state.persistent.onGround)
		{
			yVel += unit.def.moveData.jumpForce;
			manState.groundCheck.delayTimer = settings.groundCheck.postJumpDelay;
		}

		vel = new Vector3(
			moveVector.x,
			yVel,
			moveVector.y
		);

		if(unit.parts.controller != null)
		{
			CollisionFlags collFlags = unit.parts.controller.Move(vel * Time.deltaTime);

			if(0 != (collFlags & CollisionFlags.CollidedSides))
			{
				vel.x = vel.z = 0.0f;
			}
		}

		bool onGround;

		if(unit.manState.groundCheck.delayTimer > 0.0f)
		{
			onGround = false;
		}
		else
		{
			onGround = GroundCheck(unit);
		}

		state.persistent.onGround = onGround;

		if(onGround)
		{
			vel.y = 0.0f;
		}

		Vector2 aimVector = state.momentary.aimInput;

		if(unit.parts.camera != null)
		{
			unit.parts.camera.transform.Rotate(
				aimVector.y, 0.0f, 0.0f
			);
		}

		state.persistent.velocity = vel;

		unit.transform.Rotate(0.0f, aimVector.x, 0.0f);

		if(manState.groundCheck.delayTimer > 0.0f)
		{
			manState.groundCheck.delayTimer -= Time.deltaTime;
		}
	}

	bool GroundCheck(Unit unit)
	{
		RaycastHit hit;
		Ray ray = new Ray(unit.transform.position, Vector3.down);

		float maxDistance = settings.groundCheck.rayRange;

		if(Physics.Raycast(ray, out hit, maxDistance, layerMask: 1 << 8))
		{
			return true;
		}

		return false;
	}

	#region Body Parts
	#endregion // Body Parts
	#endregion // Methods
}
