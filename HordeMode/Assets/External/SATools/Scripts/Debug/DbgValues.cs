using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public sealed partial class DbgValues
{
	#region Types
	enum DbgValueKind : System.Int32
	{
		None,
		Object,
		Int,
		Float,
		Bool,
		Vec2,
		Vec3,
		Quat,
	}

	[StructLayout(LayoutKind.Explicit)]
	struct DbgNodeValue
	{
		[FieldOffset(0)]
		public DbgValueKind kind;
		[FieldOffset(4)]
		public int intValue;
		[FieldOffset(4)]
		public float floatValue;
		[FieldOffset(4)]
		public bool boolValue;
		[FieldOffset(4)]
		public Vector2 vec2Value;
		[FieldOffset(4)]
		public Vector3 vec3Value;
		[FieldOffset(4)]
		public Quaternion quatValue;
	}

	class DbgNode
	{
		public readonly string name;
		public readonly List<DbgNode> children = new List<DbgNode>();

		public DbgNodeValue value;
		public object refValue;

		public DbgNode(string name)
		{
			this.name = name;
		}
	}

	class DbgObjectData
	{
		public readonly List<DbgNode> roots = new List<DbgNode>();
		public readonly Dictionary<string, DbgNode> lookup = new Dictionary<string, DbgNode>();
	}
	#endregion // Types

	#region Fields
	Dictionary<object, DbgObjectData> nodes = new Dictionary<object, DbgObjectData>();
	#endregion // Fields

	#region Properties
	public static DbgValues instance
	{
		get;
		private set;
	}
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	#region System
	public static DbgValues Setup()
	{
		instance = new DbgValues();

		return instance;
	}

	public void Shutdown()
	{
		nodes.Clear();
		instance = null;
	}
	#endregion // System

	#region Interface
	public static void Set(object owner, string path, int value)
	{
		instance.SetNodeValue(owner, path, new DbgNodeValue { kind = DbgValueKind.Int, intValue = value });
	}

	public static void Set(object owner, string path, float value)
	{
		instance.SetNodeValue(owner, path, new DbgNodeValue { kind = DbgValueKind.Float, floatValue = value });
	}

	public static void Set(object owner, string path, bool value)
	{
		instance.SetNodeValue(owner, path, new DbgNodeValue { kind = DbgValueKind.Bool, boolValue = value });
	}

	public static void Set(object owner, string path, Vector2 value)
	{
		instance.SetNodeValue(owner, path, new DbgNodeValue { kind = DbgValueKind.Vec2, vec2Value = value });
	}

	public static void Set(object owner, string path, Vector3 value)
	{
		instance.SetNodeValue(owner, path, new DbgNodeValue { kind = DbgValueKind.Vec3, vec3Value = value });
	}

	public static void Set(object owner, string path, Quaternion value)
	{
		instance.SetNodeValue(owner, path, new DbgNodeValue { kind = DbgValueKind.Quat, quatValue = value });
	}

	public static void Set(object owner, string path, object value)
	{
		instance.SetNodeValue(owner, path, value);
	}

	public static void Clear(object owner)
	{
		instance.ClearData(owner);
	}

	public static void Clear(object owner, string path)
	{
		instance.ClearNode(owner, path);
	}
	#endregion // Interface

	#region Setting
	void SetNodeValue(object owner, string path, object refValue)
	{
		instance.SetNodeValue(owner, path, new DbgNodeValue { kind = DbgValueKind.Object }, refValue);
	}

	void SetNodeValue(object owner, string path, DbgNodeValue value, object refValue = null)
	{
		DbgObjectData data = GetOrAddData(owner);

		DbgNode node = GetNode(data, path);
		if(node == null)
		{
			node = AddNode(data, path);
			data.lookup[path] = node;
		}

		node.value = value;
		node.refValue = refValue;
	}
	#endregion // Setting

	#region Querying / Adding
	DbgObjectData GetData(object owner)
	{
		return nodes.FindOrNull(owner);
	}

	DbgObjectData GetOrAddData(object owner)
	{
		return nodes.FindOrAddNew(owner);
	}

	DbgNode GetNode(object owner, string path)
	{
		DbgObjectData data = GetData(owner);
		if(data == null) { return null; }

		return GetNode(data, path);
	}

	DbgNode GetNode(DbgObjectData data, string path)
	{
		return data.lookup.FindOrNull(path);
	}

	DbgNode AddNode(DbgObjectData data, string path)
	{
		string[] comps = path.Split('.');

		int compIndex = 0;
		List<DbgNode> nodes = data.roots;
		DbgNode node = null;
		while(compIndex < comps.Length)
		{
			string comp = comps[compIndex];
			node = GetOrAdd(comp, nodes);
			nodes = node.children;
			++compIndex;
		}

		return node;
	}

	DbgNode GetOrAdd(string pathComp, List<DbgNode> nodes)
	{
		DbgNode node;

		for(int i = 0; i < nodes.Count; i++)
		{
			node = nodes[i];
			if(node.name == pathComp)
			{
				return node;
			}
		}

		node = new DbgNode(pathComp);
        nodes.Add(node);
		return node;
	}
	#endregion // Querying / Adding

	#region Clearing
	void ClearData(object owner)
	{
		nodes.Remove(owner);
	}

	void ClearNode(object owner, string path)
	{
		DbgObjectData data = GetData(owner);
		if(data == null) { return; }

		DbgNode node = GetNode(data, path);
		if(node == null) { return; }

		node.value = default(DbgNodeValue);
		node.refValue = null;
		data.lookup.Remove(path);
	}
	#endregion // Clearing

	#region Drawing
	public void SystemOnGUI()
	{
		foreach(var kvp in nodes)
		{
			object owner = kvp.Key;
			DbgObjectData data = kvp.Value;

			var ueOwner = owner as UE.Object;
			if(ueOwner != null)
			{
				GUILayout.Label(GarbageCache.GetName(ueOwner));
			}
			else
			{
				GUILayout.Label(owner.ToString());
			}

			for(int i = 0; i < data.roots.Count; i++)
			{
				DbgNode node = data.roots[i];
				DrawNode(node, indent: 1);
			}
		}
	}

	void DrawNode(DbgNode node, int indent)
	{
		GUILayout.BeginHorizontal();

		GUILayout.Space(30.0f * (float)indent);

		GUILayout.Label(node.name, GUILayout.Width(200.0f));

		switch(node.value.kind)
		{
		case DbgValueKind.Object:
			GUILayout.Label(node.refValue == null ? "<null>" : node.refValue.ToString());
			break;
		case DbgValueKind.Int:
			GUILayout.Label(node.value.intValue.ToString());
			break;
		case DbgValueKind.Float:
			GUILayout.Label(node.value.floatValue.ToString("0.000"));
			break;
        case DbgValueKind.Bool:
			GUILayout.Toggle(node.value.boolValue, "");
			break;
		case DbgValueKind.Vec2:
			GUILayout.Label(node.value.vec2Value.ToString());
			break;
		case DbgValueKind.Vec3:
			GUILayout.Label(node.value.vec3Value.ToString());
			break;
		case DbgValueKind.Quat:
			Vector3 euler = node.value.quatValue.eulerAngles;
			GUILayout.Label(euler.ToString());
			break;
		}
		GUILayout.EndHorizontal();

		for(int i = 0; i < node.children.Count; i++)
		{
			DbgNode child = node.children[i];
			DrawNode(child, indent + 1);
		}
	}
	#endregion // Drawing
	#endregion // Methods
}
