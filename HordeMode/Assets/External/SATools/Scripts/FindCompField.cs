using UnityEngine;
using UE = UnityEngine;
using UnityEngine.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;

#region Base
// NOTE: This cannot be declare in FindCompField, causes silent editor crash
public enum FindCompKind
{
	Direct,
	FindUnderSelf,
	FindUnderNode,
}

public abstract class FindCompField<T>
	where T : Component
{
	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField]
	FindCompKind kind;
	[SerializeField, EnumRestrict("kind", FindCompKind.FindUnderNode)]
	[Tooltip("A parent to find the child on.")]
	Transform parent;
	[SerializeField, EnumRestrict("kind", FindCompKind.FindUnderNode, FindCompKind.FindUnderSelf)]
	[Tooltip("The name of the child node to find.")]
	string childName;
	[SerializeField]
	bool findInChildren = true;
	[SerializeField, EnumRestrict("kind", FindCompKind.FindUnderNode, FindCompKind.FindUnderSelf)]
	[Tooltip("If inactive children should be found.")]
	bool findInactive;
	[SerializeField, EnumRestrict("kind", FindCompKind.Direct)]
	[Tooltip("Component will be assigned directly.")]
	T directComponent;

	[Tooltip("Current runtime component.")]
	T _component;
#pragma warning restore 0649
	#endregion // Serialized Fields
	#endregion // Fields

	#region Properties
	public T component
	{
		get { return _component; }
		private set { _component = value; }
	}
	#endregion // Properties

	#region Methods
	public T SetupWithSelf(Transform self)
	{
		component = null;
		component = GetWithSelf(self);
		return component;
	}

	public T GetWithSelf(Transform self)
	{
		return Get(
			kind,
			parent,
			self,
			childName,
			findInChildren,
			findInactive,
			directComponent
		);
	}

	public void SetDirect(T component)
	{
		this.kind = FindCompKind.Direct;
		this.directComponent = component;
		this.component = component;
	}

	public static T Get(
		FindCompKind kind,
		Transform parent,
		Transform self,
		string childName,
		bool findInChildren,
		bool findInactive,
		T directComponent,
		bool instantiateFbxChildren = false
	)
	{
		T component = null;

		switch(kind)
		{
			case FindCompKind.Direct:
			{
				component = directComponent;
				break;
			}

			case FindCompKind.FindUnderSelf:
			{
				if(self != null)
				{
					Transform node = self.FindDeep(childName, findInactive);
					if(node != null && !findInChildren) { component = node.GetComponent<T>(); }
					else if(node != null && findInChildren) { component = node.GetComponentInChildren<T>(findInactive); }
				}
				break;
			}

			case FindCompKind.FindUnderNode:
			{
				if(parent != null)
				{
					Transform node = parent.FindDeep(childName, findInactive);
					if(node != null && !findInChildren) { component = node.GetComponent<T>(); }
					else if(node != null && findInChildren) { component = node.GetComponentInChildren<T>(findInactive); }
				}
				break;
			}
		}

		return component;
	}
	#endregion // Methods
}
#endregion // Base

#region Transform
[Serializable]
public class FindTransField : FindCompField<Transform>
{
}
#endregion // Transform

#region Animator
[Serializable]
public class FindAnimatorField : FindCompField<Animator>
{
}
#endregion // Animator

#region Rigidbody
[Serializable]
public class FindRigidField : FindCompField<Rigidbody>
{
}
#endregion // Rigidbody

#region Joint
[Serializable]
public class FindJointField : FindCompField<Joint>
{
}
#endregion // Joint

#region Renderer
[Serializable]
public class FindRendField : FindCompField<Renderer>
{
}
#endregion // Renderer

#region MeshRenderer
[Serializable]
public class FindMeshRendField : FindCompField<MeshRenderer>
{
}
#endregion // MeshRenderer

#region SkinnedMeshRenderer
[Serializable]
public class FindSkinnedMeshRendField : FindCompField<SkinnedMeshRenderer>
{
}
#endregion // SkinnedMeshRenderer

#region Light
[Serializable]
public class FindLightField : FindCompField<Light>
{
}
#endregion // Light