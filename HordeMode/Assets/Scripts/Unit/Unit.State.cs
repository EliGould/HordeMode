using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed partial class Unit : UnitBase
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
#pragma warning restore 0649
	#endregion // Serialized Types

	public sealed partial class State
	{
		public sealed partial class Persistent : PersistentBase
		{
			public Vector3? navTarget;
			public Vector3 velocity;
			public bool onGround;

			public override void Reset()
			{
				navTarget = null;
				velocity = Vector3.zero;
				onGround = false;
			}
		}

		public sealed partial class Momentary : MomentaryBase
		{
			public Vector2 moveInput;
			public Vector2 aimInput;
			public bool fireInput;
			public bool jumpInput;

			public bool obeyGravity;

			public override void Reset()
			{
				moveInput = new Vector2();
				aimInput = new Vector2();
				fireInput = false;
				jumpInput = false;

				obeyGravity = true;
			}
		}
	}

	public sealed partial class ManagerState
	{
		public class AiData
		{
			public AiState state;
			public Unit chasingTarget;
			public Unit closestTarget;
		}

		public class GroundCheckData
		{
			public float delayTimer;
		}

		public GroundCheckData groundCheck = new GroundCheckData();

		public Player owner;

		public int faction;

		public Vector3 homePoint;

		public AiData aiData = new AiData();
	}

	public enum AiState
	{
		Idle,
		Chasing,
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
