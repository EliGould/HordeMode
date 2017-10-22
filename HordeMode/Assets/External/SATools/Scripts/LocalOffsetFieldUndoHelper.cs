using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class LocalOffsetFieldUndoHelper : ScriptableObject
{
	// Serialized to allow for undo
	[SerializeField]
	public Quaternion worldSpaceRot = Quaternion.identity;
}
