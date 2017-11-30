using System;
using System.Collections;
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
        public LayerMask hitLayers;
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

        if (hit)
        {
            Rigidbody attachedRigid = hitInfo.collider.attachedRigidbody;

            if (attachedRigid != null)
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
        var bullet = Instantiate(
            projectileData.bulletPrefab,
            projectileData.bulletSpawn.transform.GetChild(0).position,
            projectileData.bulletSpawn.transform.GetChild(0).rotation);

        bullet.GetComponent<Projectile>().SetWeapon(this);
        bullet.GetComponent<Rigidbody>().useGravity = projectileData.useGravity;

        bullet.transform.Rotate(90, 0, 0);

        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.up * projectileData.bulletSpeed;

        //Pool bullets later!
        Destroy(bullet, 2.0f);
    }

    public void DamageByProjectile(Collision coll, int damageFactor)
    {
        if (coll.gameObject != wielder)
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
    }


    IEnumerator CooldownRoutine()
    {
        onCooldown = true;
        yield return Coroutines.WaitForSeconds(miscData.cooldown);
        onCooldown = false;
    }
    #endregion // Methods
}
