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
	class ProjectileData
	{
		[SerializeField]
		public GameObject bulletPrefab;
		[SerializeField]
		public GameObject bulletSpawn;
		[SerializeField]
		public float bulletSpeed;
		[SerializeField]
		public bool useGravity;
		[SerializeField]
		public int pooledAmount;
		public List<GameObject> projectilePool;

	}

	[Serializable]
	public class MiscData
	{
		[SerializeField]
		public float impactForce = 10.0f;
		[SerializeField]
		public float knockbackForce = 0.5f;
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
	ProjectileData projectileData;
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
			projectileData.projectilePool = new List<GameObject>();
			for(int i = 0; i < projectileData.pooledAmount; i++)
			{
				GameObject bullet = Instantiate(projectileData.bulletPrefab, projectileData.bulletSpawn.transform.GetChild(0).position, projectileData.bulletPrefab.transform.rotation);
				bullet.GetComponent<Projectile>().SetWeapon(this);
				bullet.GetComponent<Rigidbody>().useGravity = projectileData.useGravity;

				bullet.SetActive(false);
				projectileData.projectilePool.Add(bullet);
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
					ray.direction * miscData.impactForce,
					ForceMode.Impulse
				);
			}

			UnitManager.instance.DamageUnit(
				hitInfo.collider,
				damageData.damage,
				ray.direction,
				hitInfo.point,
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

	void FireProjectile()
	{
		for(int i = 0; i < projectileData.projectilePool.Count; i++)
		{
			if(!projectileData.projectilePool[i].activeInHierarchy)
			{
				projectileData.projectilePool[i].transform.position = projectileData.bulletSpawn.transform.GetChild(0).position;
				projectileData.projectilePool[i].transform.rotation = projectileData.bulletSpawn.transform.GetChild(0).rotation * projectileData.bulletPrefab.transform.rotation;

				projectileData.projectilePool[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
				projectileData.projectilePool[i].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
				projectileData.projectilePool[i].GetComponent<Rigidbody>().velocity = projectileData.bulletSpawn.transform.forward * projectileData.bulletSpeed;

				projectileData.projectilePool[i].SetActive(true);
				break;
			}
		}


	}

	public void DamageByProjectile(Collision coll, int damageFactor)
	{
		Vector3 direction = Vector3.Normalize(coll.collider.transform.position - wielder.transform.position);

		Vector3 point = coll.contacts[0].point;

		UE.Debug.DrawLine(coll.collider.transform.position, wielder.transform.position, Color.blue);


		UnitManager.instance.DamageUnit(
				coll.collider,
				damageData.damage * damageFactor,
				direction,
				point,
				attacker: wielder,
				weapon: this
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
