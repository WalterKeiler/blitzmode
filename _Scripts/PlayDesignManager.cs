using Godot;
using System;
using System.Collections.Generic;

public partial class PlayDesignManager : Node
{
	public static PlayDesignManager Instance;
	
	[Export] public PackedScene playerPrefab;

	private List<PlayDesignPlayer> players;
	public override void _Ready()
	{
		Instance = this;
		players = new List<PlayDesignPlayer>();
	}
	
	public override void _Process(double delta)
	{
	}

	public void SpawnNewPlayer(PlayerType playerType)
	{
		PlayDesignPlayer player = (PlayDesignPlayer)playerPrefab.Instantiate<Node3D>();
		
		player.playerType = playerType;
		players.Add(player);
	}
}
