using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class SoundManager : MonoBehaviour
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
#pragma warning restore 0649
	#endregion // Serialized Types
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
#pragma warning restore 0649
	#endregion // Serialized Fields

	SoundSettings settings;

	DynamicPool<SoundInstance> soundPool;

	Dictionary<int, SoundInstance> idToSound;
	List<SoundInstance> playingSounds;

	int idCounter;
	#endregion // Fields

	#region Properties
	public static SoundManager instance
	{
		get;
		private set;
	}

	public SoundChannel defaultChannel
	{
		get { return settings.defaultChannelSettings; }
	}

	public SoundChannels channels
	{
		get;
		private set;
	}
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	public static SoundManager Setup(SoundSettings settings, SoundChannels channels)
	{
		instance = new GameObject("SoundManager").AddComponent<SoundManager>();

		instance.settings = settings;
		instance.channels = channels;

		instance.idToSound = new Dictionary<int, SoundInstance>();
		instance.playingSounds = new List<SoundInstance>();

		instance.idCounter = 0;

		instance.soundPool = new DynamicPool<SoundInstance>(() =>
		{
			var sound = new GameObject("SFX").AddComponent<SoundInstance>();

			sound.source = sound.gameObject.AddComponent<AudioSource>();

			sound.transform.parent = instance.transform;
			sound.gameObject.SetActive(false);
			
			return sound;
		});

		instance.soundPool.onFree += (SoundInstance sound) =>
		{
			sound.source.Stop();
			sound.source.clip = null;
			sound.id = 0;

			sound.transform.parent = instance.transform;
			sound.attachedToUnit = null;

			sound.gameObject.SetActive(false);
		};

		return instance;
	}

	public void SystemUpdate()
	{
		for(int i = playingSounds.Count - 1; i >= 0; --i)
		{
			var sfx = playingSounds[i];

			//DbgValues.Set(sfx, "time", sfx.time);
			//DbgValues.Set(sfx, "length", sfx.length);
			//DbgValues.Set(sfx, "paused", sfx.paused);

			UpdateSoundInstance(sfx);
		}
	}

	void UpdateSoundInstance(SoundInstance sfx)
	{
		if(!sfx.sound.loop && sfx.normTime >= 1.0f)
		{
			RemoveSound(sfx, wasDestroyed: false);
			return;
		}

		float fadeInMix = 1.0f;
		float fadeOutMix = 1.0f;

		if(sfx.sound.loop)
		{
			// TODO: Not sure what to use for delta time. Think normal one makes most sense
			if(sfx.loopingData.fadingOut)
			{
				sfx.loopingData.fadeOutTimer += Time.deltaTime;

				float fadeOutAlpha = sfx.loopingData.fadeOutTimer / sfx.sound.fadeOutTime;
				fadeOutMix = 1.0f - Mathf.Clamp01(fadeOutAlpha);

				if(sfx.loopingData.fadeOutTimer >= sfx.sound.fadeOutTime)
				{
					RemoveSound(sfx, wasDestroyed: false);
					return;
				}
			}
			
			if(sfx.sound.fadeInTime > 0.0f)
			{
				sfx.loopingData.fadeInTimer += Time.deltaTime;

				float fadeInAlpha = sfx.loopingData.fadeInTimer / sfx.sound.fadeInTime;
				fadeInMix = Mathf.Clamp01(fadeInAlpha);
			}
		}
		else
		{
			float fadeOutStart = sfx.oneShotData.startedFadingOutAt ?? sfx.length - sfx.sound.fadeOutTime;
			if(sfx.time >= fadeOutStart)
			{
				if(sfx.oneShotData.startedFadingOutAt == null)
				{
					sfx.oneShotData.startedFadingOutAt = sfx.time;
				}
				float fadeOutAlpha = Mathf.InverseLerp(
					fadeOutStart,
					fadeOutStart + sfx.sound.fadeOutTime,
					sfx.time
				);
				fadeOutMix = 1.0f - Mathf.Clamp01(fadeOutAlpha);
				if(fadeOutMix <= 0.0f)
				{
					RemoveSound(sfx, wasDestroyed: false);
					return;	
				}
			}

			float fadeInAlpha = sfx.time / sfx.sound.fadeInTime;
			fadeInMix = Mathf.Clamp01(fadeInAlpha);
		}

		sfx.fadeVolume = fadeInMix * fadeOutMix;
		//DbgValues.Set(sfx, "fade in mix", fadeInMix);
		//DbgValues.Set(sfx, "fade out mix", fadeOutMix);
		//DbgValues.Set(sfx, "fade volume", sfx.fadeVolume);		
		sfx.RefreshSourceVolume();
	}

	public void Shutdown()
	{
		Destroy(gameObject);
		instance = null;
	}

	public void OnTimeScaleChanged(float newTimeScale)
	{
		for(int i = 0; i < playingSounds.Count; i++)
		{
			SoundInstance sfx = playingSounds[i];
			sfx.SetTimeScale(newTimeScale);
		}
	}

	#region Interface
	public SoundHandle Play(
		Sound sound,
		SoundFlag flags,
		Transform parent = null,
		Vector3? worldPos = null,
		Unit attachToUnit = null
	)
	{
		if(sound == null) { return new SoundHandle(); }
		if(sound.clips.Length == 0) { return new SoundHandle(); }

		if(!sound.loop && 0 == (flags & SoundFlag.OneShot))
		{
			Dbg.LogWarnOnce(sound, "Tried to play {0} as one shot", sound);
			return new SoundHandle();
		}

		if(sound.loop && 0 == (flags & SoundFlag.Looping))
		{
			Dbg.LogWarnOnce(sound, "Tried to play {0} as looping", sound);
			return new SoundHandle();
		}

		string channelName = sound.channel.name;
		SoundChannel channel = channels.GetValue(channelName);
		if(channel == null)
		{
			channel = settings.defaultChannelSettings;
		}

		var sfx = GetSfxInst();

		sfx.sound = sound;
#if UNITY_EDITOR
		string name = GarbageCache.GetName(sound);
		if(!sfx.name.EndsWith(name))
		{
			sfx.name = "SFX_" + name;
		}
#endif // UNITY_EDITOR
		if(parent != null)
		{
			sfx.gameObject.transform.parent = parent;
		}

		sfx.attachedToUnit = attachToUnit;
		sfx.transform.position = worldPos ?? (parent == null ? Vector3.zero : parent.position);

		sfx.Reset(sound, channel, timeScale: App.timeScale);
		sfx.source.Play();

		return new SoundHandle(sfx);
	}

	public void Stop(SoundHandle handle)
	{
		var sfx = GetSfx(handle);

		if(sfx != null)
		{
			if(sfx.sound.fadeOutTime <= 0.0f)
			{
				Remove(handle);
				return;
			}

			if(sfx.sound.loop)
			{
				sfx.loopingData.fadingOut = true;
			}
			else if(sfx.oneShotData.startedFadingOutAt == null)
			{
				// Only start fading out if before where it would
				// by default
				float decayStart = sfx.length - sfx.sound.fadeOutTime;
				if(sfx.time < decayStart)
				{
					sfx.oneShotData.startedFadingOutAt = sfx.time;
				}
			}
		}
	}

	public void Remove(SoundHandle handle)
	{
		var sfx = GetSfx(handle);

		if(sfx != null)
		{
			RemoveSound(sfx, wasDestroyed: false);
		}
	}

	public void RemoveAll(Unit owner)
	{
		for(int i = playingSounds.Count - 1; i >= 0; i--)
		{
			var sfx = playingSounds[i];
			if(sfx.attachedToUnit == owner)
			{
				RemoveSound(sfx, wasDestroyed: false);
			}
		}
	}

	public bool IsPlaying(SoundHandle handle)
	{
		return GetSfx(handle) != null;
	}

	public void SetPaused(SoundHandle handle, bool paused)
	{
		var sfx = GetSfx(handle);

		if(sfx != null)
		{
			sfx.SetPaused(paused);
		}
	}

	public void SetVolume(SoundHandle handle, float volume)
	{
		var sfx = GetSfx(handle);

		if(sfx != null)
		{
			sfx.SetVolume(volume);
		}
	}

	public void SetPitch(SoundHandle handle, float pitch)
	{
		var sfx = GetSfx(handle);

		if(sfx != null)
		{
			sfx.SetVolume(pitch);
		}
	}

	public void SetNormTime(SoundHandle handle, float normTime)
	{
		var sfx = GetSfx(handle);

		if(sfx != null)
		{
			sfx.normTime = normTime;
		}
	}

	public void SetTime(SoundHandle handle, float time)
	{
		var sfx = GetSfx(handle);

		if(sfx != null)
		{
			sfx.time = time;
		}
	}

	public void Clear()
	{
		for(int i = playingSounds.Count - 1; i >= 0; --i)
		{
			var sfx = playingSounds[i];

			Destroy(sfx.gameObject);

			soundPool.RemoveUnusable(sfx);
		}

		playingSounds.Clear();
		idToSound.Clear();
	}
	#endregion // Interface

	public void OnSoundInstanceDestroyed(SoundInstance sfx)
	{
		Dbg.LogWarnIf(idToSound.ContainsKey(sfx.id),
			sfx, "{0} was destroyed before whatever was using it cleaned it up", sfx
		);
		RemoveSound(sfx, wasDestroyed: true);
	}

	void RemoveSound(SoundInstance sfx, bool wasDestroyed)
	{
		playingSounds.Remove(sfx);
		idToSound.Remove(sfx.id);

		ReturnSfxInst(sfx, wasDestroyed);
	}

	SoundInstance GetSfx(SoundHandle handle)
	{
		return GetSfx(handle.id);
	}

	SoundInstance GetSfx(int id)
	{
		SoundInstance sfx = idToSound.FindOrNull(id);
		if(sfx != null)
		{
			if(sfx.sound.loop && sfx.loopingData.fadingOut)
			{
				return null;
			}
			if(!sfx.sound.loop && sfx.oneShotData.startedFadingOutAt != null)
			{
				return null;
			}
		}

		return sfx;
	}

	SoundInstance GetSfxInst()
	{
		var sfx = soundPool.Get();
		sfx.id = ++idCounter;
		idToSound[sfx.id] = sfx;
		playingSounds.Add(sfx);
        sfx.gameObject.SetActive(true);
		return sfx;
	}

	void ReturnSfxInst(SoundInstance sfx, bool wasDestroyed)
	{
		if(wasDestroyed)
		{
			soundPool.RemoveUnusable(sfx);
		}
		else
		{
			soundPool.Free(sfx);
		}
	}
	#endregion // Methods
}
