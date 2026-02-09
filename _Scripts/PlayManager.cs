using Godot;
using System;
using System.Collections.Generic;

public partial class PlayManager : Node
{
	public static PlayManager Instance { get; private set; }

	[Export] private PackedScene playerPrefab;
	[Export] private PackedScene BallPrefab;
	[Export] private CameraController mainCam;

	[Export] public Control playSelectionUI;
	
	[Export] public Play OffencePlay;
	[Export] public Play DefencePlay;
	
	[Export] public Play KickoffPlay;
	
	[Export] public float lineOfScrimmage = 0;
	[Export] public float firstDownLine = 0;

	[Export] public int ScoreTeam1 = 0;
	[Export] public int ScoreTeam2 = 0;
	[Export] public int PlayDirection = 1;
	[Export] public int CurrentDown = 1;

	public bool inbetweenPlays;

	public bool isExtraPointPlay;
	public bool isKickoff;
	
	bool timerRunning = false;
	public bool midTurnover = false;
	public float quarterTimer;
	public int quarterNumber = 1;
	
	List<PlayerController> reciverPositions = new List<PlayerController>();
	
	public static event Action<bool> InitPlay;
	public static event Action UpdateScore;
	public static event Action<bool> EndPlay;
	
	private GameManager gm;
	PlaySelectionUIManager psm;
	public override void _EnterTree()
	{
		base._EnterTree();
		Instance = this;
		EndPlay += PlayEnded;
		Ball.BallCaught += BallCaught;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		EndPlay -= PlayEnded;
		Ball.BallCaught -= BallCaught;
	}

	public override void _Ready()
	{
		gm = GameManager.Instance;
		psm = PlaySelectionUIManager.Instance;
		
		var ball = BallPrefab.Instantiate<Node3D>();
		ball.Position = Vector3.Zero;
		AddChild(ball);

		mainCam.target = ball;

		isExtraPointPlay = false;
		
		SpawnPlayers();
		FirstDown();
		CurrentDown--;
		quarterTimer = gm.QuarterLengthMin * 60;
		
		Kickoff(1);
		StartGame();
	}

	async void StartGame()
	{
		await ToSignal(GetTree().CreateTimer(.25f), "timeout");
		
		//playSelectionUI.Visible = true;
		//psm.Init(false);
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if(timerRunning) quarterTimer -= (float)delta;
		if(quarterTimer < 0)
		{
			quarterTimer = gm.QuarterLengthMin * 60;
			quarterNumber++;
		}
	}
	
