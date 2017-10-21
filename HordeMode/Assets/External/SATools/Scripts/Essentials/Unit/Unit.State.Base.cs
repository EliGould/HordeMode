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
			public Persistent()
			{
				Reset();
			}
		}

		public sealed partial class Momentary : MomentaryBase
		{
			public Momentary()
			{
				Reset();
			}
		}

		public abstract class PersistentBase
		{
			// Should fully reset all fields to their initial values
			public virtual void Reset()
			{
			}
		}

		public abstract class MomentaryBase
		{
			// Should fully reset all fields to their initial values
			public virtual void Reset()
			{
			}
		}

		public Persistent persistent = new Persistent();
		public Momentary momentary = new Momentary();
	}

	public sealed partial class ManagerState
	{
		public int id;
	}
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
#pragma warning restore 0649

	public State state = new State();
	public ManagerState manState = new ManagerState();
	#endregion // Serialized Fields

	
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	#endregion // Methods
}
