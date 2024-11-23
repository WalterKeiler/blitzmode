using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerController : Node3D
{
	[Export] public PlayerStats playerStats;
	[Export(PropertyHint.Range, "0,1,")] float _PlayersprintAmount = 1;
	[Export] Node3D _mainCam;
	[Export] InputManager _inputManager;
	[Export] public bool isplayerControlled;
	[Export] public bool isOffence;
	[Export] BaseMaterial3D mat;
	[Export] Node3D ball;
	[Export] Path3D ballPath;
	[Export] PathFollow3D ballPathFollow;
	[Export] private Node3D[] throwTargets;
	public List<PlayerActions> PlayerAction;

	Vector3 _moveDirection;
	float _sprintMultiplier;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print(mat.ResourceName);
		PlayerAction = new List<PlayerActions>();
		_inputManager.InputPressAction += DoAction;
		_inputManager.InputReleaseAction += CancelAction;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GetInput();
		Move(delta);
	}
	
	void GetInput()
	{

		Vector3 inputDir = _inputManager.GetDirectionalInput();
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

	public void DoAction(PlayerActions action)
	{
		GD.Print("Started: " + action);
		if(!PlayerAction.Contains(action))
			PlayerAction.Add(action);
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
	}
	public void CancelAction(PlayerActions action)
	{
		GD.Print("Stopped: " + action);
		if (PlayerAction.Contains(action))
			PlayerAction.Remove(action);
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
	}
	async void SpinMove()
	{
		mat.SetAlbedo(Colors.Red);
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		mat.SetAlbedo(Colors.White);
	}
	async void Jump()
	{
		mat.SetAlbedo(Colors.Blue);
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		mat.SetAlbedo(Colors.White);
	}
	async void StiffArm()
	{
		mat.SetAlbedo(Colors.Orange);
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		mat.SetAlbedo(Colors.White);
	}
	async void Tackle()
	{
		mat.SetAlbedo(Colors.Green);
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		mat.SetAlbedo(Colors.White);
	}
	async void Dive()
	{
		mat.SetAlbedo(Colors.Teal);
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		mat.SetAlbedo(Colors.White);
	}
	async void ThrowBall()
	{
		mat.SetAlbedo(Colors.Yellow);
		Vector3 startPoint = Transform.Origin;
		Vector3 endPoint = Vector3.Zero;

		ball.Reparent(ballPathFollow);
		ball.Position = Vector3.Zero;
		ball.Rotation = Vector3.Zero;

		float closest = -100000;
		Node3D target = null;
		for (int i = 0; i < throwTargets.Length; i++)
		{
			Vector3 dir = startPoint.DirectionTo(throwTargets[i].Position);
			float dot = dir.Dot(_moveDirection);

			if (dot >= closest)
			{
				GD.Print(dot);
				if (dot - closest <= .1f)
				{
					GD.Print("In Line");
					if(startPoint.DistanceTo(throwTargets[i].Position) <= startPoint.DistanceTo(endPoint))
					{
						closest = dot;
						endPoint = throwTargets[i].Position;
						target = throwTargets[i];
					}
				}
				else
				{
					closest = dot;
					endPoint = throwTargets[i].Position;
					target = throwTargets[i];
				}
			}
		}
		Vector3 midPoint = endPoint.Lerp(startPoint, .5f);

		float distance = startPoint.DistanceTo(endPoint);
		midPoint.Y = .1f * distance;
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
		ballPathFollow.ProgressRatio = 0;
		
		ball.Reparent(target);
		// var yTween = CreateTween();
		// yTween.TweenProperty(ball, "position:y", 1,
		// 	distance * GetProcessDeltaTime() * playerStats.Agility).SetTrans(Tween.TransitionType.Linear);
		// await ToSignal(yTween, "finished");
		GD.Print("Tween finished.");
		mat.SetAlbedo(Colors.White);
	}
	async void ChangePlayer()
	{
		mat.SetAlbedo(Colors.BlanchedAlmond);
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		mat.SetAlbedo(Colors.White);
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
