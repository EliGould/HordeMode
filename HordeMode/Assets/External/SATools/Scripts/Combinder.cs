using UnityEngine;
using System.Collections.Generic;

public class Combinder<T>
{
	#region Types
	public delegate T Combind(T prev, T next);
	public delegate T DefaultVal();
	public delegate void OnChanged(T val);

	struct Pair
	{
		public readonly object setter;
		public readonly T val;
		public Pair(object setter, T val)
		{
			this.setter = setter;
			this.val = val;
		}
	}

	DefaultKind defaultKind;
	Combind combinder;
	DefaultVal defaultValAction;
	T defaultValue;
	#endregion // Types

	enum DefaultKind
	{
		action,
		val
	}

	#region Fields
	public event OnChanged onChanged;

	List<Pair> vals = new List<Pair>();
	EqualityComparer<T> comparer = null;
	T current;
	bool recalculate;
	#endregion // Fields

	#region Properties
	public T Current
	{
		get
		{
			if(recalculate)
			{
				CombindCurrent();
			}
			return current;
		}
	}
	#endregion // Properties

	#region Methods
	public Combinder(Combind combinder, DefaultVal defaultVal)
	{
		this.combinder = combinder;
		this.defaultValAction = defaultVal;
		this.defaultKind = DefaultKind.action;

		current = defaultVal();
		
		recalculate = false;
	}

	public Combinder(Combind combinder, T defaultVal)
	{
		this.combinder = combinder;
		this.defaultValue = defaultVal;
		this.defaultKind = DefaultKind.val;

		current = defaultVal;

		recalculate = false;
	}

	public void SetComparer(EqualityComparer<T> comparer)
	{
		this.comparer = comparer;
	}

#if UNITY_EDITOR
	public bool TryAdd<U>(T val, U setter) where U : class
#else
	public bool TryAdd(T val, object setter)
#endif
	{
		if(Find(setter))
		{
			return false;
		}
		Set(val, setter);
		return true;
	}

	bool Find(object setter)
	{
		int dummy;
		return Find(setter, out dummy);
	}

	bool Find(object setter, out int index)
	{
		for(index = 0; index < vals.Count; ++index)
		{
			var pair = vals[index];
			if(pair.setter == setter)
			{
				return true;
			}
		}
		return false;
	}
	
#if UNITY_EDITOR
	public void Set<U>(T val, U setter) where U : class
#else
	public void Set(T val, object setter)
#endif
	{
		for(int i = 0; i < vals.Count; ++i)
		{
			Pair pair = vals[i];
			if(pair.setter == setter)
			{
				// No change
				if(ValuesEqual(pair.val, val))
				{
					return;
				}
			}
		}

		Remove(setter);
		vals.Add(new Pair(setter, val));

		recalculate = true;
		if(onChanged != null)
		{
			// If we have the delegate assigned we want to auto update as we can't rely upon lazy evaluation happening.
			CombindCurrent();
		}
	}
#if UNITY_EDITOR
	public bool Remove<U>(U setter) where U : class
#else
	public bool Remove(object setter)
#endif
	{
		bool removed = RemoveAllWithSetter(setter) > 0;
		recalculate |= removed;
		if(recalculate && onChanged != null)
		{
			// If we have the delegate assigned we want to auto update as we can't rely upon lazy evaluation happening.
			CombindCurrent();
		}
		return removed;
	}

#if UNITY_EDITOR
	int RemoveAllWithSetter<U>(U setter) where U : class
#else
	int RemoveAllWithSetter(object setter)
#endif
	{
		int removed = 0;
		for(int i = vals.Count - 1; i >= 0; --i)
		{
			if(vals[i].setter == setter)
			{
				++removed;
				vals.RemoveAt(i);
			}
		}
		return removed;
	}

	public void Clear()
	{
		vals.Clear();

		T defaultValue = GetDefaultValue();

		TriggerOnChanged(prev: current, next: defaultValue);

		current = defaultValue;
		recalculate = false;
	}

