using UnityEngine;
using UE = UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed partial class Game : GameBase
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
	[Serializable]
	class Settings
	{
		public UnitSettings unit;
	}
#pragma warning restore 0649
	#endregion // Serialized Types
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField]
	Settings settings;
#pragma warning restore 0649
	#endregion // Serialized Fields

	Camera mainCamera;

	int sceneFrameDelay;

	UnitManager unitManager;
	#endregion // Fields

	#region Properties
	public override bool readyForSetup
	{
		get
		{
			return sceneFrameDelay-- <= 0;
		}
	}
	#endregion // Properties

	#region Methods
	public void Preinitialize(
		Camera mainCamera
	)
	{
		this.mainCamera = mainCamera;
		//SceneManager.LoadScene("GUI", LoadSceneMode.Additive);
		//sceneFrameDelay = 1;
	}

	protected override void SetupSystems()
	{
		var unitSetup = new UnitSetupData();
		Child(ref unitManager, UnitManager.Setup(
			settings.unit,
			ref unitSetup
		));

		//gameUi = GameUi.Setup(
		//	mainCamera
		//);
    }

	protected override void ShutdownSystems()
	{
		//gameUi.Shutdown();
		//gameUi = null;

		unitManager.Shutdown();
		unitManager = null;
	}

	public override void AtUpdate()
	{
		unitManager.SystemUpdate();
	}
	#endregion // Methods
}
