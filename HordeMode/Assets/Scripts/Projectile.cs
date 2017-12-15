using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class Projectile : MonoBehaviour
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
#pragma warning restore 0649
	#endregion // Serialized Types
	#endregion // Types

	#region Fields

	int damage;
	float impactForce;
	float knockbackForce;

	#region Serialized Fields
#pragma warning disable 0649

#pragma warning restore 0649
	#endregion // Serialized Fields
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Methods

	public void setDamageData(int damage, float impactForce, float knockbackForce)
	{
		this.damage = damage;
		this.impactForce = impactForce;
		this.knockbackForce = knockbackForce;
	}

	void OnEnable()
	{
		Invoke("DestroyProjectile", time: 2.0f);
	}

	void DestroyProjectile()
	{
		gameObject.SetActive(false);
	}

	private void OnDisable()
	{
		CancelInvoke();
	}

	void OnCollisionEnter(Collision collision)
	{
		Vector3 direction = Vector3.Normalize(collision.contacts[0].thisCollider.gameObject.transform.position - collision.contacts[0].otherCollider.gameObject.transform.position);
		Vector3 point = collision.contacts[0].point;

		UnitManager.instance.DamageUnit(
				collision.collider,
				damage,
				knockbackForce,
				impactForce,
				direction,
				point
			);

		gameObject.SetActive(false);
	}

	#endregion // Methods
}
