using Godot;
using System;

public partial class CameraController : Node3D
{
	[Export] public Node3D target;
	[Export] float cameraMoveSpeed = 5;

	private bool moveCam;
	
	public override void _EnterTree()
	{
		base._EnterTree();
		PlayManager.InitPlay += Start;
		PlayManager.EndPlay -= Stop;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		PlayManager.InitPlay += Start;
		PlayManager.EndPlay -= Stop;
	}

	void Start()
	{
		moveCam = true;
	}

	void Stop(bool b)
	{
		moveCam = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!moveCam) return;
		GlobalPosition = GlobalPosition.MoveToward(target.GlobalPosition, cameraMoveSpeed * (float)delta);
	}
	
}
