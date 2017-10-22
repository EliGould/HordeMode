#if UNITY_EDITOR
using UnityEngine;
using UE = UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public static class HandlesHelper
{
	public static void Scope(Action scope)
	{
		if(scope == null)
		{
			return;
		}

		var oldMatrix = Handles.matrix;
		var oldColor = Handles.color;
		var oldLighting = Handles.lighting;

		scope();

		Handles.matrix = oldMatrix;
		Handles.color = oldColor;
		Handles.lighting = oldLighting;
	}
}
#endif // UNITY_EDITOR