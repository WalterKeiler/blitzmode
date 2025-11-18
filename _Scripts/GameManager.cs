using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }

	[Export] public TeamData team1;
	[Export] public TeamData team2;
	
	[Export, ExportGroup("Play")] public int yardsToFirstDown = 30;
	[Export] public int DownsTillTurnover = 4;
	[Export] public int TouchdownScoreValue = 6;
	[Export] public int FieldGoalScoreValue = 3;
	[Export] public int SafetyScoreValue = 2;
	[Export] public int ExtraPointKickScoreValue = 1;
	[Export] public int ExtraPointPlayScoreValue = 2;
	[Export] public int NumberOfQuarters = 4;
	[Export] public int QuarterLengthMin = 5;
	[Export] public int timeToPickPlaySec = 30;
	[Export, ExportGroup("Field")] public int fieldLength = 100;
	[Export] public int fieldWidth = 53;
	[Export] public int EndzoneDepth = 30;
	[Export, ExportGroup("Player Input")] public InputManager[] playerInputTeam1;
	[Export] public InputManager[] playerInputTeam2;

	
	public List<PlayerController> offencePlayers;
	public List<PlayerController> defencePlayers;
	
	public PlayerController[] players;

	
	public override void _EnterTree()
	{
		base._EnterTree();
		Instance = this;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public PlayerController GetPlayerControlledPlayer(bool offence)
	{
		if(offence)
		{
			foreach (var player in offencePlayers)
			{
				if (player.isPlayerControlled) return player;
			}
		}
		else
		{
			foreach (var player in defencePlayers)
			{
				if (player.isPlayerControlled) return player;
			}
		}

		return null;
	}

	public InputManager GetInputByPlayerID(int ID)
	{
		foreach (var p1 in playerInputTeam1)
		{
			if (p1.PlayerID == ID) return p1;
		}
		foreach (var p2 in playerInputTeam2)
		{
			if (p2.PlayerID == ID) return p2;
		}

		return null;
	}
	
}
