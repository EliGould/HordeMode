using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public struct InputAction
{
	#region Types
	public enum Kind : int
	{
		Button,
		Axis,
		Axis2D,
	}

	public enum ValueChange
	{
		None,
		NotZero,
		Zero,
	}
	#endregion // Types

	#region Fields
	[FieldOffset(0)]
	public readonly Kind kind;
	[FieldOffset(sizeof(Kind))]
	public readonly ValueChange valueChanged;
	[FieldOffset(sizeof(Kind) + sizeof(ValueChange))]
	bool boolValue;
	[FieldOffset(sizeof(Kind) + sizeof(ValueChange))]
	Vector2 vectorValue;
	#endregion // Fields

	#region Properties
	public bool buttonHeld
	{
		get
		{
			switch(kind)
			{
				case Kind.Button: return boolValue;
				case Kind.Axis: return Math.Abs(vectorValue.x) > 0.0f;
				case Kind.Axis2D: return vectorValue.sqrMagnitude > 0.0f;
				default: throw new Exception("Unhandled Kind");
			}
		}
	}

	public bool buttonDown
	{
		get { return valueChanged == ValueChange.NotZero; }
	}

	public bool buttonUp
	{
		get { return valueChanged == ValueChange.Zero; }
	}

	public float axis
	{
		get
		{
			switch(kind)
			{
				case Kind.Button: return boolValue ? 1.0f : 0.0f;
				case Kind.Axis: return vectorValue.x;
				case Kind.Axis2D: return vectorValue.normalized.magnitude;
				default: throw new Exception("Unhandled Kind");
			}
		}
	}

	public Vector2 axis2D
	{
		get
		{
			switch(kind)
			{
				case Kind.Button: return new Vector2(boolValue ? 1.0f : 0.0f, 0.0f);
				case Kind.Axis:
				case Kind.Axis2D:
					return vectorValue;
				default: throw new Exception("Unhandled kind");
			}
		}
	}
	#endregion // Properties

	#region Methods
	public InputAction(Kind kind)
	{
		this.kind = kind;
		this.valueChanged = ValueChange.None;
		this.vectorValue = default(Vector2);
		this.boolValue = default(bool);
	}

	public InputAction(bool boolValue, ValueChange valueChanged)
	{
		this.kind = Kind.Button;
		this.valueChanged = valueChanged;
		this.vectorValue = default(Vector2);
		this.boolValue = boolValue;
	}

	public InputAction(float floatValue, ValueChange valueChanged)
	{
		this.kind = Kind.Axis;
		this.valueChanged = valueChanged;
		this.boolValue = default(bool);
		this.vectorValue = new Vector2(floatValue, 0.0f);
	}

	public InputAction(Vector2 vectorValue, ValueChange valueChanged)
	{
		this.kind = Kind.Axis;
		this.valueChanged = valueChanged;
		this.boolValue = default(bool);
		this.vectorValue = vectorValue;
	}
	#endregion // Methods
}
