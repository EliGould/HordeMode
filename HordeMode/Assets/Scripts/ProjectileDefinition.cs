using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Projectile", menuName = "SA/Weapon/Definition")]
public sealed partial class ProjectileDefinition : ScriptableObject
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
	[Serializable]
	public class ProjectileData
	{
		[SerializeField]
		public GameObject bulletPrefab;
		[SerializeField]
		public GameObject bulletSpawn;
		[SerializeField]
		public float bulletSpeed = 30;
		[SerializeField]
		public bool useGravity = false;
		[SerializeField]
		public int pooledAmount = 20;
		public List<GameObject> projectilePool;
	}

#pragma warning restore 0649
	#endregion // Serialized Types
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField]
	public ProjectileData projectileData;
#pragma warning restore 0649
	#endregion // Serialized Fields
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	#endregion // Methods
}
