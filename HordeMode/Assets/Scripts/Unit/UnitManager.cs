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
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
#pragma warning restore 0649
	#endregion // Serialized Fields
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Methods
	#region Interface
	#endregion // Interface

	public void SystemUpdate()
	{
		for(int i = 0; i < allUnits.Count; i++)
		{
			Unit unit = allUnits[i];
			if(unit.isActiveAndEnabled)
			{
				UpdateUnit(unit);
			}
		}
	}

	void UpdateUnit(Unit unit)
	{
		List<UnitQuirk> quirks = unit.parts.quirks;
		for(int i = 0; i < quirks.Count; i++)
		{
			UnitQuirk quirk = quirks[i];
			quirk.DoUpdate();
		}
	}
	#endregion // Methods
}
