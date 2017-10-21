#if USE_REWIRED
using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using RE = Rewired;

public sealed partial class InputManager : InputManagerBase
{
	#region Types
	#endregion // Types

	#region Fields
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Methods
	public override void SetupExternal()
	{
		RE.Player sysPlayer = RE.ReInput.players.SystemPlayer;
		var sysPlayerController = sysPlayer.controllers;

		for(int i = 0; i < EnumHelper<RE.ControllerType>.count; i++)
		{
			var type = EnumHelper<RE.ControllerType>.values[i];
			var controllers = RE.ReInput.controllers.GetControllers(type);
			if(controllers == null) { continue; }

			for(int z = 0; z < controllers.Length; z++)
			{
				var controller = controllers[z];
				sysPlayerController.AddController(controller, removeFromOtherPlayers: false);
            }
		}

		RE.ReInput.ControllerConnectedEvent += OnRewiredControllerConnected;
		RE.ReInput.ControllerPreDisconnectEvent += OnPreRewiredControllerDisconnect;
	}

	public override void ShutdownExternal()
	{
		RE.ReInput.ControllerConnectedEvent -= OnRewiredControllerConnected;
		RE.ReInput.ControllerPreDisconnectEvent -= OnPreRewiredControllerDisconnect;
	}

	void OnRewiredControllerConnected(RE.ControllerStatusChangedEventArgs obj)
	{
		RE.ReInput.players.SystemPlayer.controllers.AddController(obj.controllerType, obj.controllerId, removeFromOtherPlayers: false);
	}

	void OnPreRewiredControllerDisconnect(RE.ControllerStatusChangedEventArgs obj)
	{
		RE.ReInput.players.SystemPlayer.controllers.RemoveController(obj.controllerType, obj.controllerId);
	}
	#endregion // Methods
}
#endif // USE_REWIRED