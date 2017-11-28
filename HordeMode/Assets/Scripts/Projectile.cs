﻿using UnityEngine;
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
    #region Serialized Fields
#pragma warning disable 0649

    [SerializeField]
    GameObject bulletPrefab;
    [SerializeField]
    Transform bulletSpawn;
    [SerializeField]
    float bulletSpeed;

#pragma warning restore 0649
    #endregion // Serialized Fields

    #endregion // Fields

    #region Properties
    #endregion // Properties

    #region Methods
    public void Fire()
    {
        var bullet = (GameObject)Instantiate(
            bulletPrefab,
            bulletSpawn.position,
            bulletSpawn.rotation);

        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * bulletSpeed;

        Destroy(bullet, 2.0f);
    }
    #endregion // Methods
}
