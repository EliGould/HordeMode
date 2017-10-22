using UnityEngine;
using UE = UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed partial class UnitDefinition : ScriptableObject
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
	[Serializable]
	public class GoreData
	{
	}

	[Serializable]
	public class MoveData
	{
		[SerializeField]
		public float acceleration = 10.0f;
		[SerializeField]
		public float deceleration = 10.0f;
		[SerializeField]
		public float moveSpeed = 10.0f;
		[SerializeField]
		public float jumpForce = 100.0f;
	}

	[Serializable]
	public class AttackData
	{
	}
#pragma warning restore 0649
	#endregion // Serialized Types
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField]
	public Texture2D texture;
	[SerializeField]
	public GoreData goreData;
	[SerializeField]
	public MoveData moveData;
	[SerializeField]
	public AttackData attackData;
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

public sealed partial class Unit : UnitBase
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
#pragma warning restore 0649
	#endregion // Serialized Types

	// Any and all data that is set in any particular scene. Everything
	// non-scene-specific goes in the Definition
	public sealed partial class SceneData
	{
		public int startFaction = 0;
	}

	// Any and all components that is somehow attached to the Unit
	// (i.e. other components that it needs to know about)
	public sealed partial class Parts
	{
		[SerializeField]
		public NestedModel visual;
		[SerializeField]
		public Camera camera;
		[SerializeField]
		public CharacterController controller;
		[SerializeField]
		public NavMeshAgent navMeshAgent;

		[NonSerialized]
		public List<SkinnedMeshRenderer> renderers = new List<SkinnedMeshRenderer>();
		[NonSerialized]
		public List<BodyPart> bodyParts = new List<BodyPart>();
	}
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
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
