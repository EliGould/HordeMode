using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed partial class Unit : SafeBehaviour
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
			public override void Reset()
			{
			}
		}

		public sealed partial class Momentary : MomentaryBase
		{
			public override void Reset()
			{
			}
		}
	}

	public sealed partial class ManagerState
	{
		public Player owner;
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
