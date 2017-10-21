using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

public class WorldSound : SafeBehaviour
{
	#region Types
	enum TimeMode
	{
		Normal,
		RandomRangeTime,
		RandomRangeNormTime,
	}
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField, InlineObject]
	Sound sound;
	[SerializeField]
	[Tooltip("Selects at what time the sound will start.")]
	TimeMode startTimeMode;
	[SerializeField, EnumRestrict("startTimeMode", TimeMode.RandomRangeNormTime, TimeMode.RandomRangeTime)]
	FloatRange startTimeRange;
	[SerializeField]
	[Tooltip("A transform that the sound will be parented to. If this is not set, the object's transform is used.")]
	Transform parent;
	// TODO
	[SerializeField]
	[Tooltip("If the sound should be attached to an Npc and follow it when entering/exiting buildings.")]
	Unit attachToUnit;

	[SerializeField, Space(5.0f)]
	bool playAutomatically;
#pragma warning restore 0649
	#endregion // Serialized Fields

	SoundHandle handle;
	#endregion // Fields

	#region Properties
	public bool isLooping
	{
		get { return sound.loop; }
	}
	#endregion // Properties

	#region Mono
	protected override void AtEnable()
	{
		if(playAutomatically)
		{
			Play();
		}
	}

	protected override void AtDisable()
	{
		Stop(disableAutoPlay: false);
	}
	#endregion // Mono

	#region Methods
	public bool IsPlaying()
	{
		return SoundManager.instance.IsPlaying(handle);
	}

	public void Play()
	{
		if(!IsPlaying())
		{
			handle = SoundManager.instance.Play(
				sound,
				SoundFlag.OneShot | SoundFlag.Looping,
				parent == null ? transform : parent
			);

			switch(startTimeMode)
			{
			case TimeMode.RandomRangeNormTime:
				SoundManager.instance.SetNormTime(handle, startTimeRange.GetRandom());
				break;

			case TimeMode.RandomRangeTime:
				SoundManager.instance.SetTime(handle, startTimeRange.GetRandom());
				break;
			default: break;
			}
		}
	}

	public void Stop(bool disableAutoPlay = true)
	{
		SoundManager.instance.Stop(handle);
		handle = new SoundHandle();

		if(disableAutoPlay)
		{
			playAutomatically = false;
		}
	}

	public void SetVolume(float volume)
	{
		SoundManager.instance.SetVolume(handle, volume);
	}

	public void SetPaused(bool paused)
	{
		SoundManager.instance.SetPaused(handle, paused);
	}

	public void Pause()
	{
		SetPaused(true);
    }

	public void Unpause()
	{
		SetPaused(false);
	}
	#endregion // Methods
}
