using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class UnitBase : SafeBehaviour
{
	#region Overrides
	protected virtual void AtPreRegister() {}
	protected virtual void AtPostRegister() { }

	protected virtual void AtPreUnregister() {}
	protected virtual void AtPostUnregister() { }
	#endregion // Overrides
}

public sealed partial class Unit : UnitBase
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
	[SerializeField, InlineObject]
	public UnitDefinition def;
	[SerializeField]
	public SceneData sceneData;
#pragma warning restore 0649
	#endregion // Serialized Fields

	public Parts parts = new Parts();
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	protected override void AtSetup()
	{
		parts.quirks = new List<UnitQuirk>();

		AtPreRegister();
		UnitManager.Register(this);
		AtPostRegister();
	}

	protected override void AtShutdown()
	{
		AtPreUnregister();
		UnitManager.Unregister(this);
		AtPostUnregister();
	}
	#endregion // Mono

	#region Methods
	#endregion // Methods
}
