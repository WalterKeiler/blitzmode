using Godot;
using System;

public partial class CameraController : Node3D
{
	[Export] Node3D target;
	[Export] float cameraMoveSpeed = 5;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GlobalPosition = GlobalPosition.MoveToward(target.GlobalPosition, cameraMoveSpeed * (float)delta);
	}
}
