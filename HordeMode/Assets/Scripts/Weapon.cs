using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public sealed class Weapon : SafeBehaviour
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
	[Serializable]
	class DamageData
	{
		[SerializeField]
		public int damage = 1;
	}

	[Serializable]
	class RayData
	{
		[SerializeField]
		public LocalOffsetField origin;
		[SerializeField]
		public float range = 20.0f;
		[SerializeField]
		public LayerMask hitLayers;
	}

	[Serializable]
	class MiscData
	{
		[SerializeField]
		public float cooldown = 0.2f;
	}
#pragma warning restore 0649
	#endregion // Serialized Types

	public enum Kind
	{
		Ray,
	}
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField]
	Kind kind;
	[SerializeField]
	DamageData damageData;
	[SerializeField]
	MiscData miscData;
	[SerializeField, EnumRestrict("kind", Kind.Ray)]
	RayData rayData;
#pragma warning restore 0649
	#endregion // Serialized Fields

	Unit wielder;
	Rigidbody rigid;
	bool onCooldown;
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Methods
	protected override void AtAwake()
	{
		rigid = GetComponent<Rigidbody>();
    }

	protected override void AtSetup()
	{
		rayData.origin.SetupWithSelf(transform);
	}

	public void SetWielder(Unit wielder)
	{
		bool isWielded = wielder != null;

		this.wielder = wielder;

		rigid.isKinematic = isWielded;
		rigid.useGravity = isWielded;

		if(!isWielded)
		{
			rigid.velocity = rigid.angularVelocity = Vector3.zero;
		}
	}

	public bool Fire()
	{
		if(onCooldown) { return false; }

		switch(kind)
		{
		case Kind.Ray:
			FireRay();
            break;
		}

		Coroutines.Start(this, CooldownRoutine());

		return true;
	}

	void FireRay()
	{
		Vector3 worldPos;
		Quaternion worldRot;
		rayData.origin.GetWorld(out worldPos, out worldRot);

		Vector3 rayDir = worldRot * Vector3.forward;
		Ray ray = new Ray(worldPos, rayDir);
		RaycastHit hitInfo;
		bool hit = Physics.Raycast(
			ray,
			out hitInfo,
			maxDistance: rayData.range,
			layerMask: rayData.hitLayers
		);

		if(hit)
		{
			UnitManager.instance.DamageUnit(
				hitInfo.collider,
				damageData.damage,
				attacker: wielder,
				weapon: this
			);
		}

		UE.Debug.DrawLine(
			ray.origin,
			hit ? hitInfo.point : ray.origin + ray.direction * rayData.range,
			hit ? Color.green : Color.red,
			3.0f
		);
    }

	IEnumerator CooldownRoutine()
	{
		onCooldown = true;
		yield return Coroutines.WaitForSeconds(miscData.cooldown);
		onCooldown = false;
    }
	#endregion // Methods
}
