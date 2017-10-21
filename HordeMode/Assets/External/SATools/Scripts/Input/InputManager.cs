using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class InputManagerBase : MonoBehaviour
{
	public virtual void SetupExternal() { }
	public virtual void ShutdownExternal() { }
}

public sealed partial class InputManager : InputManagerBase
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
#pragma warning restore 0649
	#endregion // Serialized Types
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
#pragma warning restore 0649
	#endregion // Serialized Fields

	InputSettings settings;

	List<InputSource> inputSources = new List<InputSource>();
	#endregion // Fields

	#region Static Properties
	public static InputManager instance
	{
		get;
		private set;
	}
	#endregion // Static Properties

	#region Properties
	public InputActionDefs actionDefs
	{
		get { return settings.actions; }
	}
	#endregion // Properties

	#region Methods
	#region System
	public static InputManager Setup(
		InputSettings settings,
		GameObject rewired
	)
	{
		instance = new GameObject("InputManager").AddComponent<InputManager>();

		instance.SetupInternal(
			settings,
			rewired
		);

		return instance;
	}

	void SetupInternal(
		InputSettings settings,
        GameObject rewired
	)
	{
		this.settings = settings;

		SetupExternal();
	}

	public void Shutdown()
	{
		ShutdownExternal();

		instance = null;
	}

	public void SystemUpdate()
	{
		for(int i = 0; i < inputSources.Count; i++)
		{
			InputSource source = inputSources[i];
			source.DoUpdate();
		}
	}
	#endregion // System

	#region Interface
	public InputSource CreateSource(int playerId)
	{
		InputSource source;
#if USE_REWIRED
		source = new InputSourceRewired(playerId, actionDefs);
#else
		throw new NotImplementedException();
#endif

		inputSources.Add(source);

		return source;
	}

	public InputAction GetAction(InputSource source, InputActionField field)
	{
		InputActionDef def = GetActionDef(field.name);
        return def == null ? new InputAction() : source.GetAction(def);
	}

	public InputAction GetAction(InputSource source, string name)
	{
		InputActionDef def = GetActionDef(name);
		return def == null ? new InputAction() : source.GetAction(def);
	}

	public InputActionDef GetActionDef(InputActionField field)
	{
		return GetActionDef(field.name);
	}

	public InputActionDef GetActionDef(string name)
	{
		return actionDefs.GetValue(name);
	}
	#endregion // Interface
	#endregion // Methods
}
