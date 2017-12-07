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

	Dictionary<Collider, Unit> collToUnit = new Dictionary<Collider, Unit>();

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

	public void GetQuirks<T>(List<T> results) where T : UnitQuirk
	{
		for(int i = 0; i < allUnits.Count; i++)
		{
			Unit unit = allUnits[i];

			if(!unit.isActiveAndEnabled) { continue; }

			for(int z = 0; z < unit.parts.quirks.Count; z++)
			{
				UnitQuirk quirk = unit.parts.quirks[z];
				if(quirk.GetType() == typeof(T))
				{
					results.Add((T)quirk);
				}
			}
		}
	}

	public void GetUnits(
		List<Unit> results,
		int? inFaction = null,
		int? notInFaction = null
	)
	{
		for(int i = 0; i < allUnits.Count; i++)
		{
			Unit unit = allUnits[i];

			if(!unit.isActiveAndEnabled) { continue; }

			bool shouldAdd =
				(inFaction == null || unit.manState.faction == inFaction.Value) &&
				(notInFaction == null || unit.manState.faction != notInFaction.Value)
			;
			if(shouldAdd)
			{
				results.Add(unit);
			}
		}
	}

	public Unit GetClosestUnit(Unit fromUnit, List<Unit> otherUnits = null)
	{
		if(otherUnits == null) { otherUnits = allUnits; }

		Unit closest = null;
		float closestDist = float.PositiveInfinity;

		for(int i = 0; i < otherUnits.Count; i++)
		{
			Unit otherUnit = otherUnits[i];

			if(otherUnit == fromUnit) { continue; }

			float dist = DistanceBetweenUnits(fromUnit, otherUnit);

			if(dist < closestDist)
			{
				closest = otherUnit;
				closestDist = dist;
			}
		}

		return closest;
	}

	public float DistanceBetweenUnits(Unit a, Unit b)
	{
		return Vector3.Distance(
			a.transform.position,
			b.transform.position
		);
	}

	public void DamageUnit(
		Collider hitCollider,
		int damage,
		Vector3 direction,
		Vector3 point,
		Unit attacker = null,
		Weapon weapon = null
	)
	{
		Unit unit = collToUnit.FindOrNull(hitCollider);
		if(unit == null) { return; }

		BodyParts bodyParts = unit.parts.bodyParts;
		BodyParts.PartData partData = bodyParts.GetPartDataForCollider(hitCollider);

		if(!partData.isAttached) { return; }

		BodyParts.PartDef partDef = bodyParts.GetPartDefForKind(partData.kind);

		int newHp = Mathf.Max(0, partData.hitPoints - damage);
		partData.hitPoints = newHp;

		Dbg.Log("{0} hit {1} in {2} for {3} damage!", GarbageCache.GetName(attacker), GarbageCache.GetName(unit), partData.kind, damage);

		if(unit.parts.navMeshAgent != null)
		{
			if(direction != Vector3.zero && weapon != null)
			{
				unit.parts.navMeshAgent.Move(direction * weapon.miscData.knockbackForce);
			}
		}

		if(newHp == 0)
		{
			if(partDef.lifeCritical)
			{
				Vector3 killPoint = point;
				float? killForce = weapon == null ? null : (float?)weapon.miscData.impactForce;
				KillUnit(unit, killPoint, killForce);
			}
			else
			{
				Vector3? worldForce = null;

				if(direction != Vector3.zero)
				{
					worldForce = direction * weapon.miscData.impactForce;
				}

				bodyParts.Detach(partData, worldForce);
			}
		}
	}

	void KillUnit(Unit unit, Vector3? killPoint = null, float? killForce = null)
	{
		Dbg.Log("{0} dies!", GarbageCache.GetName(unit));
		unit.parts.bodyParts.DetachAll(
			killPoint,
			killForce
		);
		Weapon wieldingWeapon = unit.manState.weaponData.wieldingWeapon;
		if(wieldingWeapon != null)
		{
			wieldingWeapon.SetWielder(null);
		}

		NavMeshAgent navAgent = unit.parts.navMeshAgent;
		if(navAgent != null)
		{
			navAgent.enabled = false;
		}
	}

	public void SetWeapon(Unit unit, Weapon weapon)
	{
		Unit.ManagerState.WeaponData weaponData = unit.manState.weaponData;

		weaponData.wieldingWeapon = weapon;
		if(weapon == null) { return; }

		weapon.SetWielder(unit);

		weapon.transform.parent = weaponData.parentNode;
		weapon.transform.localPosition = Vector3.zero;
		weapon.transform.localRotation = Quaternion.identity;
	}

	public void SwitchWeapon(Unit unit)
	{
		unit.parts.weapons[unit.parts.weaponIndex].gameObject.SetActive(false);
		unit.parts.weaponIndex = (unit.parts.weaponIndex + 1) % unit.parts.weapons.Count;
		unit.parts.weapons[unit.parts.weaponIndex].gameObject.SetActive(true);

		SetWeapon(unit, unit.parts.weapons[unit.parts.weaponIndex]);
	}

	#endregion // Interface

	#region Updating
	public void SystemUpdate()
	{
		for(int i = 0; i < allUnits.Count; i++)
		{
			Unit unit = allUnits[i];
			if(unit.isActiveAndEnabled)
			{
				UpdateUnit(unit);
			}
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

		UpdateMovement(unit);
		UpdatePathing(unit);
		UpdateWeapon(unit);
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

		//DbgValues.Set(unit, "xzSpeed", xzSpeed);
		//DbgValues.Set(unit, "moveInput", inputMoveVector);
		//DbgValues.Set(unit, "aimInput", inputMoveVector);
		//DbgValues.Set(unit, "jumpInput", state.momentary.jumpInput);
		//DbgValues.Set(unit, "fireInput", state.momentary.fireInput);
		//DbgValues.Set(unit, "rot", unit.transform.rotation);

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

		Vector2 aimVector = state.momentary.aimInput * UnitSettings.AimSettings.sensitivity;

		if(unit.parts.camera != null)
		{
			float rotationX = unit.parts.camera.transform.localRotation.eulerAngles.x;

			if((rotationX + aimVector.y) <= UnitSettings.AimSettings.minRotationX || (rotationX + aimVector.y) >= UnitSettings.AimSettings.maxRotationX)
			{
				unit.parts.camera.transform.Rotate(
					aimVector.y, 0.0f, 0.0f
				);
			}
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

	void UpdatePathing(Unit unit)
	{
		NavMeshAgent navAgent = unit.parts.navMeshAgent;

		if(navAgent == null || !navAgent.isActiveAndEnabled)
		{
			return;
		}

		const float epsilon = 0.001f;

		Unit.State.Persistent persistent = unit.state.persistent;

		if(persistent.navTarget == null)
		{
			navAgent.isStopped = true;
			navAgent.ResetPath();
		}
		else
		{
			NavMeshHit hitInfo;
			bool validNavTarget = NavMesh.SamplePosition(
				persistent.navTarget.Value,
				out hitInfo,
				maxDistance: Mathf.Infinity,
				areaMask: 1 << 0
			);

			if(!validNavTarget)
			{
				persistent.navTarget = null;
				return;
			}

			float diff = Vector3.Distance(hitInfo.position, navAgent.destination);
			if(diff > epsilon)
			{
				//Dbg.Log("Setting new destination");
				bool success = navAgent.SetDestination(persistent.navTarget.Value);
				if(!success)
				{
					persistent.navTarget = null;
				}
			}

			if(persistent.navTarget != null)
			{
				bool reached = ReachedDestination(unit);

				if(reached)
				{
					//Dbg.Log("Reached destination");
					persistent.navTarget = null;
				}
			}
		}
	}

	void UpdateWeapon(Unit unit)
	{
		Unit.ManagerState.WeaponData weaponData = unit.manState.weaponData;
		if(weaponData.wieldingWeapon == null) { return; }

		if(unit.state.momentary.fireInput)
		{
			weaponData.wieldingWeapon.Fire();
		}

		if(unit.state.momentary.weaponChangeInput)
		{
			SwitchWeapon(unit);
		}
	}
	#endregion // Updating

	#region Lifecycle
	protected override void AtDidRegister(Unit unit)
	{
		unit.parts.bodyParts.Setup(unit);

		using(var colls = TempList<Collider>.Get())
		{
			unit.parts.bodyParts.GetAttachedColliders(colls.buffer);

			for(int i = 0; i < colls.Count; i++)
			{
				Collider coll = colls[i];
				collToUnit[coll] = unit;
			}
		}

		if(unit.parts.weapons.Count != 0)
		{
			for(int i = 0; i < unit.parts.weapons.Count; i++)
			{
				unit.parts.weapons[i] = Instantiate(unit.parts.weapons[i]);
			}

			SetWeapon(unit, unit.parts.weapons[unit.parts.weaponIndex]);
		}
	}

	protected override void AtWillUnregister(Unit unit)
	{
		using(var colls = TempList<Collider>.Get())
		{
			unit.parts.bodyParts.GetAttachedColliders(colls.buffer);

			for(int i = 0; i < colls.Count; i++)
			{
				Collider coll = colls[i];
				collToUnit.Remove(coll);
			}
		}
	}
	#endregion // Lifecycle

	public bool ReachedDestination(Unit unit)
	{
		const float epsilon = 0.001f;

		NavMeshAgent navAgent = unit.parts.navMeshAgent;

		bool reached =
			navAgent.pathStatus == NavMeshPathStatus.PathComplete &&
			navAgent.remainingDistance <= epsilon &&
			!navAgent.pathPending
		;

		return reached;
	}
	#region Body Parts
	#endregion // Body Parts
	#endregion // Methods
}
