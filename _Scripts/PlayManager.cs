using Godot;
using System;
using System.Collections.Generic;

public partial class PlayManager : Node
{
	public static PlayManager Instance { get; private set; }

	[Export] private PackedScene playerPrefab;
	[Export] private PackedScene BallPrefab;
	[Export] private CameraController mainCam;
	
	[Export] public Play OffencePlay;
	[Export] public Play DefencePlay;
	
	[Export] public float lineOfScrimmage = 0;

	[Export] public int PlayDirection = 1;
	[Export] public int CurrentDown = 1;
	
	public static event Action InitPlay;
	public static event Action<bool> EndPlay;

	
	private GameManager gm;

	public override void _EnterTree()
	{
		base._EnterTree();
		Instance = this;
		EndPlay += PlayEnded;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		EndPlay -= PlayEnded;
	}

	public override void _Ready()
	{
		gm = GameManager.Instance;
		
		var ball = BallPrefab.Instantiate<Node3D>();
		ball.Position = Vector3.Zero;
		AddChild(ball);

		mainCam.target = ball;
		
		SpawnPlayers();
		StartPlay();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	void SpawnPlayers()
	{
		gm.players = new PlayerController[OffencePlay.PlayerDataOffence.Length + DefencePlay.PlayerDataDefence.Length];
		int index = 0;
		for(int i = 0; i < OffencePlay.PlayerDataOffence.Length; i++)
		{
			var player = playerPrefab.Instantiate<Node3D>();
			player.Position = Vector3.Zero;
			player.Name = (OffencePlay.PlayerDataOffence[i].PlayerType.PlayerType + " " + i);// = new StringName();
			((PlayerController)player).playerStats = OffencePlay.PlayerDataOffence[i].PlayerType;
			((PlayerController)player)._mainCam = mainCam;
			((PlayerController)player).isOffence = true;
			// ((PlayerController)player).playerID = index;

			switch (OffencePlay.PlayerDataOffence[i].PlayerType.PlayerType)
			{
				case PlayerType.Quarterback :
					((PlayerController)player).StartColor = Colors.White;
					break;
				case PlayerType.OLineman :
					((PlayerController)player).StartColor = Colors.Moccasin;
					break;
				case PlayerType.Receiver :
					((PlayerController)player).StartColor = Colors.Chartreuse;
					break;
			}
			
			AddChild(player);
			gm.players[index] = player as PlayerController;
			
			index++;
		}
		
		for(int i = 0; i < DefencePlay.PlayerDataDefence.Length; i++)
		{
			var player = playerPrefab.Instantiate<Node3D>();
			player.Position = Vector3.Zero;
			player.Name = (DefencePlay.PlayerDataDefence[i].PlayerType.PlayerType + " " + i);// = new StringName();
			((PlayerController)player).playerStats = DefencePlay.PlayerDataDefence[i].PlayerType;
			((PlayerController)player).aiManager.currentZone = DefencePlay.PlayerDataDefence[i].Zone;
			((PlayerController)player)._mainCam = mainCam;
			((PlayerController)player).isOffence = false;
			// ((PlayerController)player).playerID = index;
			
			switch (DefencePlay.PlayerDataDefence[i].PlayerType.PlayerType)
			{
				case PlayerType.DLineman :
					((PlayerController)player).StartColor = Colors.Firebrick;
					break;
				case PlayerType.Safety :
					((PlayerController)player).StartColor = Colors.Plum;
					break;
			}
			
			AddChild(player);
			gm.players[index] = player as PlayerController;
			index++;
		}
	}
	
	public void StartPlay()
	{
		gm.offencePlayers = new List<PlayerController>();
		gm.defencePlayers = new List<PlayerController>();
		
		Ball.Instance.ballState = BallState.Held;
		Ball.Instance.GlobalPosition = Vector3.Up;
		
		int o = 0;
		int d = 0;
		for (int i = 0; i < gm.players.Length; i++)
		{
			if (gm.players[i].isOffence)
			{
				gm.offencePlayers.Add(gm.players[i]);
				PlayerDataOffence play = OffencePlay.PlayerDataOffence[o];
				Vector2 pos = OffencePlay.PlayerDataOffence[o].Position;
				gm.players[i].Position = new Vector3(lineOfScrimmage + pos.Y, 1, pos.X);
				GD.Print(gm.players[i].Name +" : " + play.IsPlayer);
				if(play.StartsWithBall)
				{
					gm.players[i].HasBall = true;
					gm.players[i].CanThrow = true;
				}
				if(!play.IsPlayer)
				{
					gm.players[i].aiManager.block = play.block;
					gm.players[i].aiManager.findOpenSpace = play.findOpenSpace;
					gm.players[i].aiManager.followRoute = play.followRoute;
					gm.players[i].inputManager = null;
					gm.players[i].playerID = -1;
					gm.players[i].isPlayerControlled = false;
					gm.players[i].HasBall = false;
					if(play.Route != null)
					{
						play.Route.currentIndex = 0;
						gm.players[i].aiManager.currentRoute = (Route)play.Route.Duplicate();
					}
				}
				else
				{
					gm.players[i].inputManager = gm.playerInputManagers[0];
					gm.players[i].isPlayerControlled = true;
				}
				o++;
			}
			else if(!gm.players[i].isOffence)
			{
				gm.defencePlayers.Add(gm.players[i]);
				PlayerDataDefence play = DefencePlay.PlayerDataDefence[d];
				Vector2 pos = play.Position;
				gm.players[i].Position = (Vector3.Right * 1) + new Vector3(lineOfScrimmage + pos.Y, 1, pos.X);
				if(!play.IsPlayer)
				{
					gm.players[i].aiManager.followPlayer = play.followPlayer;
					gm.players[i].aiManager.coverZone = play.coverZone;
					gm.players[i].aiManager.rushBall = play.rushBall;
					gm.players[i].inputManager = null;
					gm.players[i].isPlayerControlled = false;
					gm.players[i].playerID = -1;
				}
				else
				{
					gm.players[i].inputManager = gm.playerInputManagers[0];
					gm.players[i].isPlayerControlled = true;
				}
				d++;
			}
		}
		
		FieldManager.Instance.SetFieldLines(lineOfScrimmage);
		
		InitPlay?.Invoke();
	}

	public void PlayEnded(bool moveLineOfScrimmage)
	{
		if(!moveLineOfScrimmage)
		{
			StartPlay();
			return;
		}
		
		lineOfScrimmage = MathF.Round(Ball.Instance.GlobalPosition.X * 10) / 10;
		StartPlay();
	}

	public static void InvokeEndPlay(bool moveLineOfScrimmage)
	{
		EndPlay?.Invoke(moveLineOfScrimmage);
	}
}
