using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// Note: Important that a handle created with the default constructor
//       (i.e. "new EffectHandle()") is an invalid one (id of 0 is
//       invalid)
public struct EffectHandle
{
	public readonly bool hasValue;
	public readonly int id;
	public readonly Effect effect;

	public EffectHandle(EffectInstance fxInstance)
	{
		this.hasValue = fxInstance != null;
		this.id = fxInstance == null ? 0 : fxInstance.id;
		this.effect = fxInstance == null ? null : fxInstance.effect;
	}
}
