using Godot;
using System;

public partial class PlayerController : Node3D
{
	[Export] float speed = 5;

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
		if (Input.IsKeyPressed(Key.W))
		{
			moveDirection.Z = 1;
		}
		else if (Input.IsKeyPressed(Key.S))
		{
			moveDirection.Z = -1;
		}
		else
		{
			moveDirection.Z = 0;
		}
		if (Input.IsKeyPressed(Key.A))
		{
			moveDirection.X = 1;
		}
		else if (Input.IsKeyPressed(Key.D))
		{
			moveDirection.X = -1;
		}
		else
		{
			moveDirection.X = 0;
		}

		moveDirection.Normalized();
	}

	void Move(double delta)
	{
		Translate(moveDirection * (float)delta * speed);
	}
}
