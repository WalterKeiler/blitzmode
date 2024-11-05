using Godot;
using System;

public partial class InputManager : Node
{
	[Export] public int PlayerID;
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public Vector3 GetDirectionalInput()
	{
		Vector3 inputDir = new Vector3();
		if (Input.IsKeyPressed(Key.W))
		{
			inputDir.Z = 1;
		}
		else if (Input.IsKeyPressed(Key.S))
		{
			inputDir.Z = -1;
		}
		else
		{
			inputDir.Z = 0;
		}
		
		if (Input.IsKeyPressed(Key.A))
		{
			inputDir.X = 1;
		}
		else if (Input.IsKeyPressed(Key.D))
		{
			inputDir.X = -1;
		}
		else
		{
			inputDir.X = 0;
		}

		return inputDir;
	}
}
