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
	public const int PATHFINDING_STEPS = 32;
	
	
	[Export] public int playerID = -1;
	[Export] public int inputID = -1;
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
	[Export] public bool HasBall;
	public bool CanCatch;
	public bool CanAct;
	[Export] public bool CanThrow;
	[Export] public bool CanMove;
	public bool IsBlocking;
	public bool CanBlock;
	public bool IsBlocked;
	public bool IsTargeted;
	public Vector3 _moveDirection;
	public float blockStamina;
	public float movementMultiplier = 1;
	public bool IsTeam1;

	private Vector3 lastFrameMoveDir;
	
	Area3D tackleBox;
	Area3D nearbyPayersBox;
	Ball ball;

	TeamData teamStats;
	
	List<PlayerController> PlayersOnTeam;
	List<PlayerController> PlayersNotOnTeam;

	[Export] bool init = false;
	bool snap = false;
	bool canTakeInput = true;
	float _sprintMultiplier;
	float switchTargetTimer;
	float blockCooldown;
	PlayerController throwTarget;

	public Color StartColor;
	
	StandardMaterial3D testMat;
	GameManager gm;
	PathfindingManager pm;
	Node3D debugBox;
	Node3D debugBox2;

	public static event Action<bool> CrossedLOS;
	public static event Action<bool> Snapped;
	
	public override void _Ready()
	{
		base._Ready();
		
		gm = GameManager.Instance;
		pm = PathfindingManager.Instance;
		
		//Init();
		CanMove = false;
		tackleBox = GetNode<Area3D>("TackleBox");
		nearbyPayersBox = GetNode<Area3D>("NearbyPayersBox");
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		InputManager.InputPressAction += DoAction;
		InputManager.InputReleaseAction += CancelAction;
		Ball.BallCaught += BallOnBallCaught;
		CrossedLOS += BallOnBallCaught;
		PlayManager.EndPlay += OnPlayerWithBallTackled;
		Snapped += Init;
		PlayManager.InitPlay += InitSnap;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		InputManager.InputPressAction -= DoAction;
		InputManager.InputReleaseAction -= CancelAction;
		Ball.BallCaught -= BallOnBallCaught;
		CrossedLOS -= BallOnBallCaught;
		PlayManager.EndPlay -= OnPlayerWithBallTackled;
		Snapped -= Init;
		PlayManager.InitPlay -= InitSnap;
	}
	
	// Called when the node enters the scene tree for the first time.
	public void Init(bool isSpecialTeams)
	{
		//snap = false;
		//GD.Print(mat.ResourceName);
		PlayerAction = new List<PlayerActions>();
		// if (inputManager != null)
		// {
		// 	inputID = inputManager.PlayerID;
		// }
		
		PlayersOnTeam = new List<PlayerController>();
		PlayersNotOnTeam = new List<PlayerController>();
		_moveDirection = Vector3.Zero;
		CanCatch = true;
		CanAct = true;
		CanMove = true;
		CanThrow = true;
		IsTargeted = false;
		IsBlocked = false;
		IsBlocking = false;
		switchTargetTimer = 0;
		testMat = (StandardMaterial3D)mat.Duplicate();
		mesh.MaterialOverride = testMat;
		lastFrameMoveDir = Vector3.Zero;
		
		
		if (playerStats.PlayerType == PlayerType.OLineman) snap = true;
		
		teamStats = IsTeam1 ? gm.team1 : gm.team2;
		
		ball = Ball.Instance;
		
		if (!isOffence) HasBall = false;
		
		blockStamina = playerStats.Strength + teamStats.Linemen;
		blockCooldown = 0;
		IsBlocking = false;
		
		testMat.SetAlbedo(StartColor);

		if (isOffence)
		{
			PlayersOnTeam = gm.offencePlayers;
			PlayersNotOnTeam = gm.defencePlayers;
		}
		else
		{
			PlayersOnTeam = gm.defencePlayers;
			PlayersNotOnTeam = gm.offencePlayers;
		}
		init = true;
		//if (PlayManager.Instance.isKickoff) CanMove = false;
		aiManager.Init();
	}

	void InitSnap(bool isSpecialTeams)
	{
		if (!HasBall) return;

		if (isSpecialTeams)
		{
			if (HasBall)
			{
				ball = Ball.Instance;
				((Node)ball).Reparent(this, false);
				ball.Position = new Vector3(.5f, -.25f, 0) * PlayManager.Instance.PlayDirection;
				snap = true;
				ball.ballSpeed = .25f;
				ball.ballState = BallState.Thrown;
				ball.throwingPlayer = this;
				ball.ResetCatchData();
				snap = false;
				Snapped?.Invoke(true);
				GD.Print("Kickoff");
			}
			return;
		}
		
		if (!isOffence) HasBall = false;
		
		ball = Ball.Instance;
		ball.Freeze = true;
		//GD.Print("Ball: "  + ball.GetParent().Name);
		ball.Position = new Vector3(-.5f, -.25f, 0) * PlayManager.Instance.PlayDirection;
		
		((Node)ball).Reparent(this, false);

		snap = true;
	}

	void SnapBall()
	{
		((Node)ball).Reparent(GetTree().Root.GetChild(0));
		HasBall = false;
		ball.throwingPlayer = this;
		Snapped?.Invoke(false);
	}
	
	private void BallOnBallCaught(bool caughtByOffence)
	{
		if (caughtByOffence)
		{
			if(isOffence)
			{
				aiManager.followRoute = 0;
				aiManager.findOpenSpace = 0;
				aiManager.block = 1;
			}
			else
			{
				aiManager.coverZone = 0;
				aiManager.followPlayer = 0;
				aiManager.rushBall = 1;
			}
		}

		//if (PlayManager.Instance.isKickoff) CanMove = true;
	}

	void OnPlayerWithBallTackled(bool incompletePass)
	{
		if(snap) return;
		init = false;
		CanAct = false;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!init) return;

		if (snap && ball.ballState == BallState.Held) snap = false;
		
		GetInput();
		
		if(CanMove)
			Move(delta);
		
		if (CanThrow && ball.GetParent() == this)// && !HasBall)
		{
			if (PlayManager.Instance.PlayDirection > 0)
			{
				if(GlobalPosition.X > PlayManager.Instance.lineOfScrimmage)
				{ 
					CrossedLOS?.Invoke(true);
					CanThrow = false;
				}
			}
			else
			{
				if(GlobalPosition.X < PlayManager.Instance.lineOfScrimmage)
				{ 
					CrossedLOS?.Invoke(true);
					CanThrow = false;
				}
			}
			
			if(switchTargetTimer > 0)
				switchTargetTimer -= (float)delta;
			else
			{
				SelectThrowTarget();
			}
			HasBall = true;
		}
		//else if (HasBall) HasBall = false;

		if (Ball.Instance.ballState == BallState.Thrown && CanCatch && !PlayerAction.Contains(PlayerActions.Throw))
		{
			CheckForCatch();
		}
		
		if (ball.ballState is BallState.Free or BallState.Fumbled && !snap)
		{
			float dist = ball.GlobalPosition.DistanceTo(GlobalPosition);

			if (dist <= 1.25f)
			{
				ball.Caught(this);
			}
		}
		
		if(IsBlocking)
		{
			blockStamina -= (float)delta;
		}
		else if(blockStamina < (playerStats.Strength + teamStats.Linemen) && CanBlock)
		{
			blockStamina += (float)delta;
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
			
			MeshInstance3D testMesh2 = new MeshInstance3D();
			testMesh2.Mesh = new BoxMesh();
			testMesh2.MaterialOverride = new Material();
			testMesh2.Position = Vector3.Zero;
			testMesh2.Scale *= .5f;
			GetParent().AddChild(testMesh2);
			debugBox2 = testMesh2;
		}
		
		float closest = -100000;
		PlayerController target = null;
		Vector3 endPoint = Vector3.Zero;
		for (int i = 0; i < PlayersOnTeam.Count; i++)
		{
			if(PlayersOnTeam[i] == this) continue;
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
			if(throwTarget.aiManager.currentRoute != null)
			{
				float distance = GlobalPosition.DistanceTo(throwTarget.GlobalPosition);
				float throwSpeed = (playerStats.Agility + teamStats.Passing) * (float)GetPhysicsProcessDeltaTime() * 3;
				Vector3 ep = throwTarget.aiManager.currentRoute.GetThrowToPoint(distance, throwTarget.GlobalPosition, GlobalPosition
					, (throwTarget.playerStats.Speed + throwTarget.teamStats.Running) * (float) GetPhysicsProcessDeltaTime(),
					ref throwSpeed);
				debugBox2.GlobalPosition = ep + Vector3.Up * 2;
			}
			
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
		if (inputID == -1 || !canTakeInput) return;
		
		Vector3 inputDir = inputManager.GetDirectionalInput();
		Vector3 xDir = Vector3.Zero;
		Vector3 zDir = Vector3.Zero;
		
		zDir = inputDir.Z * _mainCam.GetGlobalBasis().X.Normalized();
		xDir = -inputDir.X * _mainCam.GetGlobalBasis().Z.Normalized();
		_moveDirection = xDir + zDir;

		if(_moveDirection.Length() > .1f)
			_moveDirection = QuerySDF(GlobalPosition + _moveDirection).Normalized();
		
		_moveDirection.Y = 0;
	}

	public void GetInput(Vector3 direction)
	{
		if (inputID != -1 || !canTakeInput) return;
		_moveDirection = direction.Normalized();
		
		_moveDirection.Y = 0;
	}

	void Move(double delta)
	{
		_moveDirection.Normalized();

		_moveDirection = _moveDirection.Lerp(lastFrameMoveDir, .75f);
		_moveDirection = _moveDirection.LimitLength();
		
		Translate(_moveDirection * (float)delta * ((playerStats.Speed + teamStats.Running) + _sprintMultiplier) * movementMultiplier);
		lastFrameMoveDir = _moveDirection;
	}
	
	
	void CheckForCatch()
	{
		if (HasBall || snap) return;
		float distanceToBall = ball.GlobalPosition.DistanceTo(GlobalPosition);
		float distanceToTarget = ball.endPoint.DistanceTo(GlobalPosition);
		float dot = GetGlobalBasis().Z.Dot(ball.GlobalPosition.DirectionTo(GlobalPosition));

		float catchRadius = 3;

		if (distanceToBall <= catchRadius)
		{
			BallCatchData data = new BallCatchData
			{
				BallDot = dot,
				CatchPriority = playerStats.Catching + teamStats.Passing,
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
				if (ctlr.HasBall && prioritizeBall)
				{
					target = ctlr;
					break;
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
	public PlayerController GetNearestPlayer(bool sameTeam, bool prioritizeBall = false, bool lookForReciver = false, PlayerController[] ingnorePlayers = default)
	{
		PlayerController target = null;
		List<PlayerController> playersToSearch = new List<PlayerController>(sameTeam ? PlayersOnTeam : PlayersNotOnTeam);
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
		List<PlayerController> playersToSearch = new List<PlayerController>(sameTeam ? PlayersOnTeam : PlayersNotOnTeam);
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
		
		List<PlayerController> playersToSearch = sameTeam ? PlayersOnTeam : PlayersNotOnTeam;
		PlayerController[] target = new PlayerController[playersToSearch.Count];

		for (int i = 0; i < target.Length; i++)
		{
			target[i] = GetNearestPlayerByType(sameTeam, playerType, prioritizeBall, target);
		}

		return target;
	}
	public PlayerController[] GetNearestPlayers(bool sameTeam, bool prioritizeBall = false)
	{
		
		List<PlayerController> playersToSearch = sameTeam ? PlayersOnTeam : PlayersNotOnTeam;
		PlayerController[] target = new PlayerController[playersToSearch.Count];

		for (int i = 0; i < target.Length; i++)
		{
			target[i] = GetNearestPlayer(sameTeam, prioritizeBall, false, target);
			//GD.Print("Found: " + target[i].Name);

		}

		return target;
	}
	public PlayerController GetNearestPlayerToBall(bool sameTeam)
	{
		PlayerController target = null;

		List<PlayerController> playersToSearch = sameTeam ? PlayersOnTeam : PlayersNotOnTeam;
		
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
		if (otherPlayer != null && otherPlayer != this && otherPlayer.inputManager == null)
		{
			int id = inputID;
			InputManager im = inputManager;
			List<PlayerActions> pa = PlayerAction;
			
			inputID = -1;
			_moveDirection = Vector3.Zero;
			inputManager = null;
			PlayerAction = new List<PlayerActions>();
			otherPlayer.inputID = id;
			otherPlayer.inputManager = im;
			otherPlayer.PlayerAction = pa;
		}

		if (inputManager == null && inputID != -1) inputManager = gm.GetInputByPlayerID(inputID);
	}
	
	public void DoAction(PlayerActions action, int calledPlayerId, bool forceAction = false, bool playerAction = false, bool isSecondaryAction = false)
	{
		if (snap)
		{
			InputManager[] inputs = IsTeam1 ? gm.playerInputTeam1 : gm.playerInputTeam2;
			bool c = false;

			foreach (var im in inputs)
			{
				if (im.PlayerID == calledPlayerId)
				{
					c = true;
					break;
				}
			}

			if (c && action == PlayerActions.Throw)
			{
				SnapBall();
			}
		}
		
		if(!CanAct)return;
		
		if(!forceAction)
		{
			if (!playerAction)
			{
				if (playerID != calledPlayerId) return;
			}
			else
			{
				if (inputID != calledPlayerId) return;
			}
		}
		
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
				if(isSecondaryAction && CanThrow) break;
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
	public void CancelAction(PlayerActions action, int calledPlayerId, bool forceAction, bool playerAction)
	{
		if(inputID == -1)
			if(playerID != calledPlayerId) return;
		else
			if(inputID != calledPlayerId) return;
		
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

	public void Block()
	{
		IsBlocking = true;
		CanBlock = false;
		PlayerController nearestPlayer = GetNearestPlayer(false);
		Vector3 dir = GlobalPosition.DirectionTo(nearestPlayer.GlobalPosition);
		//GD.Print(nearestPlayer._moveDirection.Dot(dir) < 0.75f);
		if (nearestPlayer._moveDirection.Dot(dir) < 0.95f && ((nearestPlayer.playerStats.Strength + nearestPlayer.teamStats.Linemen) / 2) < blockStamina)
		{
			nearestPlayer._moveDirection = Vector3.Zero;
			nearestPlayer.IsBlocked = true;
		}
		else
		{
			nearestPlayer.IsBlocked = false;
			BlockCoolDown();
			IsBlocking = false;
		}
	}

	async void BlockCoolDown()
	{
		movementMultiplier = .5f;
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		CanBlock = true;
		if (!isPlayerControlled && aiManager.targetPlayer != null)
		{
			aiManager.targetPlayer.IsTargeted = false;
			aiManager.targetPlayer = null;
		}
		blockStamina = playerStats.Strength + teamStats.Linemen;
		movementMultiplier = 1;
	}
	
	void Sprint(bool stopAction = false)
	{
		if (stopAction)
		{
			_sprintMultiplier = 0;
			return;
		}
		_sprintMultiplier = playerStats.Agility + teamStats.Running;
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
		testMat.SetAlbedo(StartColor);
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
		testMat.SetAlbedo(StartColor);
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
		testMat.SetAlbedo(StartColor);
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
		//PlayerController tackleTarget = GetNearestPlayer(tackleBox, false, true);
		PlayerController tackleTarget = GetNearestPlayer(false, true);
		if (tackleTarget != null)
		{
			tackleTarget.DoAction(PlayerActions.Tackled, tackleTarget.inputID, true);
			GD.Print("Tackled");
		}
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		testMat.SetAlbedo(StartColor);
		if (PlayerAction.Contains(PlayerActions.Tackle))
			PlayerAction.Remove(PlayerActions.Tackle);
	}
	async void Tackled()
	{
		PlayerActions[] restrictions =
		{
			PlayerActions.Tackled
		};
		if(!CanDoAction(PlayerActions.Tackled, restrictions)) return;
		
		testMat.SetAlbedo((Colors.Black));
		CanCatch = false;
		CanMove = false;
		
		if(HasBall && ball.ballState == BallState.Held && !PlayManager.Instance.inbetweenPlays)
		{
			PlayManager.Instance.InvokeEndPlay(true);
			return;
		}
		
		await ToSignal(GetTree().CreateTimer(1), "timeout");

		CanMove = true;
		CanCatch = true;
		testMat.SetAlbedo(StartColor);
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
		
		tackleTarget = GetNearestPlayer(tackleBox, false, true);
		if (tackleTarget != null)
		{
			tackleTarget.DoAction(PlayerActions.Tackled, tackleTarget.playerID, true);
			GD.Print("Tackled");
		}
		
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		canTakeInput = true;
		
		testMat.SetAlbedo(StartColor);
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

		if (!CanThrow)
		{
			if (PlayerAction.Contains(PlayerActions.Throw))
				PlayerAction.Remove(PlayerActions.Throw);
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
		float throwSpeed = (playerStats.Agility + teamStats.Passing) * (float)GetPhysicsProcessDeltaTime() * 3;// * distance;
		//GD.Print("Speed: " + throwSpeed + " Agility: " + playerStats.Agility + " processTime: " + (float)GetPhysicsProcessDeltaTime());
		float maxThrowDistance = Mathf.Clamp((playerStats.Agility + teamStats.Passing) * (playerStats.Strength + teamStats.Passing) * 5, 0, MAXTHROWDISTANCE);
		
		
		if(throwTarget.aiManager.currentRoute != null)
			endPoint = throwTarget.aiManager.currentRoute.GetThrowToPoint(distance,endPoint, startPoint
				, (throwTarget.playerStats.Speed + throwTarget.teamStats.Running) * (float)GetPhysicsProcessDeltaTime(), ref throwSpeed);
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
		GD.Print(ball.ballState);
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
		testMat.SetAlbedo(StartColor);
		if (PlayerAction.Contains(PlayerActions.ChangePlayer))
			PlayerAction.Remove(PlayerActions.ChangePlayer);
	}
	
	Vector3 QuerySDF(Vector3 target)
	{
		float minWeight = float.MaxValue;
		Vector3 finalDir = Vector3.Zero;
		for (int i = 0; i < PATHFINDING_STEPS; i++)
		{
			Vector3 unitDir = MathW.PointOnUnitCircleXZ(i,i,PATHFINDING_STEPS);
            
			float testWeight = pm.QuerySDF(GlobalPosition + unitDir * .1f, target, this);
            
			if (minWeight > testWeight)
			{
				minWeight = testWeight;
				finalDir = unitDir;
			}
		}

		return finalDir;
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
