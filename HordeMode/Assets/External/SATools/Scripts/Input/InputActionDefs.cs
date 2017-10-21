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
public class InputActionDef : DynEnum
{
	[SerializeField]
	public InputAction.Kind kind;
	[SerializeField]
	public int rewiredAction1;
	[SerializeField]
	public int rewiredAction2;
}

[CreateAssetMenu(fileName = "InputActionDefs", menuName = "SA/Input/Action Definitions")]
public class InputActionDefs : DynEnums<InputActionDef>
{
	public List<InputActionDef> values
	{
		get { return dynEnums; }
	}
}

public class InputActionDefSet : DynEnumSet<InputActionDef>
{
	public InputActionDefSet(List<InputActionDef> dynEnums) : base(dynEnums) { }
}

public class InputActionLoader : DynEnumsLoader<InputActionDef>
{
	public override DynEnums<InputActionDef> Load()
	{
#if UNITY_EDITOR
		return AssetDatabase.LoadAssetAtPath<InputActionDefs>("Assets/Data/InputActionDefs.asset");
#else
		return null;
#endif
	}
}

[Serializable]
public class InputActionField : SingleDynEnumField<InputActionDef>
{
}

[Serializable]
public class InputActionsField : DynEnumField<InputActionDef>
{
	public override InputActionDef GetDynEnumByName(string name)
	{
		return InputManager.instance.actionDefs.GetValue(name);
	}

	public override DynEnumSet<InputActionDef> CreateDynEnumSet(List<InputActionDef> dynEnums)
	{
		return new InputActionDefSet(dynEnums);
	}

	public new InputActionDefSet GetSet()
	{
		return (InputActionDefSet)base.GetSet();
	}
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(InputActionField))]
public class InputActionFieldDrawer : SingleDynEnumFieldDrawer<InputActionDef>
{
	InputActionLoader _loader = new InputActionLoader();

	protected override DynEnumsLoader<InputActionDef> loader
	{
		get { return _loader; }
	}
}

[CustomPropertyDrawer(typeof(InputActionsField))]
public class InputActionsFieldDrawer : DynEnumFieldDrawer<InputActionDef>
{
	InputActionLoader _loader = new InputActionLoader();

	protected override DynEnumsLoader<InputActionDef> loader
	{
		get { return _loader; }
	}
}
#endif // UNITY_EDITOR