using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class Bitcrusher : MonoBehaviour
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
	[SerializeField, Range(1, 64)]
	[Tooltip("The sample interval of the crusher. The higher the value, the crushier.")]
	int interval = 4;
	[SerializeField, Range(0.0f, 1.0f)]
	[Tooltip("How much of the effect apply.")]
	float mix = 1.0f;
#pragma warning restore 0649
	#endregion // Serialized Fields

	int counter;
	float sampled;
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	protected void OnAudioFilterRead(float[] data, int channels)
	{
		for(int i = 0; i < data.Length; ++i)
		{
			if((counter++ % interval) == 0)
			{
				sampled = data[i];
			}

			data[i] = data[i] * (1.0f - mix) + sampled * mix;
		}
	}
	#endregion // Mono

	#region Methods
	#endregion // Methods
}
