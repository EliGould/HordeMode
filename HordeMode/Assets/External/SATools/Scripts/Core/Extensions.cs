using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

public static class Extensions
{
	#region Color
	public static Color WithRGB(this Color c, float? r = null, float? g = null, float? b = null)
	{
		c.r = r ?? c.r;
		c.g = g ?? c.g;
		c.b = b ?? c.b;
		return c;
	}

	public static Color WithAlpha(this Color c, float alpha)
	{
		c.a = alpha;
		return c;
	}
	#endregion // Color

	#region Vector
	public static Vector2 XY(this Vector3 v)
	{
		return new Vector2(v.x, v.y);
	}

	public static Vector2 XZ(this Vector3 v)
	{
		return new Vector2(v.x, v.z);
	}

	public static Vector3 ToXY(this Vector2 v)
	{
		return new Vector3(v.x, v.y, 0.0f);
	}

	public static Vector3 ToXZ(this Vector2 v)
	{
		return new Vector3(v.x, 0.0f, v.y);
	}
	#endregion // Vector

	#region String
	public static string Fmt(this string fmt, object a)
	{
		return string.Format(fmt, a);
	}

	public static string Fmt(this string fmt, object a, object b)
	{
		return string.Format(fmt, a, b);
	}

	public static string Fmt(this string fmt, params object[] args)
	{
		return string.Format(fmt, args);
	}
	#endregion // String

	#region Array
	public static T[] Fill<T>(this T[] array, T value)
	{
		for(int i = 0; i < array.Length; ++i)
		{
			array[i] = value;
		}

		return array;
	}

	public static T[] Fill<T>(this T[] array, Func<T> creator)
	{
		for(int i = 0; i < array.Length; ++i)
		{
			array[i] = creator();
		}

		return array;
	}

	public static T[] Fill<T>(this T[] array, Func<int, T> creator)
	{
		for(int i = 0; i < array.Length; ++i)
		{
			array[i] = creator(i);
		}

		return array;
	}

	public static T[] FillDefault<T>(this T[] array)
	{
		for(int i = 0; i < array.Length; ++i)
		{
			array[i] = default(T);
		}

		return array;
	}

	public static T[] FillNew<T>(this T[] array)
		where T : new()
	{
		for(int i = 0; i < array.Length; ++i)
		{
			array[i] = new T();
		}

		return array;
	}

	public static int FindIndex<T>(this T[] array, T obj) where T : class
	{
		for(int i = 0; i < array.Length; ++i)
		{
			if(array[i] == obj)
			{
				return i;
			}
		}
		return -1;
	}

	public static int FindIndex(this string[] array, string obj)
	{
		for(int i = 0; i < array.Length; ++i)
		{
			if(array[i] == obj)
			{
				return i;
			}
		}
		return -1;
	}

