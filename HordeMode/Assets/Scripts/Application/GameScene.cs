using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class GameScene : MonoBehaviour
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
#pragma warning restore 0649
	#endregion // Serialized Types
	#endregion // Types

	#region Fields
	protected SoundManager soundMan;
	protected UnitManager unitMan;
	protected InputManager inputMan;
	protected PlayerManager playerMan;
	//protected GameUi gameUi;
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	public void Preinitialize()
	{
		soundMan = SoundManager.instance;
		unitMan = UnitManager.instance;
		inputMan = InputManager.instance;
		playerMan = PlayerManager.instance;
        //gameUi = GameUi.instance;
	}

	public virtual void Setup() { }
	public virtual void Shutdown() { }
	public virtual void SystemUpdate() { }
	#endregion // Methods
}
