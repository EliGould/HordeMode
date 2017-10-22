using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class UnitQuirk : MonoBehaviour
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
#pragma warning restore 0649
	#endregion // Serialized Types

	[Flags]
	enum SetupFlags
	{
		IsSetup = 1 << 0,
		IsEnabled = 1 << 1,
	}
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
#pragma warning restore 0649
	#endregion // Serialized Fields

	Unit _unit;

	SetupFlags flags;
	#endregion // Fields

	#region Properties
	public Unit unit
	{
		get { return _unit; }
		private set { _unit = value; }
	}
	#endregion // Properties

	#region Mono
	protected void OnEnable()
	{
		if(0 != (flags & SetupFlags.IsSetup) && 0 == (flags & SetupFlags.IsEnabled))
		{
			flags |= SetupFlags.IsEnabled;
			AtEnable();
		}
	}

	protected void OnDisable()
	{
		if(0 != (flags & SetupFlags.IsSetup) && 0 != (flags & SetupFlags.IsEnabled))
		{
			flags &= ~SetupFlags.IsEnabled;
			AtDisable();
		}
	}
	#endregion // Mono

	#region Methods
	public void Setup(Unit unit)
	{
		this.unit = unit;

		flags |= SetupFlags.IsSetup;
		AtSetup();

		if(0 == (flags & SetupFlags.IsEnabled) && isActiveAndEnabled)
		{
			flags |= SetupFlags.IsEnabled;
			AtEnable();
		}
	}

	public void Cleanup()
	{
		AtCleanup();
	}

	public void Shutdown()
	{
		Cleanup();

		if(0 != (flags & SetupFlags.IsEnabled) && isActiveAndEnabled)
		{
			flags &= ~SetupFlags.IsEnabled;
			AtDisable();
		}

		flags &= ~SetupFlags.IsSetup;
		AtShutdown();
	}

	public virtual void AtSetup()
	{
	}

	protected virtual void AtEnable()
	{
	}

	public abstract void DoUpdate();

	public virtual void AtCleanup()
	{
	}

	protected virtual void AtDisable()
	{
	}

	public virtual void AtShutdown()
	{
	}
	#endregion // Methods
}
