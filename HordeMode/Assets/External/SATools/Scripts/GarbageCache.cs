using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// To avoid random Unity garbage
public static class GarbageCache
{
	#region Fields
	static readonly Dictionary<int, string> nameLookup = new Dictionary<int, string>();
	static readonly Dictionary<int, string> tagLookup = new Dictionary<int, string>();
	#endregion // Fields

	#region Methods
	public static string GetName(UE.Object obj)
	{
		int instanceId = obj.GetInstanceID();

		string name;
		if(nameLookup.TryGetValue(instanceId, out name))
		{
			return name;
		}

		name = obj.name;
		nameLookup[instanceId] = name;
		return name;
	}

	public static string GetTag(GameObject obj)
	{
		int instanceId = obj.GetInstanceID();

		string tag;
		if(tagLookup.TryGetValue(instanceId, out tag))
		{
			return tag;
		}

		tag = obj.tag;
		tagLookup[instanceId] = tag;
		return tag;
	}

	public static void ClearFor(UE.Object obj)
	{
		int instanceId = obj.GetInstanceID();

		nameLookup.Remove(instanceId);
		if(obj is GameObject) { tagLookup.Remove(instanceId); }
	}

	public static void Clear()
	{
		nameLookup.Clear();
		tagLookup.Clear();
	}
	#endregion // Methods
}
