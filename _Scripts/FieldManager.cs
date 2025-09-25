using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class FieldManager : Node
{
	public static FieldManager Instance { get; private set; }

	[Export] private PackedScene playerPrefab;
	[Export] private PackedScene BallPrefab;
	[Export] private CameraController mainCam;
	
	[Export] public int fieldLength = 100;
	[Export] public int fieldWidth = 53;
	[Export] int EndzoneDepth = 30;

	[Export] int LineOfScrimmage = 100;
	[Export] public Play OffencePlay;
	[Export] public Play DefencePlay;
	
	[Export] Material fieldMaterial;
	[Export] MeshInstance3D fieldMesh;
	[Export] SceneTree endzonePrefab;
	
	[Export] public InputManager[] playerInputManagers;

	private PlayerController[] players;
	
	List<Node3D> offencePlayers;
	List<Node3D> defencePlayers;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
		
		PlaneMesh m = (PlaneMesh)fieldMesh.Mesh;
		var mat = m.GetMaterial();
		((ShaderMaterial)mat).SetShaderParameter("yardsLength", fieldLength);
		((ShaderMaterial)mat).SetShaderParameter("yardsWidth", fieldWidth);
		((ShaderMaterial)mat).SetShaderParameter("lineOfScrimmage", 0);
		((ShaderMaterial)mat).SetShaderParameter("firstDownLine", 20);
		m.SetMaterial(mat);// = mat;
		m.Size = new Vector2(fieldLength, fieldWidth + 7);

		var ball = BallPrefab.Instantiate<Node3D>();
		ball.Position = Vector3.Zero;
		AddChild(ball);

		mainCam.target = ball;
		
		fieldMesh.Mesh = m;
		SpawnPlayers();
		StartPlay();
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	void SpawnPlayers()
	{
		players = new PlayerController[OffencePlay.PlayerDataOffence.Length + DefencePlay.PlayerDataDefence.Length];
		int index = 0;
		for(int i = 0; i < OffencePlay.PlayerDataOffence.Length; i++)
		{
			var player = playerPrefab.Instantiate<Node3D>();
			player.Position = Vector3.Zero;
			player.Name = ("Offence " + " " + OffencePlay.PlayerDataOffence[i].PlayerType.GetName() + " " + i);// = new StringName();
			((PlayerController)player).playerStats = OffencePlay.PlayerDataOffence[i].PlayerType;
			((PlayerController)player)._mainCam = mainCam;
			((PlayerController)player).isOffence = true;
			AddChild(player);
			players[index] = player as PlayerController;
			index++;
		}
		
		for(int i = 0; i < DefencePlay.PlayerDataDefence.Length; i++)
		{
			var player = playerPrefab.Instantiate<Node3D>();
			player.Position = Vector3.Zero;
			player.Name = ("Defence " + " " + DefencePlay.PlayerDataDefence[i].PlayerType.GetName() + " " + i);// = new StringName();
			((PlayerController)player).playerStats = DefencePlay.PlayerDataDefence[i].PlayerType;
			((PlayerController)player)._mainCam = mainCam;
			((PlayerController)player).isOffence = false;
			AddChild(player);
			players[index] = player as PlayerController;
			index++;
		}
	}
	
	public void StartPlay()
	{
		offencePlayers = new List<Node3D>();
		defencePlayers = new List<Node3D>();
		int o = 0;
		int d = 0;
		for (int i = 0; i < players.Length; i++)
		{
			if (players[i].isOffence)
			{
				offencePlayers.Add(players[i]);
				PlayerDataOffence play = OffencePlay.PlayerDataOffence[o];
				Vector2 pos = OffencePlay.PlayerDataOffence[o].Position;
				players[i].Position = new Vector3(pos.Y, 1, pos.X);
				GD.Print(players[i].Name +" : " + play.IsPlayer);
				if(play.StartsWithBall)
				{
					players[i].HasBall = true;
					players[i].CanThrow = true;
				}
				if(!play.IsPlayer)
				{
					players[i].aiManager.block = play.block;
					players[i].aiManager.findOpenSpace = play.findOpenSpace;
					players[i].aiManager.followRoute = play.followRoute;
					players[i].inputManager = null;
					players[i].isPlayerControlled = false;
					if(play.Route != null)
					{
						play.Route.currentIndex = 0;
						players[i].aiManager.currentRoute = (Route)play.Route.Duplicate();
					}
					players[i].aiManager.init = true;
				}
				else
				{
					players[i].inputManager = playerInputManagers[0];
					players[i].isPlayerControlled = true;
				}
				o++;
			}
			else if(!players[i].isOffence)
			{
				defencePlayers.Add(players[i]);
				PlayerDataDefence play = DefencePlay.PlayerDataDefence[d];
				Vector2 pos = play.Position;
				players[i].Position = (Vector3.Right * 1) + new Vector3(pos.Y, 1, pos.X);
				if(!play.IsPlayer)
				{
					players[i].aiManager.followPlayer = play.followPlayer;
					players[i].aiManager.coverZone = play.coverZone;
					players[i].aiManager.rushBall = play.rushBall;
					players[i].inputManager = null;
					players[i].isPlayerControlled = false;
					players[i].aiManager.init = true;
				}
				else
				{
					players[i].inputManager = playerInputManagers[0];
					players[i].isPlayerControlled = true;
				}
				d++;
			}
			players[i].Init();
		}
	}
}
