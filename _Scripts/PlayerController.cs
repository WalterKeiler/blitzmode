using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;

public partial class PlayerController : Node3D
{
	[Export] public int playerID = -1;
	[Export] public PlayerStats playerStats;
	[Export(PropertyHint.Range, "0,1,")] float _PlayersprintAmount = 1;
	[Export] Node3D _mainCam;
	[Export] public InputManager inputManager;
	[Export] public bool isplayerControlled;
	[Export] public bool isOffence;
	[Export] BaseMaterial3D mat;
	[Export] Area3D tackleBox;
	[Export] Area3D nearbyPayersBox;
	[Export] Area3D allPayersBox;
	[Export] Node3D ball;
	[Export] Path3D ballPath;
	[Export] PathFollow3D ballPathFollow;
	[Export] private Node3D[] throwTargets;
	public List<PlayerActions> PlayerAction;
	public bool HasBall;

	bool canTakeInput = true;
	Vector3 _moveDirection;
	float _sprintMultiplier;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print(mat.ResourceName);
		PlayerAction = new List<PlayerActions>();
		InputManager.InputPressAction += DoAction;
		InputManager.InputReleaseAction += CancelAction;
		if (inputManager != null)
		{
			isOffence = inputManager.isOffence;
			playerID = inputManager.PlayerID;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GetInput();
		Move(delta);
		
		if (ball.GetParent() == this && !HasBall) HasBall = true;
		else if (HasBall) HasBall = false;
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

	void Move(double delta)
	{
		_moveDirection.Normalized();
		Translate(_moveDirection * (float)delta * (playerStats.Speed + _sprintMultiplier));
	}

	PlayerController GetNearestPlayer(Area3D area, bool sameTeam, bool prioritizeBall = false)
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
	
	PlayerController GetNearestPlayerToBall(Area3D area, bool sameTeam)
	{
		PlayerController target = null;
		Area3D[] overlapping = area.GetOverlappingAreas().ToArray();
		float minDistance = float.MaxValue;
		for (int i = 0; i < overlapping.Length; i++)
		{
			PlayerController ctlr =  (PlayerController) overlapping[i].GetParent();
			if(ctlr == this) continue;
			float currentDistance = ctlr.GlobalPosition.DistanceTo(ball.GlobalPosition);
			if((sameTeam && ctlr.isOffence == isOffence) || (!sameTeam && ctlr.isOffence != isOffence))
			{
				if (currentDistance <= minDistance)
				{
					target = ctlr;
					minDistance = currentDistance;
				}
			}
		}

		return target;
	}

	void ChangePlayer(PlayerController otherPlayer)
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
		}
		if(!PlayerAction.Contains(action))
			PlayerAction.Add(action);
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
		if(PlayerAction.Contains(PlayerActions.SpinMove)) return;
		mat.SetAlbedo(Colors.Red);
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		mat.SetAlbedo(Colors.White);
		if (PlayerAction.Contains(PlayerActions.SpinMove))
			PlayerAction.Remove(PlayerActions.SpinMove);
	}
	async void Jump()
	{
		if(PlayerAction.Contains(PlayerActions.Jump)) return;
		//if(tackleBox.GetOverlappingAreas().Count > 1) return;
		float jumpHeight = 3;
		mat.SetAlbedo(Colors.Blue);
		canTakeInput = false;
		var tween = CreateTween();
		tween.TweenProperty(GetNode("."), "position:y", jumpHeight,
			30 * GetProcessDeltaTime()).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.Chain().TweenProperty(GetNode("."), "position:y", jumpHeight, .05f);
		tween.Chain().TweenProperty(GetNode("."), "position:y", 1,
			20 * GetProcessDeltaTime()).SetTrans(Tween.TransitionType.Sine);
		await ToSignal(tween, "finished");
		
		canTakeInput = true;
		mat.SetAlbedo(Colors.White);
		if (PlayerAction.Contains(PlayerActions.Jump))
			PlayerAction.Remove(PlayerActions.Jump);
	}
	async void StiffArm()
	{
		if(PlayerAction.Contains(PlayerActions.StiffArm)) return;
		mat.SetAlbedo(Colors.Orange);
		tackleBox.Monitorable = false;
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		tackleBox.Monitorable = true;
		mat.SetAlbedo(Colors.White);
		if (PlayerAction.Contains(PlayerActions.StiffArm))
			PlayerAction.Remove(PlayerActions.StiffArm);
	}
	async void Tackle()
	{
		if(PlayerAction.Contains(PlayerActions.Tackle)) return;
		mat.SetAlbedo(Colors.Green);
		PlayerController tackleTarget = GetNearestPlayer(tackleBox, false, true);
		if (tackleTarget != null)
			GD.Print("Tackled");
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		mat.SetAlbedo(Colors.White);
		if (PlayerAction.Contains(PlayerActions.Tackle))
			PlayerAction.Remove(PlayerActions.Tackle);
	}
	async void Dive()
	{
		if(PlayerAction.Contains(PlayerActions.Dive)) return;
		mat.SetAlbedo(Colors.Teal);
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
			tackleTarget = GetNearestPlayer(allPayersBox, false);
			diveDirection = GlobalPosition.DirectionTo(tackleTarget.GlobalPosition).Normalized();
		}
		
		_moveDirection = Vector3.Zero;
		
		var tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(GetNode("."), "position:y", diveHeight,
			30 * GetProcessDeltaTime()).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(GetNode("."), "position", GlobalPosition + (diveDirection * 3),
			30 * GetProcessDeltaTime()).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.Chain().TweenProperty(GetNode("."), "position:y", diveHeight,
			.05f).SetTrans(Tween.TransitionType.Sine);
		tween.Chain().TweenProperty(GetNode("."), "position:y", 1,
			20 * GetProcessDeltaTime()).SetTrans(Tween.TransitionType.Sine);
		await ToSignal(tween, "finished");
		
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		canTakeInput = true;
		
		mat.SetAlbedo(Colors.White);
		if (PlayerAction.Contains(PlayerActions.Dive))
			PlayerAction.Remove(PlayerActions.Dive);
	}
	async void ThrowBall()
	{
		if(PlayerAction.Contains(PlayerActions.Throw)) return;
		
		mat.SetAlbedo(Colors.Yellow);
		Vector3 startPoint = GlobalPosition;
		Vector3 endPoint = Vector3.Zero;

		ball.Reparent(ballPathFollow);
		ball.Position = Vector3.Zero;
		ball.Rotation = Vector3.Zero;

		float closest = -100000;
		Node3D target = null;
		for (int i = 0; i < throwTargets.Length; i++)
		{
			Vector3 dir = startPoint.DirectionTo(throwTargets[i].GlobalPosition);
			float dot = dir.Dot(_moveDirection);

			if (dot >= closest)
			{
				GD.Print(dot);
				if (dot - closest <= .1f)
				{
					GD.Print("In Line");
					if(startPoint.DistanceTo(throwTargets[i].GlobalPosition) <= startPoint.DistanceTo(endPoint))
					{
						closest = dot;
						endPoint = throwTargets[i].GlobalPosition;
						target = throwTargets[i];
					}
				}
				else
				{
					closest = dot;
					endPoint = throwTargets[i].GlobalPosition;
					target = throwTargets[i];
				}
			}
		}
		
		Vector3 midPoint = endPoint.Lerp(startPoint, .5f);

		float distance = startPoint.DistanceTo(endPoint);
		midPoint.Y = Mathf.Clamp(.1f * distance, midPoint.Y, 10);
		ballPath.Curve.ClearPoints();
		ballPathFollow.ProgressRatio = 0;
		
		ballPath.Curve.AddPoint(startPoint);
		ballPath.Curve.AddPoint(midPoint);
		Vector3 inDir = startPoint.DirectionTo(endPoint);
		
		ballPath.Curve.SetPointIn(1,  -inDir * distance / 4);
		ballPath.Curve.SetPointOut(1, inDir * distance / 4);
		ballPath.Curve.SetPointTilt(1,distance);
		ballPath.Curve.AddPoint(endPoint);
		ballPath.Curve.SetPointTilt(2,distance * 2 - 1.25f);
		
		GD.Print(ballPathFollow.ProgressRatio);
		
		var tween = CreateTween();
		tween.TweenProperty(ballPathFollow, "progress_ratio", 1,
			 distance * GetProcessDeltaTime() * playerStats.Agility).SetTrans(Tween.TransitionType.Linear);
		await ToSignal(tween, "finished");
		PlayerController otherPlayer = target as PlayerController;
		ChangePlayer(otherPlayer);
		
		ballPathFollow.ProgressRatio = 0;
		
		ball.Reparent(target);
		
		GD.Print("Tween finished.");
		mat.SetAlbedo(Colors.White);
		if (PlayerAction.Contains(PlayerActions.Throw))
			PlayerAction.Remove(PlayerActions.Throw);
	}
	async void ChangePlayer()
	{
		if(PlayerAction.Contains(PlayerActions.ChangePlayer)) return;
		mat.SetAlbedo(Colors.BlanchedAlmond);
		PlayerController otherPlayer = GetNearestPlayerToBall(allPayersBox, true);
		ChangePlayer(otherPlayer);
		
		await ToSignal(GetTree().CreateTimer(.25f), "timeout");
		mat.SetAlbedo(Colors.White);
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
	ChangePlayer
}
