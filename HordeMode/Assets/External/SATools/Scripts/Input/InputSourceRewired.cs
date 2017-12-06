#if USE_REWIRED
using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using RE = Rewired;

public sealed class InputSourceRewired : InputSource
{
    #region Types
    class ActionPair
    {
        public InputActionDef def;
        public InputAction action;
    }
    #endregion // Types

    #region Fields
    readonly RE.Player player;
    readonly ActionPair[] actions;
    readonly Dictionary<InputActionDef, int> actionToIndex;
    #endregion // Fields

    #region Properties
    #endregion // Properties

    #region Methods
    public InputSourceRewired(int playerId, InputActionDefs possibleActions)
    {
        if (playerId == -1)
        {
            player = RE.ReInput.players.SystemPlayer;
            player.controllers.hasKeyboard = true;
            player.controllers.hasMouse = true;
        }
        else
        {
            player = RE.ReInput.players.GetPlayer(playerId);
        }

        List<InputActionDef> defs = possibleActions.values;
        actions = new ActionPair[defs.Count];
        actionToIndex = new Dictionary<InputActionDef, int>();
        for (int i = 0; i < defs.Count; i++)
        {
            var def = defs[i];

            actions[i] = new ActionPair
            {
                def = def,
                action = new InputAction(def.kind),
            };
            actionToIndex[def] = i;
        }
    }

    public override void DoUpdate()
    {
		for(int i = 0; i < actions.Length; i++)
		{
			ActionPair pair = actions[i];

			switch(pair.def.kind)
			{
				case InputAction.Kind.Button:
					{
						// Fix something here! Is called for 5/6 Frames
						bool held = player.GetButton(pair.def.rewiredAction1);
						bool oldHeld = pair.action.buttonHeld;
						var change =
							held == oldHeld ? InputAction.ValueChange.None :
							held ? InputAction.ValueChange.NotZero :
							InputAction.ValueChange.Zero
						;
						pair.action = new InputAction(held, change);
						break;
					}
				case InputAction.Kind.Axis:
					{
						float value = player.GetAxis(pair.def.rewiredAction1);
						float oldValue = pair.action.axis;
						var change =
							value == oldValue ? InputAction.ValueChange.None :
							Math.Abs(value) > 0.0f ? InputAction.ValueChange.NotZero :
							InputAction.ValueChange.Zero
						;
						pair.action = new InputAction(value, change);
						break;
					}

				case InputAction.Kind.Axis2D:
					{
						Vector2 value = player.GetAxis2D(pair.def.rewiredAction1, pair.def.rewiredAction2);
						Vector2 oldValue = pair.action.axis2D;
						var change =
							value.sqrMagnitude == oldValue.sqrMagnitude ? InputAction.ValueChange.None :
							value.sqrMagnitude > 0.0f ? InputAction.ValueChange.NotZero :
							InputAction.ValueChange.Zero
						;
						pair.action = new InputAction(value, change);
						break;
					}
			}
		}
    }

    public override InputAction GetAction(InputActionDef def)
    {
        int index = actionToIndex[def];
        return actions[index].action;
    }
    #endregion // Methods
}
#endif // USE_REWIRED