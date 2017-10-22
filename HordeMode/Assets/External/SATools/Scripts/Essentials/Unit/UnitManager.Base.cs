using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed partial class UnitManager : UnitManagerBase
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
#pragma warning restore 0649
	#endregion // Serialized Types

	public delegate void RegisteredHandler(Unit unit);
	public delegate void UnregisteredHandler(Unit unit);
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
#pragma warning restore 0649
	#endregion // Serialized Fields

	public RegisteredHandler onUnitDidRegister;
	public UnregisteredHandler onUnitWillUnregister;

#pragma warning disable 0414
	UnitSettings settings;
#pragma warning restore 0414

	int idCounter;
	List<Unit> allUnits;
	Dictionary<int, Unit> idToUnit;
	#endregion // Fields

	#region Static Properties
	public static UnitManager instance
	{
		get;
		private set;
	}
	#endregion // Static Properties

	#region Properties
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	#region System
	public static UnitManager Setup(UnitSettings settings, ref UnitSetupData setupData)
	{
		instance = new GameObject("UnitManager").AddComponent<UnitManager>();

		instance.SetupInternal(settings, ref setupData);

		return instance;
	}

	void SetupInternal(UnitSettings settings, ref UnitSetupData setupData)
	{
		this.settings = settings;

		allUnits = new List<Unit>();
		idToUnit = new Dictionary<int, Unit>();

		instance.AtSetup(settings, ref setupData);
	}

	public void Shutdown()
	{
		Destroy(instance.gameObject);
		instance = null;
	}

	bool IsRegistered(Unit unit)
	{
		return idToUnit.ContainsKey(unit.manState.id);
	}

	public static void Register(Unit unit)
	{
		instance.RegisterInternal(unit);

		if(instance.onUnitDidRegister != null) { instance.onUnitDidRegister(unit); }
    }

	void RegisterInternal(Unit unit)
	{
		if(IsRegistered(unit)) { return; }

		int id = ++idCounter;

		allUnits.Add(unit);
		idToUnit[id] = unit;

		unit.state.persistent.Reset();

		// TODO
		unit.GetComponents<UnitQuirk>(results: unit.parts.quirks);

		for(int i = 0; i < unit.parts.quirks.Count; ++i)
		{
			UnitQuirk quirk = unit.parts.quirks[i];
			quirk.Setup(unit);
		}
	}

	public static void Unregister(Unit unit)
	{
		instance.UnregisterInternal(unit);
	}

	void UnregisterInternal(Unit unit)
	{
		if(!IsRegistered(unit)) { return; }

		if(instance.onUnitWillUnregister != null) { instance.onUnitWillUnregister(unit); }

		// TODO
		for(int i = 0; i < unit.parts.quirks.Count; ++i)
		{
			UnitQuirk quirk = unit.parts.quirks[i];
			quirk.Shutdown();
		}

		allUnits.Remove(unit);
		idToUnit.Remove(unit.manState.id);
	}
	#endregion // System
	#endregion // Methods
}

public abstract class UnitManagerBase : MonoBehaviour
{
	protected virtual void AtSetup(UnitSettings settings, ref UnitSetupData setupData) { }
}

// Should contain any additional required systems and data
// the manager may need
public partial struct UnitSetupData
{
}