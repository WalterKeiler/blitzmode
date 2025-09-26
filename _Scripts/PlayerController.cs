using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;
using Array = System.Array;

public partial class PlayerController : Node3D
{
	public const float MAXTHROWDISTANCE = 60;
	public const float SWITCHTARGETCOOLDOWN = .25f;
	
	[Export] public int playerID = -1;
	[Export] public PlayerStats playerStats;
	[Export(PropertyHint.Range, "0,1,")] float _PlayerSprintAmount = 1;
	[Export] public Node3D _mainCam;
	[Export] public InputManager inputManager;
	[Export] public AIManager aiManager;
	[Export] public bool isPlayerControlled;
	[Export] public bool isOffence;
	[Export] MeshInstance3D mesh;
	[Export] Material mat;
	[Export] bool debugMode;
	
	public List<PlayerActions> PlayerAction;
	public bool HasBall;
	public bool CanCatch;
	public bool CanThrow;
	public bool CanMove;

	Area3D tackleBox;
	Area3D nearbyPayersBox;
	Area3D CatchZone;
	Ball ball;
	
	List<Node3D> PlayersOnTeam;
	List<Node3D> PlayersNotOnTeam;

	bool init = false;
	bool canTakeInput = true;
	Vector3 _moveDirection;
	float _sprintMultiplier;
	float switchTargetTimer;
	PlayerController throwTarget;

	StandardMaterial3D testMat;
	
	Node3D debugBox;
	
	public override void _Ready()
	{
		base._Ready();
		Init();
		
		tackleBox = GetNode<Area3D>("TackleBox");
		nearbyPayersBox = GetNode<Area3D>("NearbyPayersBox");
	}

	// Called when the node enters the scene tree for the first time.
	public void Init()
	{
		//GD.Print(mat.ResourceName);
		PlayerAction = new List<PlayerActions>();
		InputManager.InputPressAction += DoAction;
		InputManager.InputReleaseAction += CancelAction;
		if (inputManager != null)
		{
			isOffence = inputManager.isOffence;
			playerID = inputManager.PlayerID;
		}

		PlayerController[] players = GetParent().GetChildren().OfType<PlayerController>().ToArray();
		PlayersOnTeam = new List<Node3D>();
		PlayersNotOnTeam = new List<Node3D>();
		_moveDirection = Vector3.Zero;
		CanCatch = true;
		CanMove = true;
		switchTargetTimer = 0;
		testMat = (StandardMaterial3D)mat.Duplicate();
		mesh.MaterialOverride = testMat;
		
		ball = Ball.Instance;
		GD.Print("Ball: "  + ball.GetParent().Name);
		if(HasBall) ((Node)ball).Reparent(this, false);
		
		if (players != null)
		{
			for (int i = 0; i < players.Length; i++)
			{
				if (players[i].isOffence == isOffence && players[i] != this)
				{
					//GD.Print(players[i].Name);
					PlayersOnTeam.Add(players[i]);
				}
				else if(players[i].isOffence != isOffence && players[i] != this)
				{
					PlayersNotOnTeam.Add(players[i]);
				}
			}
		}
		init = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!init) return;
		GetInput();
		
		if(CanMove)
			Move(delta);
		
		if (CanThrow && ball.GetParent() == this)// && !HasBall)
		{
			if(switchTargetTimer > 0)
				switchTargetTimer -= (float)delta;
			else
			{
				SelectThrowTarget();
			}
			HasBall = true;
		}
		else if (HasBall) HasBall = false;

		if (Ball.Instance.ballState == BallState.Thrown && CanCatch && !PlayerAction.Contains(PlayerActions.Throw))
		{
			CheckForCatch();
		}
		
