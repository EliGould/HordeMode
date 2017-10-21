using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

// Buffers for temporary use within scopes.
// Should be used with care and should always
// be returned.
// Has a fixed depth of how many temporary
// buffers of the same kind can be in use
// simultaneously.
//
// Example usage:
//     using(var list = TempList<Transform>.Get())
//     {
//         var list = TempList<Transform>.Get();
//         GetComponents(list.buffer);
//     }
// Or (less safe, may forget to return):
//     var list = TempList<Transform>.Get();
//     GetComponents(list.buffer);
//     ...
//     list.Return();
//
#region Interface Classes
public static class TempList<T>
{
	#region Constants
	const int MAX_DEPTH = 3;
	#endregion // Constants

	#region Fields
	static readonly TempListImpl<T>[] entries = new TempListImpl<T>[MAX_DEPTH].Fill(
		() => new TempListImpl<T>()
	);
	#endregion // Fields

	#region Methods
	public static TempListImpl<T> Get()
	{
		return (TempListImpl<T>)TempObjectHelper.TakeFirstUnused(entries);
	}

	public static void Return(TempListImpl<T> list)
	{
		TempObjectHelper.ReturnUsed(entries, list);
	}
	#endregion // Methods
}

public static class SmallTempArray<T>
{
	#region Constants
	const int MAX_DEPTH = 3;
	const int LENGTH = 8;
	#endregion // Constants

	#region Fields
	static readonly TempArrayImpl<T>[] entries = new TempArrayImpl<T>[MAX_DEPTH].Fill(
		() => new TempArrayImpl<T>(LENGTH)
	);
	#endregion // Fields

	#region Methods
	public static TempArrayImpl<T> Get()
	{
		return (TempArrayImpl<T>)TempObjectHelper.TakeFirstUnused(entries);
	}

	public static void Return(TempArrayImpl<T> array)
	{
		TempObjectHelper.ReturnUsed(entries, array);
	}
	#endregion // Methods
}

public static class TempArray<T>
{
	#region Constants
	const int MAX_DEPTH = 3;
	const int LENGTH = 32;
	#endregion // Constants

	#region Fields
	static readonly TempArrayImpl<T>[] entries = new TempArrayImpl<T>[MAX_DEPTH].Fill(
		() => new TempArrayImpl<T>(LENGTH)
	);
	#endregion // Fields

	#region Methods
	public static TempArrayImpl<T> Get()
	{
		return (TempArrayImpl<T>)TempObjectHelper.TakeFirstUnused(entries);
	}

	public static void Return(TempArrayImpl<T> array)
	{
		TempObjectHelper.ReturnUsed(entries, array);
	}
	#endregion // Methods
}

public static class LargeTempArray<T>
{
	#region Constants
	const int MAX_DEPTH = 3;
	const int SIZE = 64;
	#endregion // Constants

	#region Fields
	static readonly TempArrayImpl<T>[] entries = new TempArrayImpl<T>[MAX_DEPTH].Fill(
		() => new TempArrayImpl<T>(SIZE)
	);
	#endregion // Fields

	#region Methods
	public static TempArrayImpl<T> Get()
	{
		return (TempArrayImpl<T>)TempObjectHelper.TakeFirstUnused(entries);
	}

	public static void Return(TempArrayImpl<T> array)
	{
		TempObjectHelper.ReturnUsed(entries, array);
	}
	#endregion // Methods
}

public static class TempStringBuilder
{
	#region Constants
	const int MAX_DEPTH = 6;
	#endregion // Constants

	#region Fields
	static readonly TempStringBuilderImpl[] entries = new TempStringBuilderImpl[MAX_DEPTH].Fill(
		() => new TempStringBuilderImpl()
	);
	#endregion // Fields

	#region Methods
	public static TempStringBuilderImpl Get()
	{
		return (TempStringBuilderImpl)TempObjectHelper.TakeFirstUnused(entries);
	}

	public static void Return(TempStringBuilderImpl builder)
	{
		TempObjectHelper.ReturnUsed(entries, builder);
	}
	#endregion // Methods
}

public static class TempHashSet<T>
{
	#region Constants
	const int MAX_DEPTH = 3;
	#endregion // Constants

	#region Fields
	static readonly TempHashSetImpl<T>[] entries = new TempHashSetImpl<T>[MAX_DEPTH].Fill(
		() => new TempHashSetImpl<T>()
	);
	#endregion // Fields

	#region Methods
	public static TempHashSetImpl<T> Get()
	{
		return (TempHashSetImpl<T>)TempObjectHelper.TakeFirstUnused(entries);
	}

	public static void Return(TempHashSetImpl<T> builder)
	{
		TempObjectHelper.ReturnUsed(entries, builder);
	}
	#endregion // Methods
}

public static class TempDict<K, V>
{
	#region Constants
	const int MAX_DEPTH = 3;
	#endregion // Constants

	#region Fields
	static readonly TempDictImpl<K, V>[] entries = new TempDictImpl<K, V>[MAX_DEPTH].Fill(
		() => new TempDictImpl<K, V>()
	);
	#endregion // Fields

	#region Methods
	public static TempDictImpl<K, V> Get()
	{
		return (TempDictImpl<K, V>)TempObjectHelper.TakeFirstUnused(entries);
	}

	public static void Return(TempDictImpl<K, V> builder)
	{
		TempObjectHelper.ReturnUsed(entries, builder);
	}
	#endregion // Methods
}

// NOTE: Needs to be manually setup since property blocks cannot be
//       created off-thread
public static class TempMatPropBlock
{
	#region Constants
	const int MAX_DEPTH = 5;
	#endregion // Constants

