﻿using UnityEngine;
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
    class BulletData
    {
        [SerializeField]
        public GameObject Bullet_Emitter;
        [SerializeField]
        public GameObject Bullet;
        [SerializeField]
        public float Bullet_Forward_Force;
    }

    [Serializable]
    public class MiscData
    {
        [SerializeField]
        public float impactForce = 10.0f;
        [SerializeField]
        public float knockbackForce = 0.5f;
        [SerializeField]
        public float cooldown = 0.2f;
    }
#pragma warning restore 0649
    #endregion // Serialized Types

    public enum Kind
    {
        Ray,
        Bullet
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
    [SerializeField, EnumRestrict("kind", Kind.Bullet)]
    BulletData bulletData;
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

        if (!isWielded)
        {
            rigid.velocity = rigid.angularVelocity = Vector3.zero;
        }
    }

    public bool Fire()
    {
        if (onCooldown) { return false; }

        switch (kind)
        {
            case Kind.Ray:
                FireRay();
                break;
            case Kind.Bullet:
                FireBullet();
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

        if (hit)
        {
            Rigidbody attachedRigid = hitInfo.collider.attachedRigidbody;

            if (attachedRigid != null)
            {
                attachedRigid.AddForce(
                    ray.direction * miscData.impactForce,
                    ForceMode.Impulse
                );
            }

            UnitManager.instance.DamageUnit(
                hitInfo.collider,
                damageData.damage,
                attacker: wielder,
                ray: ray,
                hitInfo: hitInfo,
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

    void FireBullet()
    {
        GameObject Temporary_Bullet_Handler;
        Temporary_Bullet_Handler = Instantiate(bulletData.Bullet, bulletData.Bullet_Emitter.transform.position, bulletData.Bullet_Emitter.transform.rotation) as GameObject;

        Rigidbody Temporary_RigidBody;
        Temporary_RigidBody = Temporary_Bullet_Handler.GetComponent<Rigidbody>();

        Temporary_RigidBody.AddForce(transform.forward * bulletData.Bullet_Forward_Force);

        Destroy(Temporary_Bullet_Handler, 10.0f);
    }

    public void SwapWeapon()
    {
        if(kind == Kind.Bullet)
        {
            kind = Kind.Ray;
        }

        else
        {
            kind = Kind.Bullet;
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
