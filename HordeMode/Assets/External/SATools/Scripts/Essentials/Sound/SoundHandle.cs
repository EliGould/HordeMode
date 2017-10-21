using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public struct SoundHandle
{
	public readonly int id;
	public readonly Sound sound;

	public bool hasValue
	{
		get { return id != 0; }
	}

	public SoundHandle(SoundInstance sfx)
	{
		id = sfx.id;
		sound = sfx.sound;
	}
}