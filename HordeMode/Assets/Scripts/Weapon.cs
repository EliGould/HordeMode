using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UE = UnityEngine;

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
		[SerializeField]
		public float impactForce = 10.0f;
		[SerializeField]
		public float knockbackForce = 0.5f;
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
	public class MiscData
	{
		[SerializeField]
		public float cooldown = 0.0f;
	}
#pragma warning restore 0649
	#endregion // Serialized Types

	public enum Kind
	{
		Ray,
		Projectile
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
	public MiscData miscData;
	[SerializeField, EnumRestrict("kind", Kind.Ray)]
	RayData rayData;
	[SerializeField, EnumRestrict("kind", Kind.Projectile)]
	public ProjectileDefinition projectileDef;
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
		if(kind == Kind.Projectile)
		{
			projectileDef.projectileData.projectilePool = new List<GameObject>();
			for(int i = 0; i < projectileDef.projectileData.pooledAmount; i++)
			{
				GameObject bullet = Instantiate(projectileDef.projectileData.bulletPrefab, projectileDef.projectileData.bulletSpawn.transform.GetChild(0).position, projectileDef.projectileData.bulletPrefab.transform.rotation);
				bullet.GetComponent<Rigidbody>().useGravity = projectileDef.projectileData.useGravity;
				bullet.SetActive(false);

				bullet.GetComponent<Projectile>().SetDamageData(damageData.damage, damageData.impactForce, damageData.knockbackForce);

				projectileDef.projectileData.projectilePool.Add(bullet);
			}
		}

		else if(kind == Kind.Ray)
		{
			rayData.origin.SetupWithSelf(transform);
		}
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
			case Kind.Projectile:
				FireProjectile();
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
			Rigidbody attachedRigid = hitInfo.collider.attachedRigidbody;

			if(attachedRigid != null)
			{
				Debug.Log(ray.direction);
				attachedRigid.AddForce(
					ray.direction * damageData.impactForce,
					ForceMode.Impulse
				);
			}

			UnitManager.instance.DamageUnit(
				hitInfo.collider,
				damageData.damage,
				damageData.knockbackForce,
				damageData.knockbackForce,
				ray.direction,
				hitInfo.point
			);
		}

		UE.Debug.DrawLine(
			ray.origin,
			hit ? hitInfo.point : ray.origin + ray.direction * rayData.range,
			hit ? Color.green : Color.red,
			3.0f
		);
	}

	void FireProjectile()
	{
		for(int i = 0; i < projectileDef.projectileData.projectilePool.Count; i++)
		{
			if(!projectileDef.projectileData.projectilePool[i].activeInHierarchy)
			{
				Rigidbody projectileRigid = projectileDef.projectileData.projectilePool[i].GetComponent<Rigidbody>();

				projectileRigid.velocity = Vector3.zero;
				projectileRigid.angularVelocity = Vector3.zero;

				projectileDef.projectileData.projectilePool[i].transform.position = this.transform.GetChild(0).position;
				projectileDef.projectileData.projectilePool[i].transform.rotation = this.transform.rotation * projectileDef.projectileData.bulletPrefab.transform.rotation;

				projectileRigid.velocity = this.transform.forward * projectileDef.projectileData.bulletSpeed;

				projectileDef.projectileData.projectilePool[i].SetActive(true);
				break;
			}
		}
	}

	IEnumerator CooldownRoutine()
	{
		onCooldown = true;
		yield return Coroutines.WaitForSeconds(miscData.cooldown);
		onCooldown = false;
	}
	#endregion // Methods
}
