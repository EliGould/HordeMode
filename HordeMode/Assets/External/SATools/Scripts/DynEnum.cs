using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

// TO IMPLEMENT NEW DYNENUMS:
// 1. Implement a DynEnum class (f.ex. NpcDynEnum : DynEnum)
//    This can be totally empty, it is only used as a qualifier.
//
// 2. Implement a DynEnums class (f.ex. NpcDynEnums : DynEnums<NpcDynEnum>)
//    Same here, can be totally empty, but don't forget the [Serializeable]
//    attribute.
//
// 3. Implement a DynEnumSet class (f.ex. NpcDynEnumSet : DynEnumSet<NpcDynEnum>)
//    This should have a single constructor that takes a list of dynEnums that passes
//    it along to the base class constructor.
//    
//    public NpcDynEnumSet(List<NpcDynEnum> dynEnums) : base(dynEnums) {}
//
// 4. Implement a DynEnumsLoader class (f.ex. NpcDynEnumsLoader : DynEnumsLoader<NpcDynEnum>)
//    This class is needed to load the editor instance of DynEnums from assets.
//    This is an unfortunate necessity since Unity needs a proper type to
//    load from assets. The implementation should look something like:
//
//    public override DynEnums<NpcDynEnum> Load()
//    {
//        return AssetDatabase.LoadAssetAtPath<NpcDynEnums>("Assets/Constants/NpcDynEnums.asset");
//    }
//
// 5. Implement a DynEnumField class (f.ex. NpcDynEnumField : DynEnumField<NpcDynEnum>)
//    This is needed to display a nice inspector where the user can select
//    multiple dynEnums. It requires (runtime) access to global dynEnums from somewhere.
//    Implementation should look something like:
//
//    public override NpcDynEnum GetDynEnumByName(string name)
//    {
//        return GameConstants.npcDynEnums.GetDynEnum(name);
//    }
//
//    It also needs to handle conversion between the abstract generic DynEnumSet type to
//    the "real", implementing class (partly for legacy reasons). It should simply be
//    something like:
//    
//    public override DynEnumSet<NpcDynEnum> CreateDynEnumSet(List<NpcDynEnum> dynEnums)
//    {
//        return new NpcDynEnumSet(dynEnums);
//    }
//
//    public new NpcDynEnumSet GetSet()
//    {
//        return (NpcDynEnumSet)base.GetSet();
//    }
//
//    Note the "new".
//
//    Finally: don't forget the [Serializeable] attribute.
//
// 6. Implement a DynEnumField drawer (f.ex. NpcDynEnumFieldDrawer : DynEnumFieldDrawer<NpcDynEnum>)
//    This is required to draw the DynEnumField nicely. This class should have a
//    CustomPropertyDrawer attribute which points to the DynEnumField type:
//
//    [CustomPropertyDrawer(typeof(NpcDynEnumField))]
//
//    The implementation should look something like:
//
//    NpcDynEnumsLoader _loader = new NpcDynEnumsLoader();
//    protected override DynEnumsLoader<NpcDynEnum> loader
//    {
//        get { return _loader; }
//    }

[Serializable]
public abstract class DynEnum
{
	[SerializeField]
	public string name;
	[SerializeField]
	public bool isNull { get { return string.IsNullOrEmpty(name) || name == "null"; } }

	public override string ToString()
	{
		return isNull ? "null" : name;
	}
}

public abstract class DynEnumSet<T> where T : DynEnum
{
	// It might be wise to make sure that these sets are sorted.
	public readonly List<T> dynEnumList;

	public DynEnumSet(List<T> dynEnums)
	{
		this.dynEnumList = dynEnums;
	}

