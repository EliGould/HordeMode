using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "SA/Effect/Effect")]
public class Effect : ScriptableObject
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
	[Serializable]
	public class Settings
	{
		[SerializeField]
		[Tooltip("How long until the effect is fully removed after being stopped.")]
		public float decayTime = 1.0f;
		[SerializeField]
		[Tooltip("If the effect should be affected by pausing. Useful for GUI effect.")]
		public bool useUnscaledTime;
	}

	[Serializable]
	public class SoundSettings
	{
		[SerializeField]
		[Tooltip("Sounds that are started when the effect is first played. Any looping sounds will be stopped when decay starts.")]
		public Sound[] duringPlay;

		[SerializeField]
		[Tooltip("Sounds that are started when the effect starts decaying. Any looping sound will be stopped when decaying ends.")]
		public Sound[] onDecay;
	}

	[Serializable]
	public class SpawnSettings
	{
		[SerializeField]
		[Tooltip("If the effect should align with its parent when spawned.")]
		public bool inheritParentRotation;
	}

	[Serializable]
	public class OneOffSettings
	{
		[SerializeField,
		Tooltip("How long the effect runs before being stopped.")]
		public float duration = 2.0f;
	}
#pragma warning restore 0649
	#endregion // Serialized Types
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField]
	public GameObject prefab;
	[SerializeField]
	public bool loop;
	[SerializeField]
	public Settings settings;
	[SerializeField]
	public SpawnSettings spawn;
	[SerializeField]
	public SoundSettings sounds;
	[SerializeField, ValueRestrict("loop", false)]
	public OneOffSettings oneOffSettings;
	#pragma warning restore 0649
	#endregion // Serialized Fields
	#endregion // Fields

	#region Properties
	#endregion // Properties
}