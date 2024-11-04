using Godot;
using System;

public partial class PlayerController : Node3D
{
	[Export] public PlayerStats playerStats;
	[Export(PropertyHint.Range, "0,1,")] float _PlayersprintAmount = 1;
	[Export] Node3D _mainCam;
	[Export] public bool isplayerControlled;
	[Export] public bool isOffence;
	[Export] public PlayerActions PlayerAction;

	Vector3 _moveDirection;
	float _sprintMultiplier;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GetInput();
		Move(delta);
	}
	
	void GetInput()
	{
		Vector3 zDir = Vector3.Zero;
		Vector3 xDir = Vector3.Zero;
		if (Input.IsKeyPressed(Key.W))
		{
			zDir = _mainCam.GetGlobalBasis().Z.Normalized();
		}
		else if (Input.IsKeyPressed(Key.S))
		{
			zDir = -_mainCam.GetGlobalBasis().Z.Normalized();
		}
		else
		{
			zDir = Vector3.Zero;
		}
		
		if (Input.IsKeyPressed(Key.A))
		{
			xDir = _mainCam.GetGlobalBasis().X.Normalized();
		}
		else if (Input.IsKeyPressed(Key.D))
		{
			xDir = -_mainCam.GetGlobalBasis().X.Normalized();
		}
		else
		{
			xDir = Vector3.Zero;
		}
		
		if (Input.IsKeyPressed(Key.Shift))
		{
			DoAction(PlayerActions.Sprint);
		}
		else
		{
			CancelAction(PlayerActions.Sprint);
		}

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
		switch (action)
		{
			case PlayerActions.Sprint : 
				Sprint();
				break;
		}
	}
	public void CancelAction(PlayerActions action)
	{
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