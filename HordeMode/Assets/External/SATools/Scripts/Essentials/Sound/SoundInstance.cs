using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class SoundInstance : MonoBehaviour
{
	#region Types
	public class OneShotData
	{
		public float? startedFadingOutAt;
	}

	public class LoopingData
	{
		public bool fadingOut;
		public float fadeInTimer;
		public float fadeOutTimer;
	}
	#endregion // Types

	#region Fields
	public int id;
	public Sound sound;
	public AudioSource source;

	public OneShotData oneShotData = new OneShotData();
	public LoopingData loopingData = new LoopingData();

	// Fade-in mixed with fade-out
	public float fadeVolume;

	public Unit attachedToUnit;

	float volume;
	float pitch;

	float baseVolume;
	float basePitch;

	float timeScale;
	#endregion // Fields

	#region Properties
	public float length
	{
		get
		{
			return source.clip.length;
		}
	}

	public float time
	{
		get
		{
			if(!paused)
			{
				// The source has stopped playing, time will return 0
				if(!source.isPlaying && source.clip != null)
				{
					return source.clip.length;
				}
			}

			return source.time;
		}
		set { source.time = value; }
	}

	public float normTime
	{
		get
		{
			return source.clip == null ? 1.0f : time / source.clip.length;
		}

		set
		{
			if(source.clip != null)
			{
				source.time = source.clip.length * value;
			}
		}
	}

	public bool paused
	{
		get;
		private set;
	}
	#endregion // Properties

	#region Mono
	protected void OnDestroy()
	{
		if(SoundManager.instance != null)
		{
			SoundManager.instance.OnSoundInstanceDestroyed(this);
		}
	}
	#endregion // Mono

	#region Methods
	public void Reset(
		Sound sound,
		SoundChannel channel,
		float timeScale
	)
	{
		this.sound = sound;
		this.baseVolume = sound.volume.GetRandom();
		this.basePitch = sound.pitch.GetRandom();

		if(channel != null)
		{
			this.source.outputAudioMixerGroup = channel.mixerGroup;
		}

		this.source.clip = sound.clips.RandomItem();
		this.source.loop = sound.loop;

		this.source.spatialBlend = sound.force2D ? 0.0f : 1.0f;

		fadeVolume = sound == null || sound.fadeInTime <= 0.0f ? 1.0f : 0.0f;

		oneShotData.startedFadingOutAt = null;
		loopingData.fadingOut = false;
		loopingData.fadeInTimer = loopingData.fadeOutTimer = 0.0f;

		SetVolume(1.0f);
		SetPitch(1.0f);
		SetTimeScale(timeScale);
	}

	public void SetPaused(bool paused)
	{
		this.paused = paused;
		if(paused) { source.Pause(); }
		else { source.UnPause(); }
	}

	public void SetVolume(float value)
	{
		volume = value;
		RefreshSourceVolume();
	}

	public void RefreshSourceVolume()
	{
		float targetVolume = fadeVolume * baseVolume * volume;
		if(source.volume != targetVolume) { source.volume = targetVolume; }
	}

	public void SetPitch(float value)
	{
		pitch = value;
		UpdatePitch();
	}

	public void SetTimeScale(float timeScale)
	{
		this.timeScale = timeScale;
		UpdatePitch();
	}

	void UpdatePitch()
	{
		float finalPitch = basePitch * pitch;
		if(!sound.bypassTimeScale) { finalPitch *= timeScale; }

		source.pitch = finalPitch;
	}
	#endregion // Methods
}
