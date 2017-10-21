using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class MathHelper
{
	#region Types
	#endregion // Types

	#region Fields
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Methods
	public static float ModWrap(float x, float m)
	{
		return (x % m + m) % m;
	}

	#region Animation Curves
	public static AnimationCurve CreateLinearCurve(
		float timeStart = 0.0f,
		float valueStart = 0.0f,
		float timeEnd = 1.0f,
		float valueEnd = 1.0f	
	)
	{
		return AnimationCurve.Linear(timeStart, valueStart, timeEnd, valueEnd);
	}

	public static AnimationCurve CreateEaseInOutCurve(
		float timeStart = 0.0f,
		float valueStart = 0.0f,
		float timeEnd = 1.0f,
		float valueEnd = 1.0f
	)
	{
		return AnimationCurve.EaseInOut(timeStart, valueStart, timeEnd, valueEnd);
	}
	#endregion // Animation Curves
	#endregion // Methods
}