	public IEnumerator<KeyValuePair<object, T>> SettersAndVals()
	{
		for(int i = 0; i < vals.Count; ++i)
		{
			Pair pair = vals[i];
			yield return new KeyValuePair<object, T>(pair.setter, pair.val);
		}
	}

	public IEnumerator<object> Setters()
	{
		for(int i = 0; i < vals.Count; ++i)
		{
			yield return vals[i].setter;
		}
	}

	public IEnumerator<T> Vals()
	{
		for(int i = 0; i < vals.Count; ++i)
		{
			yield return vals[i].val;
		}
	}

	void TriggerOnChanged(T prev, T next)
	{
		if(onChanged != null)
		{
			bool equals = ValuesEqual(prev, next);

			if(equals == false)
			{
				onChanged(next);
			}
		}
	}

	bool ValuesEqual(T a, T b)
	{
		bool equals;

		if(comparer != null)
		{
			equals = comparer.Equals(a, b);
		}
		else
		{
			equals = EqualityComparer<T>.Default.Equals(a, b);
		}

		return equals;
	}

	T GetDefaultValue()
	{
		switch(defaultKind)
		{
		case DefaultKind.action:
			return defaultValAction();
		case DefaultKind.val:
			return defaultValue;
		default:
			return default(T);
		}
	}
	
	void CombindCurrent()
	{
		T current = GetDefaultValue();

		for(int i = 0; i < vals.Count; ++i)
		{
			var p = vals[i];
			current = combinder(current, p.val);
		}

		T old = this.current;
		this.current = current;
		TriggerOnChanged(prev: old, next: current);
	}
	#endregion // Combinder
}

#region Specialised

public class ColorCombinder : Combinder<Color>
{
	public ColorCombinder(Color defaultColor) : base(Combine.Multiply, defaultColor) {}
}

public class AndCombinder : Combinder<bool>
{
	public AndCombinder() : base(Combine.And, true) {}
}

public class OrCombinder : Combinder<bool>
{
	public OrCombinder() : base(Combine.Or, false) {}
}

public class BitAndCombinder : Combinder<int>
{
	public BitAndCombinder(int defaultValue) : base(Combine.BitAnd, defaultValue) { }
}

public class BitOrCombinder : Combinder<int>
{
	public BitOrCombinder() : base(Combine.BitOr, 0) { }
}

#endregion

#region Combine Methods

public class Combine
{
	//Helper functions for combining different data types.

	public static Vector2 Add(Vector2 prev, Vector2 next)
	{
		Vector2 combined;
		combined.x = prev.x * next.x;
		combined.y = prev.y * next.y;
		return combined;
	}

	public static Vector2 Multiply(Vector2 prev, Vector2 next)
	{
		Vector2 combined;
		combined.x = prev.x * next.x;
		combined.y = prev.y * next.y;
		return combined;
	}

	public static Vector3 Multiply(Vector3 prev, Vector3 next)
	{
		Vector3 combined;
		combined.x = prev.x * next.x;
		combined.y = prev.y * next.y;
		combined.z = prev.z * next.z;
		return combined;
	}

	public static Color Multiply(Color prev, Color next)
	{
		Color combined;
		combined.r = prev.r * next.r;
		combined.g = prev.g * next.g;
		combined.b = prev.b * next.b;
		combined.a = prev.a * next.a;
		return combined;
	}

	public static float Multiply(float prev, float next)
	{
		return prev * next;
	}

	public static float Add(float prev, float next)
	{
		return (prev + next);
	}

	public static int Add(int prev, int next)
	{
		return (prev + next);
	}

	public static bool And(bool prev, bool next)
	{
		return prev && next;
	}

	public static bool Or(bool prev, bool next)
	{
		return prev || next;
	}

	public static int BitAnd(int prev, int next)
	{
		return prev & next;
	}

	public static int BitOr(int prev, int next)
	{
		return prev | next;
	}

	public static T Min<T>(T prev, T next) where T: System.IComparable<T>
	{
		return prev.CompareTo(next) < 0 ? prev : next;
	}
	public static T Max<T>(T prev, T next) where T: System.IComparable<T>
	{
		return prev.CompareTo(next) > 0 ? prev : next;
	}
}

#endregion // Combine Methods