using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class EnemyManager : MonoBehaviour
{
	#region Types
	#endregion // Types

	#region Static Fields
	#endregion // Static Fields

	#region Fields
	UnitManager unitMan;
	#endregion // Fields

	#region Static Properties
	static EnemyManager instance
	{
		get;
		set;
	}
	#endregion // Static Properties
	
	#region Properties
	#endregion // Properties

	#region Methods
	#region System
	public static EnemyManager Setup(
		UnitManager unitMan
	)
	{
		instance = new GameObject("EnemyManager").AddComponent<EnemyManager>();

		instance.SetupInternal(
			unitMan
		);

		return instance;
	}

	void SetupInternal(
		UnitManager unitMan
	)
	{
		this.unitMan = unitMan;
	}

	public void Shutdown()
	{
	}
	
	public void SystemFixedUpdate()
	{
	}
	
	public void SystemUpdate()
	{
		using(var enemies = TempList<EnemyQuirk>.Get())
		using(var players = TempList<Unit>.Get())
		{
			unitMan.GetQuirks(enemies.buffer);
			unitMan.GetUnits(players.buffer, notInFaction: 0);

			UpdateEnemies(enemies.buffer, players.buffer);
		}
	}

	void UpdateEnemies(List<EnemyQuirk> enemies, List<Unit> players)
	{
		for(int i = 0; i < enemies.Count; i++)
		{
			EnemyQuirk quirk = enemies[i];
			UpdateEnemy(quirk, players);
		}
	}

	void UpdateEnemy(EnemyQuirk quirk, List<Unit> players)
	{
		Unit enemyUnit = quirk.unit;
		Unit.ManagerState.AiData aiData = enemyUnit.manState.aiData;

		switch(aiData.state)
		{
		case Unit.AiState.Idle:
			Unit closestPlayer = unitMan.GetClosestUnit(enemyUnit, players);
			aiData.closestTarget = closestPlayer;

			if(closestPlayer == null) { break; }

			if(unitMan.DistanceBetweenUnits(enemyUnit, closestPlayer) <= quirk.detectRange)
			{
				aiData.state = Unit.AiState.Chasing;
				aiData.chasingTarget = closestPlayer;
			}
			break;

		case Unit.AiState.Chasing:
			if(unitMan.DistanceBetweenUnits(enemyUnit, aiData.chasingTarget) > quirk.chaseRange)
			{
				aiData.state = Unit.AiState.Idle;
			}
			break;
		}
	}

	Unit GetClosestUnit(Unit fromUnit, List<Unit> otherUnits)
	{
		Unit closest = null;
		float closestDist = float.PositiveInfinity;

		Vector3 fromPos = fromUnit.transform.position;

		for(int i = 0; i < otherUnits.Count; i++)
		{
			Unit otherUnit = otherUnits[i];

			if(otherUnit == fromUnit) { continue; }

			float dist = Vector3.Distance(
				fromPos,
				otherUnit.transform.position
			);

			if(dist < closestDist)
			{
				closest = otherUnit;
				closestDist = dist;
			}
		}

		return closest;
	}
	
	public void SystemLateUpdate()
	{
	}
	
	public void SystemPostRender()
	{
	}
	#endregion // System
	#endregion // Methods
}