	#region Fields
	static TempMatPropBlockImpl[] entries;
	#endregion // Fields

	#region Methods
	// NOTE: For this class to be used this method needs to be called
	//       on the main thread
	public static void Setup()
	{
		entries = new TempMatPropBlockImpl[MAX_DEPTH].FillNew();
	}

	public static TempMatPropBlockImpl Get(Renderer rend)
	{
		var ret = (TempMatPropBlockImpl)TempObjectHelper.TakeFirstUnused(entries);

		rend.GetPropertyBlock(ret.obj);

		return ret;
	}

	public static TempMatPropBlockImpl Get()
	{
		return (TempMatPropBlockImpl)TempObjectHelper.TakeFirstUnused(entries);
	}

	public static void Return(TempMatPropBlockImpl builder)
	{
		TempObjectHelper.ReturnUsed(entries, builder);
	}
	#endregion // Methods
}
#endregion // Interface Classes

#region Single Object Classes
public abstract class TempObject<OBJ_T>
	: IDisposable
{
	public bool inUse
	{
		get;
		private set;
	}

	public readonly OBJ_T obj;

	protected TempObject(OBJ_T obj)
	{
		this.obj = obj;
	}

	public void Take()
	{
		if(inUse) { throw new Exception("TempObject already in use"); }

		inUse = true;
	}

	public void Return()
	{
		if(!inUse) { throw new Exception("TempObject not in use"); }

		Clear();
		inUse = false;
	}

	public void Dispose()
	{
		if(inUse) { Return(); }
	}

	protected abstract void Clear();
}

public sealed class TempMatPropBlockImpl
	: TempObject<MaterialPropertyBlock>
{
	public TempMatPropBlockImpl()
		: base(new MaterialPropertyBlock())
	{
	}

	protected override void Clear()
	{
		obj.Clear();
	}
}
#endregion // Single Object Classes

#region Buffer Classes
public abstract class TempBuffer<BUFF_T>
	: TempObject<BUFF_T>
{
	// For backwards-compability
	public BUFF_T buffer
	{
		get
		{
			return obj;
		}
	}

	protected TempBuffer(BUFF_T buffer)
		: base(buffer)
	{
	}

	public abstract int Count
	{
		get;
	}
}

public sealed class TempListImpl<T> : TempBuffer<List<T>>
{
	public TempListImpl()
		: base(new List<T>())
	{
	}

	protected override void Clear()
	{
		buffer.Clear();
	}

	public override int Count
	{
		get { return buffer.Count; }
	}

	public T this[int index]
	{
		get { return buffer[index]; }
		set { buffer[index] = value; }
	}

	public void Add(T item)
	{
		buffer.Add(item);
	}

	public bool Remove(T item)
	{
		return buffer.Remove(item);
	}

	public void RemoveAt(int index)
	{
		buffer.RemoveAt(index);
	}
}

public sealed class TempArrayImpl<T> : TempBuffer<T[]>
{
	public TempArrayImpl(int length)
		: base(new T[length])
	{
	}

	protected override void Clear()
	{
		Array.Clear(buffer, 0, buffer.Length);
	}

	public override int Count
	{
		get { return buffer.Length; }
	}

	public T this[int index]
	{
		get { return buffer[index]; }
		set { buffer[index] = value; }
	}
}

public sealed class TempStringBuilderImpl : TempBuffer<StringBuilder>
{
	public TempStringBuilderImpl()
		: base(new StringBuilder())
	{
	}

	protected override void Clear()
	{
		buffer.Length = 0;
	}

	public override int Count
	{
		get { return buffer.Length; }
	}

	public char this[int index]
	{
		get { return buffer[index]; }
		set { buffer[index] = value; }
	}

	public override string ToString()
	{
		return buffer.ToString();
	}
}

public sealed class TempHashSetImpl<T> : TempBuffer<HashSet<T>>
{
	public TempHashSetImpl()
		: base(new HashSet<T>())
	{
	}

	protected override void Clear()
	{
		buffer.Clear();
	}

	public override int Count
	{
		get { return buffer.Count; }
	}

	public void Add(T item)
	{
		buffer.Add(item);
	}

	public bool Remove(T item)
	{
		return buffer.Remove(item);
	}
}

public sealed class TempDictImpl<K, V>
	: TempBuffer<Dictionary<K, V>>, IEnumerable<KeyValuePair<K, V>>
{
	public TempDictImpl()
		: base(new Dictionary<K, V>())
	{
	}

	protected override void Clear()
	{
		buffer.Clear();
	}

	public override int Count
	{
		get { return buffer.Count; }
	}

	public V this[K key]
	{
		get { return buffer[key]; }
		set { buffer[key] = value; }
	}

	public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
	{
		return buffer.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return buffer.GetEnumerator();
	}
}
#endregion // Buffer Classes

#region Helper Classes
public static class TempObjectHelper
{
	public static TempObject<T> TakeFirstUnused<T>(TempObject<T>[] entries)
	{
		for(int i = 0; i < entries.Length; ++i)
		{
			TempObject<T> obj = entries[i];
			if(!obj.inUse)
			{
				obj.Take();
				return obj;
			}
		}

		throw new Exception("No more temporary objects available");
	}

	public static void ReturnUsed<T>(TempObject<T>[] entries, TempObject<T> obj)
	{
		for(int i = 0; i < entries.Length; ++i)
		{
			var entry = entries[i];
			if(entry == obj)
			{
				entry.Return();
				return;
			}
		}

		throw new Exception("Tried returning unknown object");
	}
}
#endregion // Helper Classes
