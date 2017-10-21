using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class InputSource
{
	public abstract void DoUpdate();
	public abstract InputAction GetAction(InputActionDef def);
}