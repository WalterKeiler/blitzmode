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
	public List<PlayerActions> PlayerAction;

	Vector3 _moveDirection;
	float _sprintMultiplier;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
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
		
		// if (Input.IsKeyPressed(Key.Shift))
		// {
		// 	DoAction(PlayerActions.Sprint);
		// }
		// else
		// {
		// 	CancelAction(PlayerActions.Sprint);
		// }
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
	Kick
}
