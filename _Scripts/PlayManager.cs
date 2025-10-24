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
	[Export] public float firstDownLine = 0;

	[Export] public int ScoreTeam1 = 0;
	[Export] public int ScoreTeam2 = 0;
	[Export] public int PlayDirection = 1;
	[Export] public int CurrentDown = 1;

	private bool inbetweenPlays;
	
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
		FirstDown();
		StartPlay();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	void SpawnPlayers()
	{
		CurrentDown = gm.DownsTillTurnover;
		PlayDirection = 1;
		
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

		int team1PlayerIndex = 0;
		int team2PlayerIndex = 0;
		
		int o = 0;
		int d = 0;
		for (int i = 0; i < gm.players.Length; i++)
		{
			gm.players[i].HasBall = false;
			gm.players[i].CanThrow = false;
			gm.players[i].playerID = i;
			gm.players[i].aiManager.currentZone = null;
			gm.players[i].aiManager.currentRoute = null;
			
			if (gm.players[i].isOffence)
			{
				gm.offencePlayers.Add(gm.players[i]);
				PlayerDataOffence play = OffencePlay.PlayerDataOffence[o];
				Vector2 pos = OffencePlay.PlayerDataOffence[o].Position;
				gm.players[i].Position = new Vector3(lineOfScrimmage + pos.Y * PlayDirection, 1, pos.X);
				gm.players[i].playerStats = play.PlayerType;
				
				gm.players[i].Name = (play.PlayerType.PlayerType + " " + i);
				
				if(play.StartsWithBall)
				{
					gm.players[i].HasBall = true;
					gm.players[i].CanThrow = true;
				}
				if(!play.IsPlayer)
				{
					SetPlayerInitalVars(true, i, play, null);
					if(play.Route != null)
					{
						play.Route.currentIndex = 0;
						gm.players[i].aiManager.currentRoute = (Route)play.Route.Duplicate();
					}
				}
				else
				{
					InputManager input;
					
					if (PlayDirection == 1)
					{
						if (gm.playerInputTeam1.Length <= team1PlayerIndex)
						{
							SetPlayerInitalVars(true, i, play, null);
							o++;
							continue;
						}

						input = gm.playerInputTeam1[team1PlayerIndex];
						team1PlayerIndex++;
					}
					else
					{
						if (gm.playerInputTeam2.Length <= team2PlayerIndex)
						{
							SetPlayerInitalVars(true, i, play, null);
							o++;
							continue;
						}

						input = gm.playerInputTeam2[team2PlayerIndex];
						team2PlayerIndex++;
					}
					
					if(input == null) continue;
					
					gm.players[i].inputManager = input;
					gm.players[i].isPlayerControlled = true;
					
				}
				o++;
			}
			else if(!gm.players[i].isOffence)
			{
				gm.defencePlayers.Add(gm.players[i]);
				PlayerDataDefence play = DefencePlay.PlayerDataDefence[d];
				Vector2 pos = play.Position;
				gm.players[i].Position = (Vector3.Right * 1) + new Vector3(lineOfScrimmage + pos.Y * PlayDirection, 1, pos.X);
				gm.players[i].playerStats = play.PlayerType;
				
				gm.players[i].Name = (play.PlayerType.PlayerType + " " + i);
				
				if(!play.IsPlayer)
				{
					SetPlayerInitalVars(false, i, null, play);
				}
				else
				{
					InputManager input;
					
					if (PlayDirection == 1)
					{
						if (gm.playerInputTeam2.Length <= team2PlayerIndex)
						{
							SetPlayerInitalVars(false, i, null, play);
							d++;
							continue;
						}

						input = gm.playerInputTeam2[team2PlayerIndex];
						team2PlayerIndex++;
					}
					else
					{
						if (gm.playerInputTeam1.Length <= team1PlayerIndex)
						{
							SetPlayerInitalVars(false, i, null, play);
							d++;
							continue;
						}

						input = gm.playerInputTeam1[team1PlayerIndex];
						team1PlayerIndex++;
					}
					
					if(input == null) continue;
					
					gm.players[i].inputManager = input;
					gm.players[i].isPlayerControlled = true;
				}
				d++;
			}
		}
		
		FieldManager.Instance.SetFieldLines(lineOfScrimmage, firstDownLine);
		inbetweenPlays = false;
		InitPlay?.Invoke();
	}

	void SetPlayerInitalVars(bool isOffence, int i, PlayerDataOffence pO, PlayerDataDefence pD)
	{
		if(isOffence)
		{
			gm.players[i].aiManager.block = pO.block;
			gm.players[i].aiManager.findOpenSpace = pO.findOpenSpace;
			gm.players[i].aiManager.followRoute = pO.followRoute;
			gm.players[i].aiManager.rushBall = 1;
			gm.players[i].inputManager = null;
			gm.players[i].inputID = -1;
			gm.players[i].isPlayerControlled = false;
			return;
		}
		
		gm.players[i].aiManager.followPlayer = pD.followPlayer;
		gm.players[i].aiManager.coverZone = pD.coverZone;
		gm.players[i].aiManager.rushBall = pD.rushBall;
		gm.players[i].aiManager.block = 1;
		gm.players[i].aiManager.targetPlayer = null;
		gm.players[i].inputManager = null;
		gm.players[i].isPlayerControlled = false;
		gm.players[i].inputID = -1;
	}
	
	public void PlayEnded(bool moveLineOfScrimmage)
	{
		if(inbetweenPlays) return;

		inbetweenPlays = true;
		
		float newLos = MathF.Round(Ball.Instance.GlobalPosition.X * 10) / 10;
		
		if (((PlayDirection == 1 && newLos > firstDownLine) || (PlayDirection == -1 && newLos < firstDownLine)) && moveLineOfScrimmage)
		{
			lineOfScrimmage = newLos;
			FirstDown();
		}
		
		if ((newLos * PlayDirection >= GameManager.Instance.fieldLength / 2f &&
		     newLos * PlayDirection <= (GameManager.Instance.fieldLength / 2f) + GameManager.Instance.EndzoneDepth))
		{
			Touchdown();
		}
		
		if ((newLos * -PlayDirection >= GameManager.Instance.fieldLength / 2f &&
		     newLos * -PlayDirection <= (GameManager.Instance.fieldLength / 2f) + GameManager.Instance.EndzoneDepth))
		{
			Safety();
		}
		
		CurrentDown--;

		if (CurrentDown <= 0)
		{
			if (moveLineOfScrimmage) lineOfScrimmage = newLos;
			Turnover(false);
			return;
		}
		
		if(!moveLineOfScrimmage)
		{
			StartPlay();
			return;
		}
		
		
		lineOfScrimmage = newLos;
		StartPlay();
	}

	void FirstDown()
	{
		GD.Print("FirstDown");
		CurrentDown = gm.DownsTillTurnover;
		firstDownLine = lineOfScrimmage + gm.yardsToFirstDown * PlayDirection;
	}

	void Touchdown()
	{
		GD.Print("Touchdown");
		Score(PlayDirection == 1, gm.TouchdownScoreValue);
	}

	void Safety()
	{
		GD.Print("Safety");
		Score(PlayDirection == -1, gm.SafetyScoreValue);
	}
	void Score(bool isTeam1 ,int scoreValue)
	{
		if(isTeam1) ScoreTeam1 += scoreValue;
		else
		{
			ScoreTeam2 += scoreValue;
		}
	}
	
	public async void Turnover(bool playStillActive)
	{
		GD.Print("Turnover");

		float rotTarget = mainCam.Rotation.Y + Mathf.Pi;
		
		foreach (var p in gm.players)
		{
			p.isOffence = !p.isOffence;
			p.CanMove = false;
			p.CanAct = false;
		}
		
		var tween = CreateTween();
		tween.TweenProperty(mainCam.GetNode("."), "rotation:y", rotTarget, .5f);
		await ToSignal(tween, "finished");
		//mainCam.RotationDegrees += Vector3.Up * 180;
		PlayDirection *= -1;
		
		FirstDown();

		foreach (var i in gm.playerInputTeam1)
		{
			i.isOffence = !i.isOffence;
		}
		foreach (var i in gm.playerInputTeam2)
		{
			i.isOffence = !i.isOffence;
		}
		
		if(PlayDirection == 1) GD.Print("Team 1 off Team 2 Def");
		if(PlayDirection == -1) GD.Print("Team 2 off Team 1 Def");
		
		StartPlay();
	}
	
	public static void InvokeEndPlay(bool moveLineOfScrimmage)
	{
		EndPlay?.Invoke(moveLineOfScrimmage);
	}
}
