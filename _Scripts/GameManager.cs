using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }

	[Export] public int yardsToFirstDown;
	[Export] public int fieldLength = 100;
	[Export] public int fieldWidth = 53;
	[Export] public int EndzoneDepth = 30;
	[Export] public InputManager[] playerInputManagers;

	
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
}
