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
		
		zDir = inputDir.Z * _mainCam.GetGlobalBasis().Z.Normalized();
		xDir = inputDir.X * _mainCam.GetGlobalBasis().X.Normalized();
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
		await ToSignal(GetTree().CreateTimer(1), "timeout");
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
