using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum EffectFlag
{
	OneShot = 1 << 0,
	Looping = 1 << 1,
}