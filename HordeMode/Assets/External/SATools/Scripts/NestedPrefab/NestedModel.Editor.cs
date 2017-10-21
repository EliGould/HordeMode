#if UNITY_EDITOR
using UnityEngine;
using UE = UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed partial class NestedModel : NestedPrefab
{
	#region Editor
	[CustomEditor(typeof(NestedModel))]
	[CanEditMultipleObjects]
	protected new class Editor : NestedPrefab.Editor
	{
	}
	#endregion // Editor
}
#endif // UNITY_EDITOR