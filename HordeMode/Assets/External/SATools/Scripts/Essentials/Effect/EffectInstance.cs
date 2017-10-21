using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public struct TransformValues
{
	public Vector3 position;
	public Quaternion rotation;
	public Vector3 scale;
}

// TODO: Simulate effects in unscaled time
public class EffectInstance : MonoBehaviour
{
	public int id;
	public Effect effect;

	public Transform visualRoot;
	public Unit attachedToUnit;

	public float lifeTime;
	public float? decayTimeLeft;

	List<SoundHandle> onPlaySfx = new List<SoundHandle>();
	List<SoundHandle> onDecaySfx = new List<SoundHandle>();

	List<ParticleSystem> particleSystems = new List<ParticleSystem>();

	public bool paused
	{
		get;
		private set;
	}

	#region Mono
	protected void OnDestroy()
	{
		// TODO
		if(EffectManager.instance != null)
		{
			EffectManager.instance.OnEffectInstanceDestroyed(this);
		}
	}
	#endregion // Mono

	#region Setup
	public void Setup()
	{
		GetComponentsInChildren<ParticleSystem>(particleSystems);
	}
	#endregion // Setup

	#region Resetting
	public void Reset(Effect effect)
	{
		this.effect = effect;

		lifeTime = 0.0f;
		decayTimeLeft = null;

		visualRoot.localPosition = Vector3.zero;
		visualRoot.localRotation = Quaternion.identity;
		visualRoot.localScale = Vector3.one;

		RemoveSounds(onPlaySfx);
		RemoveSounds(onDecaySfx);
	}
	#endregion // Resetting

	public void Play()
	{
		PlaySounds(effect.sounds.duringPlay, onPlaySfx);

		for(int i = 0; i < particleSystems.Count; ++i)
		{
			ParticleSystem sys = particleSystems[i];
			sys.Clear();
			sys.Play();
			var emission = sys.emission;
			emission.enabled = !paused;
		}
	}

	public void Decay()
	{
		SetEmissionEnabled(false);

		StopSounds(onPlaySfx);
		PlaySounds(effect.sounds.onDecay, onDecaySfx);
	}

	public void SetPaused(bool paused)
	{
		this.paused = paused;

		SetEmissionEnabled(!paused);
		SetSoundsPaused(onPlaySfx, paused);
	}

	#region Particles
	void SetEmissionEnabled(bool enabled)
	{
		for(int i = 0; i < particleSystems.Count; ++i)
		{
			ParticleSystem system = particleSystems[i];
			var emission = system.emission;

			emission.enabled = enabled;
		}
	}
	#endregion // Particles

	#region SFX
	public void PlaySounds(Sound[] sounds, List<SoundHandle> sfx)
	{
		if(sounds == null) { return; }

		for(int i = 0; i < sounds.Length; ++i)
		{
			PlaySound(sounds[i], sfx);
		}
	}

	void PlaySound(Sound sound, List<SoundHandle> sfx)
	{
		SoundHandle handle = SoundManager.instance.Play(
			sound,
			SoundFlag.OneShot | SoundFlag.Looping,
			parent: transform,
			attachToUnit: attachedToUnit
		);
		if(handle.hasValue)
		{
			sfx.Add(handle);
		}
	}

	public void StopSounds(List<SoundHandle> sfx)
	{
		var soundMan = SoundManager.instance;
		for(int i = 0; i < sfx.Count; ++i)
		{
			SoundHandle handle = sfx[i];
			soundMan.Stop(handle);
		}
	}

	public void RemoveSounds(List<SoundHandle> sfx)
	{
		var soundMan = SoundManager.instance;
		for(int i = 0; i < sfx.Count; ++i)
		{
			soundMan.Remove(sfx[i]);
		}
		sfx.Clear();
	}

	void SetSoundsPaused(List<SoundHandle> sfx, bool paused)
	{
		var soundMan = SoundManager.instance;
		for(int i = 0; i < sfx.Count; ++i)
		{
			soundMan.SetPaused(sfx[i], paused);
		}
	}
	#endregion // SFX
}