		if (ball.ballState is BallState.Free or BallState.Fumbled)
		{
			float dist = ball.GlobalPosition.DistanceTo(GlobalPosition);

			if (dist <= 1.25f)
			{
				ball.Reparent(this, true);
				ball.ballState = BallState.Held;
			}
		}
	}

	void SelectThrowTarget()
	{
		if (debugBox == null)
		{
			MeshInstance3D testMesh = new MeshInstance3D();
			testMesh.Mesh = new BoxMesh();
			testMesh.MaterialOverride = new Material();
			testMesh.Position = Vector3.Zero;
			GetParent().AddChild(testMesh);
			debugBox = testMesh;
		}
		
		float closest = -100000;
		PlayerController target = null;
		Vector3 endPoint = Vector3.Zero;
		for (int i = 0; i < PlayersOnTeam.Count; i++)
		{
			//if(throwTarget != null) break;
			if(!((PlayerController)PlayersOnTeam[i]).playerStats.canBeThrowTarget) continue;
			
			Vector3 dir = GlobalPosition.DirectionTo(PlayersOnTeam[i].GlobalPosition);
			float dot = dir.Dot(_moveDirection);

			if (dot >= closest)
			{
				//GD.Print(dot);
				// if (dot - closest <= .1f)
				// {
				// 	GD.Print("In Line");
				// 	if(GlobalPosition.DistanceTo(PlayersOnTeam[i].GlobalPosition) <= GlobalPosition.DistanceTo(endPoint))
				// 	{
				// 		closest = dot;
				// 		endPoint = PlayersOnTeam[i].GlobalPosition;
				// 		target = (PlayerController)PlayersOnTeam[i];
				// 	}
				// }
				// else
				{
					closest = dot;
					endPoint = PlayersOnTeam[i].GlobalPosition;
					target = (PlayerController)PlayersOnTeam[i];
				}
			}
		}
		
		if (target != null)
		{
			if (throwTarget != target)
			{
				switchTargetTimer = SWITCHTARGETCOOLDOWN;
			}
			throwTarget = target;
		}
		if(throwTarget != null)
		{
			debugBox.GlobalPosition = throwTarget.GlobalPosition;
			debugBox.Scale = Vector3.One;
		}
		else
		{
			GD.Print("No target found");
			throwTarget = GetNearestPlayer(true, false, true);
			debugBox.Scale = Vector3.One * 2;
		}
	}
	
	void GetInput()
	{
		if (playerID == -1 || !canTakeInput) return;
		
		Vector3 inputDir = inputManager.GetDirectionalInput();
		Vector3 xDir = Vector3.Zero;
		Vector3 zDir = Vector3.Zero;
		
		zDir = inputDir.Z * _mainCam.GetGlobalBasis().X.Normalized();
		xDir = -inputDir.X * _mainCam.GetGlobalBasis().Z.Normalized();
		_moveDirection = xDir + zDir;
		
		_moveDirection.Y = 0;
	}

	public void GetInput(Vector3 direction)
	{
		if (playerID != -1 || !canTakeInput) return;
		_moveDirection = direction.Normalized();
		
		_moveDirection.Y = 0;
	}

	void Move(double delta)
	{
		_moveDirection.Normalized();
		Translate(_moveDirection * (float)delta * (playerStats.Speed + _sprintMultiplier));
	}
	
	
	void CheckForCatch()
	{
		float distanceToBall = ball.GlobalPosition.DistanceTo(GlobalPosition);
		float distanceToTarget = ball.endPoint.DistanceTo(GlobalPosition);
		float dot = GetGlobalBasis().Z.Dot(ball.GlobalPosition.DirectionTo(GlobalPosition));

		float catchRadius = 3;

		if (distanceToBall <= catchRadius)
		{
			BallCatchData data = new BallCatchData
			{
				BallDot = dot,
				CatchPriority = playerStats.Catching,
				DistanceToBall = distanceToBall,
				DistanceToTarget = distanceToTarget,
				Player = this
			};
            
			Ball.Instance.AddCatchOption(data);
		}
	}
	
	
	/// <summary>
	/// Use when searching with collisions
	/// </summary>
	/// <param name="area"></param>
	/// <param name="sameTeam"></param>
	/// <param name="prioritizeBall"></param>
	/// <returns></returns>
	public PlayerController GetNearestPlayer(Area3D area, bool sameTeam, bool prioritizeBall = false)
	{
		PlayerController target = null;
		Area3D[] overlapping = area.GetOverlappingAreas().ToArray();
		float minDistance = float.MaxValue;
		for (int i = 0; i < overlapping.Length; i++)
		{
			PlayerController ctlr =  (PlayerController) overlapping[i].GetParent();
			float currentDistance = ctlr.GlobalPosition.DistanceTo(GlobalPosition);
			if((sameTeam && ctlr.isOffence == isOffence) || (!sameTeam && ctlr.isOffence != isOffence))
			{
				if (currentDistance <= minDistance)
				{
					target = ctlr;
					minDistance = currentDistance;
					if (ctlr.HasBall && prioritizeBall) break;
				}
			}
		}

		return target;
	}
	/// <summary>
	/// Use when searching all players for closest one
	/// </summary>
	/// <param name="sameTeam"></param>
	/// <param name="prioritizeBall"></param>
	/// <returns></returns>
	public PlayerController GetNearestPlayer(bool sameTeam, bool prioritizeBall = false, bool lookForReciver = false)
	{
		PlayerController target = null;
		List<Node3D> playersToSearch = sameTeam ? PlayersOnTeam : PlayersNotOnTeam;
		float minDistance = float.MaxValue;
		for (int i = 0; i < playersToSearch.Count; i++)
		{
			PlayerController ctlr =  (PlayerController) playersToSearch[i];
			float currentDistance = ctlr.GlobalPosition.DistanceTo(GlobalPosition);
			
			if (currentDistance <= minDistance)
			{
				if(lookForReciver && !ctlr.playerStats.canBeThrowTarget) continue;
				
				target = ctlr;
				minDistance = currentDistance;
				if (ctlr.HasBall && prioritizeBall) break;
			}
		}

		return target;
	}
	/// <summary>
	/// Use when searching all players for closest one
	/// </summary>
	/// <param name="sameTeam"></param>
	/// <param name="prioritizeBall"></param>
	/// <returns></returns>
	public PlayerController GetNearestPlayerByType(bool sameTeam, PlayerType playerType, bool prioritizeBall = false, PlayerController[] ingnorePlayers = default)
	{
		PlayerController target = null;
		List<Node3D> playersToSearch = sameTeam ? PlayersOnTeam : PlayersNotOnTeam;
		float minDistance = float.MaxValue;

		if (ingnorePlayers != default)
		{
			foreach (var p in ingnorePlayers)
			{
				if(p == null) continue;
				playersToSearch.Remove(p);
			}
		}
		
		for (int i = 0; i < playersToSearch.Count; i++)
		{
			PlayerController ctlr =  (PlayerController) playersToSearch[i];
			if(ctlr.playerStats.PlayerType != playerType) continue;
			
			float currentDistance = ctlr.GlobalPosition.DistanceTo(GlobalPosition);
			
			if (currentDistance <= minDistance)
			{
				target = ctlr;
				minDistance = currentDistance;
				if (ctlr.HasBall && prioritizeBall) break;
			}
		}

		return target;
	}
	
	public PlayerController[] GetNearestPlayersByType(bool sameTeam, PlayerType playerType, bool prioritizeBall = false)
	{
		
		List<Node3D> playersToSearch = sameTeam ? PlayersOnTeam : PlayersNotOnTeam;
		PlayerController[] target = new PlayerController[playersToSearch.Count];

		for (int i = 0; i < target.Length; i++)
		{
			target[i] = GetNearestPlayerByType(sameTeam, playerType, prioritizeBall, target);
		}

		return target;
	}
	public PlayerController GetNearestPlayerToBall(bool sameTeam)
	{
		PlayerController target = null;

		List<Node3D> playersToSearch = sameTeam ? PlayersOnTeam : PlayersNotOnTeam;
		
		float minDistance = float.MaxValue;
		for (int i = 0; i < playersToSearch.Count; i++)
		{
			PlayerController ctlr =  (PlayerController) playersToSearch[i];
			if(ctlr == this) continue;
			float currentDistance = ctlr.GlobalPosition.DistanceTo(ball.GlobalPosition);
			if (currentDistance <= minDistance)
			{
				target = ctlr;
				minDistance = currentDistance;
			}
		}

		return target;
	}

	public void ChangePlayer(PlayerController otherPlayer)
	{
		if (otherPlayer != null && otherPlayer != this)
		{
			otherPlayer.playerID = playerID;
			otherPlayer.inputManager = inputManager;
			otherPlayer.PlayerAction = PlayerAction;
			playerID = -1;
			_moveDirection = Vector3.Zero;
		}
	}
	
	public void DoAction(PlayerActions action, int calledPlayerId)
	{
		if(playerID != calledPlayerId) return;
		
		
		GD.Print("Started: " + action);
		switch (action)
		{
			case PlayerActions.Sprint : 
				Sprint();
				break;
			case PlayerActions.SpinMove :
				SpinMove();
				break;
			case PlayerActions.Jump :
				Jump();
				break;
			case PlayerActions.StiffArm :
				StiffArm();
				break;
			case PlayerActions.Tackle :
				Tackle();
				break;
			case PlayerActions.Dive :
				Dive();
				break;
			case PlayerActions.Throw :
				ThrowBall();
				break;
			case PlayerActions.ChangePlayer :
				ChangePlayer();
				break;
			case PlayerActions.Tackled :
				Tackled();
				break;
		}
		//if(!PlayerAction.Contains(action))
		//	PlayerAction.Add(action);
	}
	public void CancelAction(PlayerActions action, int calledPlayerId)
	{
		if(playerID != calledPlayerId) return;
		
		GD.Print("Stopped: " + action);
		switch (action)
		{
			case PlayerActions.Sprint : 
				Sprint(true);
				break;
		}
	}

	bool CanDoAction(PlayerActions action, PlayerActions[] restrictions)
	{
		
		foreach (PlayerActions restriction in restrictions)
		{
			if (PlayerAction.Contains(restriction))
			{
				if (PlayerAction.Contains(action))
					PlayerAction.Remove(action);
				return false;
			}
		}
		
		
		if(PlayerAction.Contains(action)) return false;
		
		PlayerAction.Add(action);
		return true;
	}
	
	void Sprint(bool stopAction = false)
	{
		if (stopAction)
		{
			_sprintMultiplier = 0;
			return;
		}
		_sprintMultiplier = playerStats.Agility;
		if (PlayerAction.Contains(PlayerActions.Sprint))
			PlayerAction.Remove(PlayerActions.Sprint);
	}
	async void SpinMove()
	{
		PlayerActions[] restrictions =
		{
			PlayerActions.Jump,
			PlayerActions.StiffArm
		};
		if(!CanDoAction(PlayerActions.SpinMove, restrictions)) return;
		
		
		testMat.SetAlbedo((Colors.Red));
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		testMat.SetAlbedo((Colors.White));
		if (PlayerAction.Contains(PlayerActions.SpinMove))
			PlayerAction.Remove(PlayerActions.SpinMove);
	}
	async void Jump()
	{
		PlayerActions[] restrictions =
		{
			PlayerActions.SpinMove,
			PlayerActions.StiffArm,
			PlayerActions.Tackle,
			PlayerActions.Dive
		};
		if(!CanDoAction(PlayerActions.Jump, restrictions)) return;
		
		//if(tackleBox.GetOverlappingAreas().Count > 1) return;
		
		float jumpHeight = 3;
		testMat.SetAlbedo((Colors.Blue));
		canTakeInput = false;
		var tween = CreateTween();
		tween.TweenProperty(GetNode("."), "position:y", jumpHeight,
			30 * GetPhysicsProcessDeltaTime()).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.Chain().TweenProperty(GetNode("."), "position:y", jumpHeight, .05f);
		tween.Chain().TweenProperty(GetNode("."), "position:y", 1,
			20 * GetPhysicsProcessDeltaTime()).SetTrans(Tween.TransitionType.Sine);
		await ToSignal(tween, "finished");
		
		canTakeInput = true;
		testMat.SetAlbedo((Colors.White));
		if (PlayerAction.Contains(PlayerActions.Jump))
			PlayerAction.Remove(PlayerActions.Jump);
	}
	async void StiffArm()
	{
		PlayerActions[] restrictions =
		{
			PlayerActions.Jump,
			PlayerActions.SpinMove
		};
		if(!CanDoAction(PlayerActions.StiffArm, restrictions)) return;
		
		testMat.SetAlbedo((Colors.Orange));
		tackleBox.Monitorable = false;
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		tackleBox.Monitorable = true;
		testMat.SetAlbedo((Colors.White));
		if (PlayerAction.Contains(PlayerActions.StiffArm))
			PlayerAction.Remove(PlayerActions.StiffArm);
	}
	async void Tackle()
	{
		PlayerActions[] restrictions =
		{
			PlayerActions.Jump,
			PlayerActions.Dive
		};
		if(!CanDoAction(PlayerActions.Tackle, restrictions)) return;
		
		testMat.SetAlbedo((Colors.Green));
		PlayerController tackleTarget = GetNearestPlayer(tackleBox, false, true);
		if (tackleTarget != null)
		{
			tackleTarget.DoAction(PlayerActions.Tackled, tackleTarget.playerID);
			GD.Print("Tackled");
		}
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		testMat.SetAlbedo((Colors.White));
		if (PlayerAction.Contains(PlayerActions.Tackle))
			PlayerAction.Remove(PlayerActions.Tackle);
	}
	async void Tackled()
	{
		PlayerActions[] restrictions =
		{
			
		};
		if(!CanDoAction(PlayerActions.Tackled, restrictions)) return;
		
		testMat.SetAlbedo((Colors.Black));
		CanCatch = false;
		CanMove = false;
		
		await ToSignal(GetTree().CreateTimer(1), "timeout");

		CanMove = true;
		CanCatch = true;
		testMat.SetAlbedo((Colors.White));
		if (PlayerAction.Contains(PlayerActions.Tackled))
			PlayerAction.Remove(PlayerActions.Tackled);
	}
	async void Dive()
	{
		PlayerActions[] restrictions =
		{
			PlayerActions.Jump,
			PlayerActions.Tackle
		};
		if(!CanDoAction(PlayerActions.Dive, restrictions)) return;
		
		testMat.SetAlbedo((Colors.Teal));
		float diveHeight = 1.25f;
		canTakeInput = false;
		
		PlayerController tackleTarget = GetNearestPlayer(nearbyPayersBox, false, true);
		Vector3 diveDirection = _moveDirection;
		if(tackleTarget != null)
		{
			diveDirection = _moveDirection.Dot(GlobalPosition.DirectionTo(tackleTarget.GlobalPosition)) > .25f ?
				GlobalPosition.DirectionTo(tackleTarget.GlobalPosition).Normalized() : _moveDirection;
		}

		if (diveDirection == Vector3.Zero)
		{
			tackleTarget = GetNearestPlayer(false);
			diveDirection = GlobalPosition.DirectionTo(tackleTarget.GlobalPosition).Normalized();
		}
		
		_moveDirection = Vector3.Zero;
		
		var tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(GetNode("."), "position:y", diveHeight,
			30 * GetPhysicsProcessDeltaTime()).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(GetNode("."), "position", GlobalPosition + (diveDirection * 3),
			30 * GetPhysicsProcessDeltaTime()).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.Chain().TweenProperty(GetNode("."), "position:y", diveHeight,
			.05f).SetTrans(Tween.TransitionType.Sine);
		tween.Chain().TweenProperty(GetNode("."), "position:y", 1,
			20 * GetPhysicsProcessDeltaTime()).SetTrans(Tween.TransitionType.Sine);
		await ToSignal(tween, "finished");
		
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		canTakeInput = true;
		
		testMat.SetAlbedo((Colors.White));
		if (PlayerAction.Contains(PlayerActions.Dive))
			PlayerAction.Remove(PlayerActions.Dive);
	}
	void ThrowBall()
	{
		PlayerActions[] restrictions =
		{
			PlayerActions.SpinMove,
			PlayerActions.StiffArm
		};
		if(!CanDoAction(PlayerActions.Throw, restrictions))
		{
			GD.Print("Can't meet restrictions");
			return;
		}
		
		
		if(ball.ballState == BallState.Thrown)
		{
			GD.Print("thrown");
			if (PlayerAction.Contains(PlayerActions.Throw))
				PlayerAction.Remove(PlayerActions.Throw);
			return;
		}
		
		testMat.SetAlbedo((Colors.Yellow));
		Vector3 startPoint = ball.GlobalPosition;
		Vector3 endPoint = throwTarget.GlobalPosition;

		// ball.Reparent(ballPathFollow);
		// ball.Position = Vector3.Zero;
		// ball.Rotation = Vector3.Zero;

		
		float distance = startPoint.DistanceTo(endPoint);
		float throwSpeed = playerStats.Agility * (float)GetPhysicsProcessDeltaTime() * 3;// * distance;
		//GD.Print("Speed: " + throwSpeed + " Agility: " + playerStats.Agility + " processTime: " + (float)GetPhysicsProcessDeltaTime());
		float maxThrowDistance = Mathf.Clamp(playerStats.Agility * playerStats.Strength * 5, 0, MAXTHROWDISTANCE);
		
		
		if(throwTarget.aiManager.currentRoute != null)
			endPoint = throwTarget.aiManager.currentRoute.GetThrowToPoint(distance,endPoint, startPoint
				, throwTarget.playerStats.Speed * (float)GetPhysicsProcessDeltaTime(), ref throwSpeed);
		GD.Print(endPoint);
		//GD.Print("Speed: " + throwSpeed * (float)GetPhysicsProcessDeltaTime());

		if (startPoint.DistanceTo(endPoint) > maxThrowDistance)
		{
			float dif = startPoint.DistanceTo(endPoint) - maxThrowDistance;

			Vector3 dir = endPoint.DirectionTo(startPoint);

			dir *= new Vector3(1, 0, 1);
			
			endPoint += (dir * dif);
		}

		((Node)ball).Reparent(GetTree().Root.GetChild(0));
		
		ball.startPoint = startPoint;
		ball.endPoint = endPoint;
		ball.ballSpeed = throwSpeed;// * (float)GetProcessDeltaTime();
		ball.ballState = BallState.Thrown;
		ball.throwingPlayer = this;
		ball.ResetCatchData();

		//if (debugBox == null)
		{
			MeshInstance3D testMesh = new MeshInstance3D();
			testMesh.Mesh = new BoxMesh();
			testMesh.MaterialOverride = new Material();
			testMesh.Position = endPoint;
			testMesh.Scale *= .5f;
			GetParent().AddChild(testMesh);
			//debugBox = testMesh;
		}
		
		//if (PlayerAction.Contains(PlayerActions.Throw))
		//	PlayerAction.Remove(PlayerActions.Throw);
	}
	async void ChangePlayer()
	{
		PlayerActions[] restrictions = Array.Empty<PlayerActions>();
		if(!CanDoAction(PlayerActions.ChangePlayer, restrictions)) return;
		
		testMat.SetAlbedo((Colors.BlanchedAlmond));
		PlayerController otherPlayer = GetNearestPlayerToBall(true);
		ChangePlayer(otherPlayer);
		
		await ToSignal(GetTree().CreateTimer(.25f), "timeout");
		testMat.SetAlbedo((Colors.White));
		if (PlayerAction.Contains(PlayerActions.ChangePlayer))
			PlayerAction.Remove(PlayerActions.ChangePlayer);
	}
}

public enum PlayerActions
{
	Sprint,
	SpinMove,
	StiffArm,
	Jump,
	Tackle,
	Block,
	Dive,
	Catch,
	Throw,
	Kick,
	ChangePlayer,
	Tackled
}
