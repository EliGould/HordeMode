using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed partial class Unit : UnitBase
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
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Methods
	protected override void AtPreRegister()
	{
		manState.homePoint = transform.position;
		manState.faction = sceneData.startFaction;
		manState.weaponData.parentNode = transform.FindDeep(def.attackData.weaponNodeName);

		parts.visual.GetComponentsInChildren<SkinnedMeshRenderer>(
			includeInactive: true,
			result: parts.renderers
		);

		for(int i = 0; i < parts.renderers.Count; i++)
		{
			var rend = parts.renderers[i];

			if(def.texture != null)
			{
				rend.material.mainTexture = def.texture;
			}

			Bounds rendBounds = rend.bounds;

			Transform rootBone = rend.rootBone;

			var collider = rootBone.gameObject.AddComponent<BoxCollider>();

			collider.center = rootBone.InverseTransformPoint(rendBounds.center);
			collider.size = rootBone.InverseTransformVector(rendBounds.size);
		}

		var copy = Instantiate(parts.visual, parts.visual.transform.parent);
		Transform copyRoot = copy.transform;
		// Destroy NestedPrefab component
		Destroy(copy);

		copyRoot.GetComponentsInChildren<SkinnedMeshRenderer>(
			includeInactive: true,
			result: parts.rigidRenderers
		);

		parts.rigidRoot = copyRoot;

		for(int i = 0; i < parts.rigidRenderers.Count; i++)
		{
			var rend = parts.renderers[i];
			var rigidRend = parts.rigidRenderers[i];

			if(def.texture != null)
			{
				rigidRend.material.mainTexture = def.texture;
			}
		}
	}
	#endregion // Methods
}