	void SpawnPlayers()
	{
		quarterTimer = gm.QuarterLengthMin * 60;
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
			((PlayerController)player).IsTeam1 = true;
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
			((PlayerController)player).IsTeam1 = false;
			// ((PlayerController)player).playerID = index;
			
			switch (DefencePlay.PlayerDataDefence[i].PlayerType.PlayerType)
			{
				case PlayerType.DLineman :
					((PlayerController)player).StartColor = Colors.Firebrick;
					break;
				case PlayerType.Safety :
					((PlayerController) player).StartColor = DefencePlay.PlayerDataDefence[i].coverZone > .5f ? Colors.Gold : Colors.Plum;
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

		reciverPositions = new List<PlayerController>();

		bool isSpecialTeams = isKickoff;
		
		int team1PlayerIndex = 0;
		int team2PlayerIndex = 0;
		
		int r = 0;
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

				isSpecialTeams = play.IsSpecialTeams;
				
				Vector2 pos = OffencePlay.PlayerDataOffence[o].Position;
				gm.players[i].Position = new Vector3(lineOfScrimmage + pos.Y * PlayDirection, 1, pos.X);
				gm.players[i].playerStats = play.PlayerType;
				gm.players[i].isOffence = true;

				if (isKickoff)
				{
					gm.players[i].Position = new Vector3((gm.fieldLength / 6f) * pos.Y * PlayDirection, 1, pos.X);
					Ball.Instance.endPoint = gm.players[i].GlobalPosition;
					BallCatchData data = new BallCatchData
					{
						BallDot = 1,
						CatchPriority = float.MaxValue,
						DistanceToBall = gm.players[i].GlobalPosition.DistanceTo(Ball.Instance.GlobalPosition),
						DistanceToTarget = gm.players[i].GlobalPosition.DistanceTo(Ball.Instance.GlobalPosition),
						Player = gm.players[i]
					};
					Ball.Instance.AddCatchOption(data);
				}
				gm.players[i].Name = (play.PlayerType.PlayerType + " " + o);
				
				if(gm.players[i].playerStats.canBeThrowTarget) reciverPositions.Add(gm.players[i]);
				
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
					gm.players[i].inputID = input.PlayerID;
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
				gm.players[i].isOffence = false;
				
				if (isKickoff)
				{
					gm.players[i].Position = new Vector3(((gm.fieldLength / 6f) * pos.Y * PlayDirection) - 2, 1, pos.X);
					if (d == 0)
					{
						gm.players[i].HasBall = true;
						Ball.Instance.startPoint = gm.players[i].GlobalPosition;
					}
				}
				
				gm.players[i].Name = (play.PlayerType.PlayerType + " " + d);
				
				if(play.followPlayer > 0 && r < reciverPositions.Count)
				{
					gm.players[i].Position = new Vector3(gm.players[i].Position.X + 2 * PlayDirection, gm.players[i].Position.Y,
						reciverPositions[r].GlobalPosition.Z);
					gm.players[i].aiManager.targetPlayer = reciverPositions[r];
					r++;
				}
				
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
					gm.players[i].inputID = input.PlayerID;
					gm.players[i].isPlayerControlled = true;
				}
				d++;
			}
		}
		
		FieldManager.Instance.SetFieldLines(lineOfScrimmage, firstDownLine);
		if(isKickoff)
		{
			FieldManager.Instance.SetFieldLines(gm.fieldLength, gm.fieldLength);
			lineOfScrimmage = -gm.fieldLength * PlayDirection;
			firstDownLine = -gm.fieldLength * PlayDirection;
		}
		inbetweenPlays = false;
		StartTimer();
		InitPlay?.Invoke(isSpecialTeams || isKickoff);
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
		gm.players[i].aiManager.currentZone = pD.Zone;
		gm.players[i].inputManager = null;
		gm.players[i].isPlayerControlled = false;
		gm.players[i].inputID = -1;
	}
	
	public void PlayEnded(bool moveLineOfScrimmage)
	{
		

		bool kickoff = false;
		bool score = false;
		
		StopTimer();
		
		float newLos = MathF.Round(Ball.Instance.GlobalPosition.X * 10) / 10;
		
		if ((((PlayDirection == 1 && newLos > firstDownLine) || (PlayDirection == -1 && newLos < firstDownLine)) &&
		     moveLineOfScrimmage) || isKickoff)
		{
			lineOfScrimmage = newLos;
			FirstDown();
		}
		
		if ((newLos * PlayDirection >= gm.fieldLength / 2f &&
		     newLos * PlayDirection <= (gm.fieldLength / 2f) + gm.EndzoneDepth))
		{
			if(!isExtraPointPlay)
			{
				Touchdown();
				score = true;
				isExtraPointPlay = true;
				playSelectionUI.Visible = true;
				psm.Init(CurrentDown == 1 || score);
				Ball.Instance.GlobalPosition = Vector3.Right * lineOfScrimmage;
				mainCam.GlobalPosition = Vector3.Right * lineOfScrimmage;
				isKickoff = false;
				return;
			}
			else
			{
				ExtraPointPlay();
				isExtraPointPlay = false;
				Kickoff();
				return;
			}
		}
		
		if ((newLos * -PlayDirection >= gm.fieldLength / 2f &&
		     newLos * -PlayDirection <= (gm.fieldLength / 2f) + gm.EndzoneDepth))
		{
			kickoff = true;
			Safety();
		}

		if (kickoff || isExtraPointPlay)
		{
			Kickoff();
			return;
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
			playSelectionUI.Visible = true;
			psm.Init(CurrentDown == 1 || score);
			mainCam.GlobalPosition = Vector3.Right * lineOfScrimmage;
			//StartPlay();
			return;
		}
		
		
		lineOfScrimmage = newLos;
		playSelectionUI.Visible = true;
		psm.Init(CurrentDown == 1 || score);
		Ball.Instance.GlobalPosition = Vector3.Right * lineOfScrimmage;
		mainCam.GlobalPosition = Vector3.Right * lineOfScrimmage;
		isKickoff = false;
		//StartPlay();
	}

	void FirstDown()
	{
		GD.Print("FirstDown");
		CurrentDown = gm.DownsTillTurnover + 1;
		firstDownLine = lineOfScrimmage + gm.yardsToFirstDown * PlayDirection;
	}

	async void Kickoff(int playDirection = 0)
	{
		Turnover(true, playDirection);
		await ToSignal(GetTree().CreateTimer(.5f), "timeout");
		OffencePlay = KickoffPlay;
		DefencePlay = KickoffPlay;
		isKickoff = true;
		playSelectionUI.Visible = false;
		CurrentDown = 0;
		StartPlay();
	}

	void Touchdown()
	{
		GD.Print("Touchdown");
		Score(PlayDirection == 1, gm.TouchdownScoreValue);
		lineOfScrimmage = (gm.fieldLength / 2f - gm.ExtraPointLineOfScrimmage) * PlayDirection;
	}

	void ExtraPointPlay()
	{
		GD.Print("ExtraPointPlay");
		Score(PlayDirection == 1, gm.ExtraPointPlayScoreValue);
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
		UpdateScore?.Invoke();
	}
	
	public async void Turnover(bool playStillActive, int forcePlayDirection = 0)
	{
		GD.Print("Turnover");
		midTurnover = true;
		float rotTarget = mainCam.Rotation.Y + Mathf.Pi;
		
		foreach (var p in gm.players)
		{
			if(forcePlayDirection != 0)
			{
				if (p.IsTeam1) p.isOffence = forcePlayDirection == 1;
				else p.isOffence = forcePlayDirection == -1;
			}
			else
				p.isOffence = !p.isOffence;
			p.CanMove = false;
			p.CanAct = false;
		}
		if(forcePlayDirection != 0) rotTarget = forcePlayDirection == 1 ? 0 : Mathf.Pi;

		float rotDuration = .5f;

		if (playSelectionUI.Visible) rotDuration = 0;
		
		var tween = CreateTween();
		tween.TweenProperty(mainCam.GetNode("."), "rotation:y", rotTarget, rotDuration);
		await ToSignal(tween, "finished");
		//mainCam.RotationDegrees += Vector3.Up * 180;
		PlayDirection *= -1;
		if (forcePlayDirection != 0) PlayDirection = forcePlayDirection;
		
		FirstDown();

		foreach (var i in gm.playerInputTeam1)
		{
			if (forcePlayDirection != 0)
			{
				i.isOffence = forcePlayDirection == 1;
			}
			else
				i.isOffence = !i.isOffence;
		}
		foreach (var i in gm.playerInputTeam2)
		{
			if (forcePlayDirection != 0)
			{
				i.isOffence = forcePlayDirection == -1;
			}
			else
				i.isOffence = !i.isOffence;
		}

		foreach (var p in gm.players)
		{
			p.CanMove = true;
			p.CanAct = true;
		}

		if(PlayDirection == 1) GD.Print("Team 1 off Team 2 Def");
		if(PlayDirection == -1) GD.Print("Team 2 off Team 1 Def");
		
		if(!playStillActive)
		{
			playSelectionUI.Visible = true;
			psm.Init(false);
			mainCam.GlobalPosition = Vector3.Right * lineOfScrimmage;
		}

		midTurnover = false;
	}
	
	public void StartTimer() {timerRunning = true;}
	public void StopTimer() {timerRunning = false;}
	
	private void BallCaught(bool isOff)
	{
		if(!isOff && !isKickoff) Turnover(true);
	}
	
	public async void InvokeEndPlay(bool moveLineOfScrimmage)
	{
		inbetweenPlays = true;
		await ToSignal(GetTree().CreateTimer(.25f), "timeout");
		EndPlay?.Invoke(moveLineOfScrimmage);
	}
}
