using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class PlayerQuirk : UnitQuirk
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
	[Serializable]
	class InputData
	{
		[SerializeField]
		public InputActionField move;
		[SerializeField]
		public InputActionField aim;
		[SerializeField]
		public InputActionField jump;
		[SerializeField]
		public InputActionField fire;
		[SerializeField]
		public InputActionField changeWeapon;
	}
#pragma warning restore 0649
	#endregion // Serialized Types
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField]
	InputData inputData;
#pragma warning restore 0649
	#endregion // Serialized Fields
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Methods
	public override void DoUpdate()
	{
		var inputMan = InputManager.instance;
		Unit.State.Momentary momentary = unit.state.momentary;

		InputSource input = unit.manState.owner.inputSource;
		momentary.moveInput = inputMan.GetAction(input, inputData.move).axis2D;
		momentary.aimInput = inputMan.GetAction(input, inputData.aim).axis2D;
		momentary.jumpInput = inputMan.GetAction(input, inputData.jump).buttonDown;
		momentary.fireInput = inputMan.GetAction(input, inputData.fire).buttonHeld;
		momentary.weaponChangeInput = inputMan.GetAction(input, inputData.changeWeapon).buttonDown;
	}
	#endregion // Methods
}
