using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class Player
{
	#region Types
	#endregion // Types

	#region Fields
	public readonly int id;
	public readonly int index;
	public readonly InputSource inputSource;
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Methods
	public Player(int id, int index, InputSource inputSource)
	{
		this.id = id;
		this.index = index;
		this.inputSource = inputSource;
	}
	#endregion // Methods
}
