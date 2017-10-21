using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public struct UnitHandle
{
	public readonly int id;

	public bool hasValue
	{
		get { return id != 0; }
	}

	public UnitHandle(Unit unit)
	{
		this.id = unit == null ? 0 : unit.manState.id;
	}
}