	public static int FindIndex<T>(this T[] array, System.Predicate<T> match)
	{
		for(int i = 0; i < array.Length; ++i)
		{
			if(match(array[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public static bool Contains<T>(this T[] array, T obj) where T : class
	{
		return FindIndex(array, obj) > -1;
	}

	public static bool Contains(this string[] array, string obj)
	{
		return FindIndex(array, obj) > -1;
	}

	public static T Find<T>(this T[] array, System.Predicate<T> match)
	{
		return System.Array.Find(array, match);
	}

	public static void Sort<T>(this T[] array, System.Comparison<T> comparison)
	{
		System.Array.Sort(array, comparison);
	}

	public static void Clear<T>(this T[] array)
	{
		System.Array.Clear(array, 0, array.Length);
	}

	public static T TryGet<T>(this T[] array, int index) where T : class
	{
		if(0 <= index && index < array.Length)
			return array[index];
		else
			return null;
	}

	public static T TryGet<T>(this List<T> list, int index) where T : class
	{
		if(0 <= index && index < list.Count)
			return list[index];
		else
			return null;
	}

	public static T GetOr<T>(this T[] array, int index, T defaultValue = default(T))
	{
		if(0 <= index && index < array.Length)
		{
			return array[index];
		}
		else
		{
			return defaultValue;
		}
	}

	public static T GetOr<T>(this List<T> list, int index, T defaultValue = default(T))
	{
		if(0 <= index && index < list.Count)
		{
			return list[index];
		}
		else
		{
			return defaultValue;
		}
	}
	#endregion // Array

	#region List
	// Stack-like (LIFO)
	public static void Push<T>(this List<T> list, T item)
	{
		list.Add(item);
	}

	// Stack-like (LIFO)
	public static T Pop<T>(this List<T> list)
	{
		var item = list[list.Count - 1];
		list.RemoveAt(list.Count - 1);
		return item;
	}

	// Queue-like (FIFO)
	public static void Enqueue<T>(this List<T> list, T item)
	{
		list.Add(item);
	}

	// Queue-like (FIFO)
	public static T Dequeue<T>(this List<T> list)
	{
		var item = list[0];
		list.RemoveAt(0);
		return item;
	}

	public static T RandomItem<T>(this IList<T> list)
	{
		if(list.Count == 0)
		{
			return default(T);
		}
		return list[UE.Random.Range(0, list.Count)];
	}

	public static T RandomItem<T>(this T[] array)
	{
		if(array.Length == 0)
		{
			return default(T);
		}
		return array[UE.Random.Range(0, array.Length)];
	}

	public static T TakeRandomItem<T>(this IList<T> list)
	{
		if(list.Count == 0) { return default(T); }

		int index = UE.Random.Range(0, list.Count);
		T val = list[index];
		list.RemoveAt(index);
		return val;
	}
	#endregion // List

	#region Dictionary
	public static Value FindOrDefault<Key, Value>(this Dictionary<Key, Value> dictionary, Key key, Value defaultValue)
	{
		Value value;
		if(dictionary.TryGetValue(key, out value))
		{
			return value;
		}
		else
		{
			return defaultValue;
		}
	}

	public static Value FindOrCall<Key, Value>(this Dictionary<Key, Value> dictionary, Key key, System.Func<Value> defaultValue)
	{
		Value value;
		if(dictionary.TryGetValue(key, out value))
		{
			return value;
		}
		else
		{
			return defaultValue();
		}
	}

	public static Value FindOrAddDefault<Key, Value>(this Dictionary<Key, Value> dictionary, Key key, Value defaultValue)
	{
		Value value;
		if(dictionary.TryGetValue(key, out value))
		{
			return value;
		}
		else
		{
			dictionary.Add(key, defaultValue);
			return defaultValue;
		}
	}

	public static Value FindOrAddNew<Key, Value>(this Dictionary<Key, Value> dictionary, Key key)
		where Value : new()
	{
		Value value;
		if(dictionary.TryGetValue(key, out value))
		{
			return value;
		}
		else
		{
			Value v = new Value();
			dictionary.Add(key, v);
			return v;
		}
	}

	public static Value FindOrAddDefault<Key, Value>(this Dictionary<Key, Value> dictionary, Key key, System.Func<Value> defaultValue)
	{
		Value value;
		if(dictionary.TryGetValue(key, out value))
		{
			return value;
		}
		else
		{
			var def = defaultValue();
			dictionary.Add(key, def);
			return def;
		}
	}

	public static Value? FindOrNullable<Key, Value>(this Dictionary<Key, Value> dictionary, Key key) where Value : struct
	{
		Value value;
		if(dictionary.TryGetValue(key, out value))
		{
			return (Value?)value;
		}
		else
		{
			return (Value?)null;
		}
	}

	public static Value FindOrNull<Key, Value>(this Dictionary<Key, Value> dictionary, Key key) where Value : class
	{
		Value value;
		if(dictionary.TryGetValue(key, out value))
		{
			return value;
		}
		else
		{
			return null;
		}
	}
	#endregion // Dictionary

	#region GameObject
	public static T AddMissingComponent<T>(this GameObject go)
		where T : Component
	{
		var obj = go.GetComponent<T>();
		if(obj == null) { obj = go.AddComponent<T>(); }
		return obj;
	}
	#endregion // GameObject

	#region Transform
	public static Transform FindDeep(this Transform trans, string name, bool includeInactive = false)
	{
		if(includeInactive)
		{
			for(int i = 0; i < trans.childCount; ++i)
			{
				Transform child = trans.GetChild(i);
				if(GarbageCache.GetName(child) == name) { return child; }
				else
				{
					child = FindDeep(child, name);
					if(child != null) { return child; }
				}
			}
		}
		else
		{
			for(int i = 0; i < trans.childCount; ++i)
			{
				Transform child = trans.GetChild(i);

				if(!child.gameObject.activeSelf) { continue; }

				if(GarbageCache.GetName(child) == name) { return child; }
				else
				{
					child = FindDeep(child, name);
					if(child != null) { return child; }
				}
			}
		}

		return null;
	}
	#endregion // Transform

	#region Editor
	#region SerializedProperty
	public static T EnumValue<T>(this SerializedProperty property) where T : struct, System.IConvertible
	{
		return EnumHelper<T>.values[property.enumValueIndex];
	}

	public static void SetEnumValue<T>(this SerializedProperty property, T value) where T : struct, System.IConvertible
	{
		property.enumValueIndex = EnumHelper<T>.IndexOf(value);
	}
	#endregion // SerializedProperty
	#endregion // Editor
}
