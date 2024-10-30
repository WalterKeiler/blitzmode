using Godot;
using System;

public partial class PlayerController : Node3D
{
	[Export] float speed = 5;
	[Export] Node3D mainCam;

	Vector3 moveDirection;
	
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
			zDir = mainCam.GetGlobalBasis().Z.Normalized();
		}
		else if (Input.IsKeyPressed(Key.S))
		{
			zDir = -mainCam.GetGlobalBasis().Z.Normalized();
		}
		else
		{
			zDir = Vector3.Zero;
		}
		
		if (Input.IsKeyPressed(Key.A))
		{
			xDir = mainCam.GetGlobalBasis().X.Normalized();
		}
		else if (Input.IsKeyPressed(Key.D))
		{
			xDir = -mainCam.GetGlobalBasis().X.Normalized();
		}
		else
		{
			xDir = Vector3.Zero;
		}

		moveDirection = xDir + zDir;
		
		moveDirection.Y = 0;
		moveDirection.Normalized();
		GD.Print(moveDirection);
	}

	void Move(double delta)
	{
		Translate(moveDirection * (float)delta * speed);
	}
}
