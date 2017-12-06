using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed partial class UnitSettings : ScriptableObject
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
	[Serializable]
	public class GroundCheckData
	{
		public float rayRange = 0.1f;
		public float postJumpDelay = 0.2f;
	}

	[Serializable]
	public class AimSettings
	{
		public const float maxRotationX = 320;
		public const float minRotationX = 40;

		public const float sensitivity = 2f;
	}
#pragma warning restore 0649
	#endregion // Serialized Types
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField]
	public GroundCheckData groundCheck;
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
