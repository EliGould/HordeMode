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
    public int damageFactor;

    Weapon weapon;

#pragma warning restore 0649
    #endregion // Serialized Fields
    #endregion // Fields

    #region Properties
    #endregion // Properties

    #region Methods

    void OnCollisionEnter(Collision collision)
    {
        weapon.DamageByProjectile(collision, damageFactor);
        Destroy(this.gameObject);
    }

    public void SetWeapon(Weapon weapon)
    {
        this.weapon = weapon;
    }
    #endregion // Methods
}
