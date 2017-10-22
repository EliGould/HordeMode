using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class EnemyQuirk : UnitQuirk
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
	[SerializeField]
	public float detectRange = 5.0f;
	[SerializeField]
	public float chaseRange = 20.0f;
#pragma warning restore 0649
	#endregion // Serialized Fields

	
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Methods
	public override void DoUpdate()
	{
		Unit.State.Persistent persistent = unit.state.persistent;
		Unit.State.Momentary momentary = unit.state.momentary;
		Unit.ManagerState manState = unit.manState;
		Unit.ManagerState.AiData aiData = manState.aiData;

		switch(aiData.state)
		{
		case Unit.AiState.Idle:
			persistent.navTarget = manState.homePoint;
			break;
		case Unit.AiState.Chasing:
			persistent.navTarget = aiData.closestTarget.transform.position;
			break;
		}
	}
	#endregion // Methods
}
