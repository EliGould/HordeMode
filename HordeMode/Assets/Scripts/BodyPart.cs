using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class BodyPart : MonoBehaviour
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

	Rigidbody rigid;
	#endregion // Fields

	#region Properties
	public new SkinnedMeshRenderer renderer
	{
		get;
		private set;
	}
	#endregion // Properties

	#region Methods
	protected void Awake()
	{
		renderer = GetComponent<SkinnedMeshRenderer>();
		Dbg.LogWarnIf(renderer == null, this, "{0} did not have SkinnedMeshRenderer", this);

		if(renderer == null) { return; }

		//renderer.rootBone.parent = transform;
		rigid = renderer.rootBone.gameObject.AddComponent<Rigidbody>();
		rigid.useGravity = false;

		renderer.rootBone.parent = transform.parent;

		Bounds rendBounds = renderer.localBounds;
		var collider = rigid.gameObject.AddComponent<BoxCollider>();
		collider.center = rendBounds.center;
		collider.size = rendBounds.size;

		//renderer.enabled = false;
	}

	public void Attach()
	{
		if(rigid == null) { return; }
	}

	public void Detach()
	{
		if(rigid == null) { return; }

	}
	#endregion // Methods
}
