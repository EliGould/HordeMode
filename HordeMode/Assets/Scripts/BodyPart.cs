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
	public Rigidbody rigid;
	public SkinnedMeshRenderer rend;
	public Transform rootBone;
	public BoxCollider coll;

	public Vector3 origPos;
	public Quaternion origRot;

	public SkinnedMeshRenderer visualRend;
	public Transform visualRootBone;
	public Collider visualColl;


	#endregion // Fields

	#region Properties
	public bool isAttached
	{
		get;
		private set;
	}
	#endregion // Properties

	#region Methods
	public void Attach()
	{
		if(isAttached) { return; }

		isAttached = true;

		coll.enabled = false;

		rigid.isKinematic = true;
		rigid.velocity = rigid.angularVelocity = Vector3.zero;

		rend.enabled = false;

		rootBone.localPosition = origPos;
		rootBone.localRotation = origRot;

		visualColl.enabled = true;
		visualRend.enabled = true;
	}

	public void Detach()
	{
		if(!isAttached) { return; }

		isAttached = false;

		coll.enabled = true;

		rootBone.position = visualRootBone.position;
		rootBone.rotation = visualRootBone.rotation;

		rigid.isKinematic = false;
		rend.enabled = true;

		visualColl.enabled = false;
		visualRend.enabled = false;
	}
	#endregion // Methods
}
