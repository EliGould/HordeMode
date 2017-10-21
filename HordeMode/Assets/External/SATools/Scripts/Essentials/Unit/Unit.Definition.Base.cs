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

	// Any and all data that is set in any particular scene. Everything
	// non-scene-specific goes in the Definition
	[Serializable]
	public sealed partial class SceneData
	{
	}

	// Any and all components that is somehow attached to the Unit
	// (i.e. other components that it needs to know about)
	[Serializable]
	public sealed partial class Parts
	{
		[NonSerialized]
		public List<UnitQuirk> quirks;
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