	public bool IntersectsWith(DynEnumSet<T> other)
	{
		var a = this.dynEnumList;
		var b = other.dynEnumList;
		for(int aI = 0; aI < a.Count; ++aI)
		{
			for(int bI = 0; bI < b.Count; ++bI)
			{
				if(a[aI] == b[bI])
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool Contains(T dynEnum)
	{
		for(int i = 0; i < dynEnumList.Count; ++i)
		{
			if(dynEnum == dynEnumList[i])
			{
				return true;
			}
		}
		return false;
	}

	// Union
	public void Add(DynEnumSet<T> other)
	{
		Add(other.dynEnumList);
	}

	public void Add(List<T> dynEnums)
	{
		for(int i = 0; i < dynEnums.Count; ++i)
		{
			T dynEnum = dynEnums[i];
			if(!dynEnumList.Contains(dynEnum))
			{
				dynEnumList.Add(dynEnum);
			}
		}
	}

	// Relative Complement
	public void Remove(DynEnumSet<T> other)
	{
		Remove(other.dynEnumList);
	}

	public void Remove(List<T> dynEnums)
	{
		for(int i = dynEnumList.Count - 1; i >= 0; --i)
		{
			T dynEnum = dynEnumList[i];
			if(dynEnums.Contains(dynEnum))
			{
				dynEnumList.RemoveAt(i);
			}
		}
	}

	public void Set(DynEnumSet<T> dynEnums)
	{
		Set(dynEnums.dynEnumList);
	}

	public void Set(List<T> dynEnums)
	{
		dynEnumList.Clear();
		dynEnumList.AddRange(dynEnums);
	}

	public void Clear()
	{
		dynEnumList.Clear();
	}
}

public abstract class DynEnumsLoader<T> where T : DynEnum
{
	public abstract DynEnums<T> Load();
}

public abstract class DynEnums<T> : ScriptableObject where T : DynEnum
{
	[SerializeField]
	protected List<T> dynEnums;

	[NonSerialized]
	Dictionary<string, T> dynEnumsByName = null;

	public T GetValue(string name)
	{
		if(dynEnumsByName == null)
		{
			dynEnumsByName = new Dictionary<string, T>(dynEnums.Count);
			for(int i = 0; i < dynEnums.Count; ++i)
			{
				var dynEnum = dynEnums[i];
				dynEnumsByName[dynEnum.name] = dynEnum;
			}
		}

		return dynEnumsByName.FindOrNull(name);
	}

#if UNITY_EDITOR
	static DynEnums<T> editorInstance;

	public static DynEnums<T> GetEditorInstance(DynEnumsLoader<T> loader)
	{
		if(editorInstance == null)
		{
			editorInstance = loader.Load();
			//editorInstance = AssetDatabase.LoadAssetAtPath<InteractorDynEnums>("Assets/Constants/InteractorDynEnums.asset");
		}
		return editorInstance;
	}

	[NonSerialized]
	string[] cachedEditorDynEnumNames = null;

	public string[] EditorGetDynEnumNames()
	{
		var names = new HashSet<string>();
		for(int i = 0; i < dynEnums.Count; ++i)
		{
			var dynEnum = dynEnums[i];
			if(!string.IsNullOrEmpty(dynEnum.name))
			{
				names.Add(dynEnum.name);
			}
		}

		cachedEditorDynEnumNames = new string[names.Count];
		names.CopyTo(cachedEditorDynEnumNames);

		return cachedEditorDynEnumNames;
	}

#endif
}

#region DynEnumField
[Serializable]
public abstract class DynEnumField<T> where T : DynEnum
{
	[SerializeField]
	string[] dynEnumNames;

	DynEnumSet<T> _cachedSet = null;

	public T first
	{
		get
		{
			List<T> dynEnums = GetSet().dynEnumList;
			return dynEnums.Count == 0 ? null : dynEnums[0];
		}
	}

	protected DynEnumSet<T> GetSet()
	{
#if IS_RELEASE || !UNITY_EDITOR
		if(_cachedSet != null)
		{
			return _cachedSet;
		}
#endif // IS_RELEASE || !UNITY_EDITOR

		using(var dynEnums = TempList<T>.Get())
		{
			for(int i = 0; i < dynEnumNames.Length; ++i)
			{
				T dynEnum = GetDynEnumByName(dynEnumNames[i]);
				dynEnums.Add(dynEnum);
			}

#if !IS_RELEASE && UNITY_EDITOR
			if(_cachedSet != null && dynEnums.Count == _cachedSet.dynEnumList.Count)
			{
				bool dirty = false;
				for(int i = 0; i < _cachedSet.dynEnumList.Count; ++i)
				{
					if(dynEnums[i] != _cachedSet.dynEnumList[i])
					{
						dirty = true;
						break;
					}
				}

				if(!dirty) { return _cachedSet; }
			}
#endif // !IS_RELEASE && UNITY_EDITOR

			_cachedSet = CreateDynEnumSet(new List<T>(dynEnums.buffer));
		}

		return _cachedSet;
	}

	public string[] GetNameList()
	{
		return dynEnumNames;
	}

	public void Add(List<T> dynEnums)
	{
		using(var newNames = TempList<string>.Get())
		{
			for(int i = 0; i < dynEnumNames.Length; ++i)
			{
				newNames.Add(dynEnumNames[i]);
			}

			for(int i = 0; i < dynEnums.Count; ++i)
			{
				string dynEnumName = dynEnums[i].name;
				int index = Array.IndexOf(dynEnumNames, dynEnumName);

				if(index == -1)
				{
					newNames.Add(dynEnumName);
				}
			}

			if(newNames.Count != dynEnumNames.Length)
			{
				dynEnumNames = newNames.buffer.ToArray();
			}
		}
	}

	public void Remove(List<T> dynEnums)
	{
		using(var newNames = TempList<string>.Get())
		{
			newNames.buffer.AddRange(dynEnumNames);

			for(int i = 0; i < dynEnums.Count; ++i)
			{
				T dynEnum = dynEnums[i];

				for(int ti = newNames.Count - 1; ti >= 0; --ti)
				{
					if(dynEnum.name == newNames[ti])
					{
						newNames.buffer.RemoveAt(ti);
						break;
					}
				}
			}

			if(newNames.Count != dynEnumNames.Length)
			{
				dynEnumNames = newNames.buffer.ToArray();
			}
		}
	}

	public abstract DynEnumSet<T> CreateDynEnumSet(List<T> dynEnums);
	public abstract T GetDynEnumByName(string name);
}
#endregion

#region DynEnumField Drawer
#if UNITY_EDITOR
//[CustomPropertyDrawer(typeof(DynEnumField<T>))]
public abstract class DynEnumFieldDrawer<T> : PropertyDrawer where T : DynEnum
{
	protected abstract DynEnumsLoader<T> loader { get; }

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		var dynEnums = DynEnums<T>.GetEditorInstance(loader);
		if(dynEnums == null)
		{
			return EditorGUIUtility.singleLineHeight;
		}
		else
		{
			var dynEnumNames = property.FindPropertyRelative("dynEnumNames");
			float height = EditorGUIUtility.singleLineHeight;
			if(dynEnumNames.isExpanded)
			{
				height += EditorGUIUtility.singleLineHeight * dynEnumNames.arraySize;
			}
			return height;
		}
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		string tooltip = EditorHelper.GetTooltipFromAttribute(fieldInfo, defaultValue: label.tooltip);
		label.tooltip = tooltip;

		var dynEnums = DynEnums<T>.GetEditorInstance(loader);
		if(dynEnums == null)
		{
			EditorGUI.LabelField(position, label, "Couldn't find dynEnums in Data/, make sure that there is a serialized object there :(");
		}
		else
		{
			var dynEnumNames = property.FindPropertyRelative("dynEnumNames");

			var names = dynEnums.EditorGetDynEnumNames();

			// If Need be this can be speed up a bit by sorting dynEnumNames by the index in names
			// Then unroll FindIndex and make sure that it starts for the previous index in by the previous loop
			// This should make the lookup more linear but it needs to make sure the sort only happens when dynEnumNames has changed.

			float buttonLength = 25f;
			Rect positionButton = position;
			Rect positionContent = position;
			positionContent.width -= buttonLength;
			positionContent.height = EditorGUIUtility.singleLineHeight;
			positionButton.x = position.xMax - buttonLength;
			positionButton.width = buttonLength;
			positionButton.height = EditorGUIUtility.singleLineHeight;

			dynEnumNames.isExpanded = EditorGUI.Foldout(positionContent, dynEnumNames.isExpanded, label);
			if(dynEnumNames.isExpanded)
			{
				if(GUI.Button(positionButton, "+"))
				{
					dynEnumNames.InsertArrayElementAtIndex(dynEnumNames.arraySize);
					var t = dynEnumNames.GetArrayElementAtIndex(dynEnumNames.arraySize - 1);
					t.stringValue = "null";
				}
				const float INDENT_WIDTH = 15.0f;
				positionContent.x += INDENT_WIDTH;
				positionContent.width -= INDENT_WIDTH;

				positionButton.y += EditorGUIUtility.singleLineHeight;
				positionContent.y += EditorGUIUtility.singleLineHeight;

				for(int i = 0; i < dynEnumNames.arraySize; ++i)
				{
					var dynEnumName = dynEnumNames.GetArrayElementAtIndex(i);
					var index = names.FindIndex(dynEnumName.stringValue);

					if(GUI.Button(positionButton, "-"))
					{
						dynEnumNames.DeleteArrayElementAtIndex(i);
						i -= 1;
					}
					var newIndex = EditorGUI.Popup(positionContent, index, names);
					if(newIndex != index)
					{
						dynEnumName.stringValue = names.TryGet(newIndex) ?? "null";
					}

					positionButton.y += EditorGUIUtility.singleLineHeight;
					positionContent.y += EditorGUIUtility.singleLineHeight;
				}
			}
		}
	}
}
#endif
#endregion

#region SingleDynEnumField
[Serializable]
public abstract class SingleDynEnumField<T> where T : DynEnum
{
	[SerializeField]
	string dynEnumName;

	public string name
	{
		get { return dynEnumName; }
	}
}
#endregion // SingleDynEnumField

#region SingleDynEnumFieldDrawer
#if UNITY_EDITOR
public abstract class SingleDynEnumFieldDrawer<T> : PropertyDrawer where T : DynEnum
{
	protected abstract DynEnumsLoader<T> loader { get; }

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUIUtility.singleLineHeight;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		string tooltip = EditorHelper.GetTooltipFromAttribute(fieldInfo, defaultValue: label.tooltip);
		label.tooltip = tooltip;

		var dynEnums = DynEnums<T>.GetEditorInstance(loader);
		if(dynEnums == null)
		{
			EditorGUI.LabelField(position, label, "Couldn't find dynEnums in Constants/, make sure that there is a serialized object there :(");
		}
		else
		{
			var dynEnumName = property.FindPropertyRelative("dynEnumName");

			var names = dynEnums.EditorGetDynEnumNames();
			
			var index = names.FindIndex(dynEnumName.stringValue);
			var newIndex = EditorGUI.Popup(position, index, names);
			if(newIndex != index)
			{
				dynEnumName.stringValue = names.TryGet(newIndex) ?? "null";
			}
		}
	}
}
#endif
#endregion // SingleDynEnumFieldDrawer