using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

public sealed class BodyParts : MonoBehaviour
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
	[Serializable]
	public class PartDef
	{
		[SerializeField]
		public bool lifeCritical;
		[SerializeField]
		public int hitPoints = 5;
		[SerializeField]
		public FindSkinnedMeshRendField rend;
	}

	[Serializable]
	class PartDefs : EnumItems<PartKind, PartDef>
	{
#if UNITY_EDITOR
		[CustomPropertyDrawer(typeof(PartDefs))]
		class ItemsDrawer : Drawer
		{
		}
#endif // UNITY_EDITOR
	}
#pragma warning restore 0649
	#endregion // Serialized Types

	public class NodeData
	{
		public SkinnedMeshRenderer rend;
		public Rigidbody rigid;
		public Transform rootBone;
		public BoxCollider coll;
	}

	public class PartData
	{
		public PartKind kind;

		public bool isAttached;
		public NodeData attached = new NodeData();
		public NodeData detached = new NodeData();

		public int hitPoints;
	}

	class Parts : EnumItems<PartKind, PartData>
	{
		public Parts() : base()
		{
			items.FillNew();
		}
		
#if UNITY_EDITOR
		[CustomPropertyDrawer(typeof(Parts))]
		class ItemsDrawer : Drawer
		{
		}
#endif // UNITY_EDITOR
	}

	public enum PartKind
	{
		Head,
		Torso,
		LeftArm,
		RightArm,
		LeftLeg,
		RightLeg,
	}
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField]
	PartDefs partDefs = new PartDefs();
#pragma warning restore 0649
	#endregion // Serialized Fields

	Dictionary<Collider, PartData> attachedCollToPart = new Dictionary<Collider, PartData>();
	Parts parts = new Parts();
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Methods
	public void Setup(Unit unit)
	{
		for(int i = 0; i < EnumHelper<PartKind>.count; i++)
		{
			PartKind kind = EnumHelper<PartKind>.values[i];

			PartDef def = partDefs[kind];
			PartData data = parts[kind];

			data.kind = kind;
			data.hitPoints = def.hitPoints;

			SkinnedMeshRenderer rend = def.rend.SetupWithSelf(transform);

			var rigidRend = unit.parts.rigidRoot.FindDeep(rend.name).GetComponent<SkinnedMeshRenderer>();

			SetupNode(rend, data.attached);
			SetupNode(rigidRend, data.detached);

			// Add a rigid to not count as static collider
			// Don't assign since it should never not be kinematic
			Rigidbody attachedRigid = data.attached.rootBone.gameObject.AddComponent<Rigidbody>();
			attachedRigid.isKinematic = true;
			attachedRigid.useGravity = false;

			data.detached.rigid = data.detached.rootBone.gameObject.AddComponent<Rigidbody>();

			// Detached arts needs to be in root, since parts are childed under eachother
			data.detached.rootBone.parent = rigidRend.transform.parent;

			attachedCollToPart[data.attached.coll] = data;
        }

		ReattachAll();
    }

	void SetupNode(SkinnedMeshRenderer rend, NodeData nodeData)
	{
		nodeData.rend = rend;
		nodeData.coll = rend.rootBone.GetComponent<BoxCollider>();
		nodeData.rootBone = rend.rootBone;
	}

	public PartData GetPartDataForCollider(Collider coll)
	{
		return attachedCollToPart.FindOrNull(coll);
	}

	public PartDef GetPartDefForKind(PartKind kind)
	{
		return partDefs[kind];
	}

	public void GetAttachedColliders(List<Collider> results)
	{
		foreach(var kvp in attachedCollToPart)
		{
			Collider coll = kvp.Key;

			results.Add(coll);
		}
	}

	public void DetachAll()
	{
		SetAllAttached(false);
	}

	public void ReattachAll()
	{
		SetAllAttached(true);
	}

	public void Detach(PartData partData)
	{
		SetAttached(partData, false);
	}

	void SetAllAttached(bool attached)
	{
		for(int i = 0; i < parts.items.Length; i++)
		{
			PartData part = parts.items[i];
			SetAttached(part, attached);
		}
	}

	void SetAttached(PartData part, bool attached)
	{
		if(part.isAttached == attached) { return; }

		PartDef partDef = GetPartDefForKind(part.kind);

		part.hitPoints = attached ? partDef.hitPoints : 0;
		part.isAttached = attached;

		SetActive(part.attached, attached);
		SetActive(part.detached, !attached);

		part.detached.coll.enabled = false;
	}

	void SetActive(NodeData node, bool active)
	{
		node.coll.enabled = active;
		node.rend.enabled = active;

		if(node.rigid != null)
		{
			if(!active)
			{
				node.rigid.velocity = node.rigid.angularVelocity = Vector3.zero;
			}

			node.rigid.isKinematic = !active;
			node.rigid.useGravity = active;
		}
	}
	#endregion // Methods
}
