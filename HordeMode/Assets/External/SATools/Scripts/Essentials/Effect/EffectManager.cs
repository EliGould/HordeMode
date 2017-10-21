using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class EffectManager : MonoBehaviour
{
	#region Types
	#endregion // Types

	#region Fields
	//EffectSettings settings;

	GameObject noPrefabDummy;
	Dictionary<GameObject, DynamicPool<EffectInstance>> prefabToEffectPool;

	Dictionary<int, EffectInstance> idToEffect;
	List<EffectInstance> playingEffects;

	int idCounter;
	#endregion // Fields

	#region Properties
	public static EffectManager instance
	{
		get;
		private set;
	}
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	public static EffectManager Setup(EffectSettings settings)
	{
		instance = new GameObject("EffectManager").AddComponent<EffectManager>();

		//instance.settings = settings;

		instance.idToEffect = new Dictionary<int, EffectInstance>();
		instance.playingEffects = new List<EffectInstance>();

		instance.idCounter = 0;

		instance.prefabToEffectPool = new Dictionary<GameObject, DynamicPool<EffectInstance>>();

		instance.noPrefabDummy = new GameObject("NoPrefabEffectDummy");
		instance.noPrefabDummy.hideFlags = HideFlags.HideAndDontSave;

		return instance;
	}

	public void SystemUpdate()
	{
		for(int i = playingEffects.Count - 1; i >= 0; --i)
		{
			var efx = playingEffects[i];

			UpdateEffectInstance(efx);
		}
	}

	void UpdateEffectInstance(EffectInstance efx)
	{
		float dt = efx.effect.settings.useUnscaledTime ?
			Time.unscaledDeltaTime :
			Time.deltaTime
		;

		if(efx.decayTimeLeft != null)
		{
			efx.decayTimeLeft -= dt;
			if(efx.decayTimeLeft <= 0.0f)
			{
				RemoveEffect(efx, wasDestroyed: false);
			}
			return;
		}

		efx.lifeTime += dt;
		if(!efx.effect.loop)
		{
			if(efx.lifeTime >= efx.effect.oneOffSettings.duration)
			{
				StopEffect(efx);
			}
		}
	}

	public void Shutdown()
	{
		Destroy(gameObject);
		instance = null;
	}

	public void OnTimeScaleChanged(float newTimeScale)
	{
		//for(int i = 0; i < playingEffects.Count; i++)
		//{
		//	EffectInstance efx = playingEffects[i];
		//	efx.SetTimeScale(newTimeScale);
		//}
	}

	#region Interface
	public EffectHandle Play(
		Effect effect,
		EffectFlag flags,
		Transform parent = null,
		Vector3? worldPos = null,
		Quaternion? worldRot = null,
		Unit attachToUnit = null
	)
	{
		if(effect == null) { return new EffectHandle(); }

		if(!effect.loop && 0 == (flags & EffectFlag.OneShot))
		{
			Dbg.LogWarnOnce(effect, "Tried to play {0} as one shot", effect);
			return new EffectHandle();
		}

		if(effect.loop && 0 == (flags & EffectFlag.Looping))
		{
			Dbg.LogWarnOnce(effect, "Tried to play {0} as looping", effect);
			return new EffectHandle();
		}

		var efx = GetEfxInst(effect.prefab);

		efx.Reset(effect);

#if UNITY_EDITOR
		string name = GarbageCache.GetName(effect);
		if(!efx.name.EndsWith(name))
		{
			efx.name = "EFX_" + name;
		}
#endif // UNITY_EDITOR
		if(parent != null)
		{
			efx.gameObject.transform.parent = parent;
		}

		efx.attachedToUnit = attachToUnit;
		efx.transform.position = DetermineWorldPos(parent, worldPos, effect);
		efx.transform.rotation = DetermineWorldRotation(parent, worldRot, effect);

		efx.Play();

		return new EffectHandle(efx);
	}

	Vector3 DetermineWorldPos(Transform parent, Vector3? worldPos, Effect effect)
	{
		if(worldPos != null) { return worldPos.Value; }
		if(parent != null) { return parent.position; }
		return Vector3.zero;
	}

	Quaternion DetermineWorldRotation(Transform parent, Quaternion? worldRot, Effect effect)
	{
		Effect.SpawnSettings settings = effect.spawn;

		return worldRot != null ? worldRot.Value :
			settings.inheritParentRotation ? parent.rotation :
			Quaternion.identity
		;
	}

	public void Stop(EffectHandle handle)
	{
		var efx = GetEfx(handle);

		if(efx != null)
		{
			StopEffect(efx);
		}
	}

	void StopEffect(EffectInstance efx)
	{
		if(efx.effect.settings.decayTime <= 0.0f)
		{
			RemoveEffect(efx, wasDestroyed: false);
			return;
		}

		if(efx.decayTimeLeft == null)
		{
			efx.decayTimeLeft = efx.effect.settings.decayTime;
			efx.Decay();
		}
	}

	public void Remove(EffectHandle handle)
	{
		var efx = GetEfx(handle);

		if(efx != null)
		{
			RemoveEffect(efx, wasDestroyed: false);
		}
	}

	public void RemoveAll(Unit owner)
	{
		for(int i = playingEffects.Count - 1; i >= 0; i--)
		{
			var efx = playingEffects[i];
			if(efx.attachedToUnit == owner)
			{
				RemoveEffect(efx, wasDestroyed: false);
			}
		}
	}

	public bool IsPlaying(EffectHandle handle)
	{
		return GetEfx(handle) != null;
	}

	public void SetPaused(EffectHandle handle, bool paused)
	{
		var efx = GetEfx(handle);

		if(efx != null)
		{
			efx.SetPaused(paused);
		}
	}

	public void Clear()
	{
		for(int i = playingEffects.Count - 1; i >= 0; --i)
		{
			var efx = playingEffects[i];

			Destroy(efx.gameObject);

			RemoveEffect(efx, wasDestroyed: true);
		}

		playingEffects.Clear();
		idToEffect.Clear();
	}
	#endregion // Interface

	public void OnEffectInstanceDestroyed(EffectInstance efx)
	{
		Dbg.LogWarnIf(idToEffect.ContainsKey(efx.id),
			efx, "{0} was destroyed before whatever was using it cleaned it up", efx
		);
		RemoveEffect(efx, wasDestroyed: true);
	}

	void RemoveEffect(EffectInstance efx, bool wasDestroyed)
	{
		playingEffects.Remove(efx);
		idToEffect.Remove(efx.id);

		ReturnEfxInst(efx, wasDestroyed);
	}

	EffectInstance GetEfx(EffectHandle handle)
	{
		return GetEfx(handle.id);
	}

	EffectInstance GetEfx(int id)
	{
		var efx = idToEffect.FindOrNull(id);
		if(efx != null)
		{
			// Currently decaying
			if(efx.decayTimeLeft != null)
			{
				return null;
			}
		}
		return efx;
	}

	EffectInstance GetEfxInst(GameObject prefab)
	{
		var pool = GetPoolFor(prefab);
		var efx = pool.Get();
		efx.id = ++idCounter;
		idToEffect[efx.id] = efx;
		playingEffects.Add(efx);
        efx.gameObject.SetActive(true);
		return efx;
	}

	void ReturnEfxInst(EffectInstance efx, bool wasDestroyed)
	{
		var pool = GetPoolFor(efx.effect.prefab);
        if(wasDestroyed)
		{
			pool.RemoveUnusable(efx);
		}
		else
		{
			pool.Free(efx);
		}
	}

	DynamicPool<EffectInstance> GetPoolFor(GameObject prefab)
	{
		if(prefab == null) { prefab = noPrefabDummy; }

		if(!prefabToEffectPool.ContainsKey(prefab))
		{
			var pool = new DynamicPool<EffectInstance>(() =>
			{
				var go = new GameObject("EFX");
				var efx = go.AddComponent<EffectInstance>();

				var visualRoot = GameObject.Instantiate(prefab);
				visualRoot.transform.parent = go.transform;
				efx.visualRoot = visualRoot.transform;

				efx.Setup();
				efx.Reset(effect: null);
				efx.gameObject.SetActive(false);
				return efx;
			});

			pool.onFree = (EffectInstance efx) =>
			{
				efx.Reset(effect: null);
				efx.transform.parent = instance.transform;
				efx.gameObject.SetActive(false);
			};

			prefabToEffectPool[prefab] = pool;
		}

		return prefabToEffectPool[prefab];
	}
	#endregion // Methods
}
