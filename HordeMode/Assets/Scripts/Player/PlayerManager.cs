using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class PlayerManager : MonoBehaviour
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
#pragma warning restore 0649
	#endregion // Serialized Types

	public class PlayerList : List<Player>
	{
		public bool Remove(int id)
		{
			int index = IndexOfPlayer(id);
			if(index != -1) { RemoveAt(index); }

			return index != -1;
		}

		public int IndexOfPlayer(Player player)
		{
			return IndexOfPlayer(player.id);
		}

		public int IndexOfPlayer(int id)
		{
			for(int i = 0; i < Count; i++)
			{
				if(this[i].id == id) { return i; }
			}

			return -1;
		}
	}
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
#pragma warning restore 0649
	#endregion // Serialized Fields

	InputManager inputMan;

    int instanceIdCounter;
	#endregion // Fields

	#region Static Properties
	public static PlayerManager instance
	{
		get;
		private set;
	}
	#endregion // Static Properties

	#region Properties
	public Player systemPlayer
	{
		get;
		private set;
	}

	public PlayerList players
	{
		get;
		private set;
	}

	public PlayerList allPlayers
	{
		get;
		private set;
	}
	#endregion // Properties

	#region Methods
	#region System
	public static PlayerManager Setup(
		InputManager inputMan
	)
	{
		instance = new GameObject("PlayerManager").AddComponent<PlayerManager>();

		instance.SetupInternal(
			inputMan
		);

		return instance;
	}

	void SetupInternal(
		InputManager inputMan
	)
	{
		this.inputMan = inputMan;

		players = new PlayerList();
		allPlayers = new PlayerList();
	}

	public void Shutdown()
	{
		instance = null;
	}
	#endregion // System

	#region Interface
	public Player CreateSystemPlayer()
	{
		if(systemPlayer != null) { return systemPlayer; }

		systemPlayer = CreatePlayer(id: -1);

		return systemPlayer;
	}

	public Player CreatePlayer()
	{
		Player player = CreatePlayer(instanceIdCounter++);

		players.Add(player);

		return player;
	}

	public void RemovePlayer(Player player)
	{
		if(!allPlayers.Remove(player.id))
		{
			return;
		}

		if(player == systemPlayer)
		{
			systemPlayer = null;
			return;
		}

		players.Remove(player.id);
	}
	#endregion // Interface

	Player CreatePlayer(int id)
	{
		int index = id == -1 ? -1 : players.Count;

		InputSource input = inputMan.CreateSource(id, index);

		var player = new Player(id, index, input);

		allPlayers.Add(player);

		return player;
    }
	#endregion // Methods
}
