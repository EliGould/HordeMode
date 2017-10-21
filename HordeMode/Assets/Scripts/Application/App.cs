using UnityEngine;
using UE = UnityEngine;
using UnityEngine.PostProcessing;
using System;
using System.Collections;
using System.Collections.Generic;
#if USE_REWIRED
using RE = Rewired;
#endif // USE_REWIRED

public sealed partial class App : AppBase
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
	[Serializable]
	class Settings
	{
		[SerializeField]
		public InputSettings input;
		[SerializeField]
		public SoundSettings sound;
		[SerializeField]
		public EffectSettings effect;
		[SerializeField]
		public SoundChannels soundChannels;
	}

	[Serializable]
	class Prefabs
	{
		[SerializeField]
		public GameObject rewired;
	}
#pragma warning restore 0649
	#endregion // Serialized Types
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField]
	Settings settings;
	[SerializeField]
	Prefabs prefabs;
#pragma warning restore 0649
	#endregion // Serialized Fields

	bool isReadyForSetup;

	Coroutines coroutines;
	DbgValues dbgValues;
#pragma warning disable 0649
	GameObject rewired;
#pragma warning restore 0649
	InputManager inputMan;
	PlayerManager playerMan;
	SoundManager soundMan;
	EffectManager effectMan;
	Camera mainCamera;
	#endregion // Fields

	#region Properties
	protected override bool readyForSetup
	{
		get { return CheckReadyForSetup(); }
	}

	public static float deltaTime
	{
		get { return Mathf.Sign(timeScale) * Time.deltaTime; }
	}
	#endregion // Properties

	#region Methods
	protected override void AtAwake()
	{
#if USE_REWIRED
		rewired = InstantiateAndChild(prefabs.rewired);
#endif // USE_REWIRED
	}

	bool CheckReadyForSetup()
	{
		return true
#if USE_REWIRED
			&& RE.ReInput.isReady
#endif // USE_REWIRED
		;
	}

	protected override void AtSetup()
	{
		coroutines = Coroutines.Setup();

		dbgValues = DbgValues.Setup();

		Child(ref soundMan, SoundManager.Setup(
			settings.sound,
			settings.soundChannels
		));

		Child(ref effectMan, EffectManager.Setup(
			settings.effect
		));

		Child(ref inputMan, InputManager.Setup(
			settings.input,
			rewired
		));

		Child(ref playerMan, PlayerManager.Setup(
			inputMan
		));

		playerMan.CreateSystemPlayer();
	}

	protected override void PreinitializeState(AppState appState)
	{
		var game = appState as Game;
		if(game != null)
		{
			game.Preinitialize(
				mainCamera
			);
		}
	}

	protected override void AtShutdown()
	{
		effectMan.Shutdown();
		effectMan = null;

		soundMan.Shutdown();
		soundMan = null;

		playerMan.Shutdown();
		playerMan = null;

		inputMan.Shutdown();
		inputMan = null;

		if(rewired != null)
		{
			Destroy(rewired);
		}

		dbgValues.Shutdown();
	}

	protected override void AtTimeScaleChanged(float newTimeScale)
	{
		Time.timeScale = Mathf.Abs(newTimeScale);
		soundMan.OnTimeScaleChanged(newTimeScale);
		effectMan.OnTimeScaleChanged(newTimeScale);
	}

	protected override void AtFixedUpdate()
	{
		coroutines.SystemFixedUpdate();
	}

	protected override void AtUpdate()
	{
		if(!isFixedFrame)
		{
			inputMan.SystemUpdate();
		}

		effectMan.SystemUpdate();

		soundMan.SystemUpdate();

		coroutines.SystemUpdate();
	}

	protected override void AtLateUpdate()
	{
	}

	protected override void AtPostRender()
	{
		coroutines.SystemPostRender();
	}

#if UNITY_EDITOR
	protected override void AtGUI()
	{
		dbgValues.SystemOnGUI();
	}
#endif // UNITY_EDITOR
	#endregion // Methods
}
