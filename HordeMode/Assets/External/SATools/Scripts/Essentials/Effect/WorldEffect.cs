using UnityEngine;
using UE = UnityEngine;
using UnityEngine.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

public class WorldEffect : SafeBehaviour
{
	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField, InlineObject]
	Effect effect;
	[SerializeField]
	[Tooltip("If no parent node is found, the WorldEffect node is used instead")]
	FindTransField parent;
	[SerializeField]
	[Tooltip("If set, the effect will play automatically on enable, then stop automatically on disable.")]
	bool playAutomatically = true;
	[SerializeField]
	Unit attachToUnit;
#pragma warning restore 0649
	#endregion // Serialized Fields

	EffectHandle handle;
	bool isSetup;
	#endregion // Fields

	#region Properties
	public bool isLooping
	{
		get { return effect == null ? false : effect.loop; }
	}
	#endregion // Properties

	#region Mono
	protected override void AtAwake()
	{
		Dbg.LogErrorIf(effect == null, this, "{0} was missing an effect", this);
	}

	protected override void AtEnable()
	{
		parent.SetupWithSelf(transform);

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
	public void Play()
	{
		var effectMan = EffectManager.instance;
		if(effect != null && !effectMan.IsPlaying(handle))
		{
			Transform parent = this.parent.component;
			if(parent == null)
			{
				parent = transform;
			}

			handle = effectMan.Play(
				effect,
				EffectFlag.OneShot | EffectFlag.Looping,
				parent,
				attachToUnit: attachToUnit
			);
		}
	}

	public void Stop(bool disableAutoPlay = true)
	{
		if(handle.hasValue)
		{
			EffectManager.instance.Stop(handle);
			handle = new EffectHandle();
		}

		if(disableAutoPlay)
		{
			playAutomatically = false;
		}
	}

	public void Pause()
	{
		SetPaused(true);
	}

	public void Unpause()
	{
		SetPaused(false);
	}

	public void SetPaused(bool paused)
	{
		EffectManager.instance.SetPaused(handle, paused);
	}
	#endregion // Methods

	#region Editor
	protected void OnValidate()
	{
		if(handle.hasValue)
		{
			Stop();
			Play();
		}
	}

	//protected void OnDrawGizmos()
	//{
	//	Gizmos.DrawIcon(transform.position, "WorldEffect icon.png", allowScaling: false);
	//}
	#endregion // Editor
}
