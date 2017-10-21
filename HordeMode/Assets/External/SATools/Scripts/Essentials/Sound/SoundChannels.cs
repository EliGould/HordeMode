using UnityEngine;
using UE = UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

[Serializable]
public class SoundChannel : DynEnum
{
	[SerializeField]
	public AudioMixerGroup mixerGroup;
}

[CreateAssetMenu(fileName = "SoundChannels", menuName = "SA/Sound/Sound Channels")]
public class SoundChannels : DynEnums<SoundChannel>
{
}

public class SoundChannelSet : DynEnumSet<SoundChannel>
{
	public SoundChannelSet(List<SoundChannel> dynEnums) : base(dynEnums) { }
}

public class SoundChannelsLoader : DynEnumsLoader<SoundChannel>
{
	public override DynEnums<SoundChannel> Load()
	{
#if UNITY_EDITOR
		return AssetDatabase.LoadAssetAtPath<SoundChannels>("Assets/Data/SoundChannels.asset");
#else
		return null;
#endif
	}
}

[Serializable]
public class SoundChannelField : SingleDynEnumField<SoundChannel>
{
}

[Serializable]
public class SoundChannelsField : DynEnumField<SoundChannel>
{
	public override SoundChannel GetDynEnumByName(string name)
	{
		return SoundManager.instance.channels.GetValue(name);
	}

	public override DynEnumSet<SoundChannel> CreateDynEnumSet(List<SoundChannel> dynEnums)
	{
		return new SoundChannelSet(dynEnums);
	}

	public new SoundChannelSet GetSet()
	{
		return (SoundChannelSet)base.GetSet();
	}
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SoundChannelField))]
public class SoundChannelFieldDrawer : SingleDynEnumFieldDrawer<SoundChannel>
{
	SoundChannelsLoader _loader = new SoundChannelsLoader();

	protected override DynEnumsLoader<SoundChannel> loader
	{
		get { return _loader; }
	}
}

[CustomPropertyDrawer(typeof(SoundChannelsField))]
public class SoundChannelsFieldDrawer : DynEnumFieldDrawer<SoundChannel>
{
	SoundChannelsLoader _loader = new SoundChannelsLoader();

	protected override DynEnumsLoader<SoundChannel> loader
	{
		get { return _loader; }
	}
}
#endif // UNITY_EDITOR