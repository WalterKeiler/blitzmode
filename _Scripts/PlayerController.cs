using Godot;
using System;

public partial class PlayerController : Node3D
{
	[Export] float speed = 5;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(Input.IsKeyPressed(Key.W))
			Translate(Vector3.Forward * (float)delta * speed);
	}
}
