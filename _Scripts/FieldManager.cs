using Godot;
using System;

public partial class FieldManager : Node
{
	[Export] int feildLength = 100;
	[Export] int feildWidth = 53;
	[Export] int EndzoneDepth = 30;

	[Export] SceneTree feildPrefab;
	[Export] SceneTree endzonePrefab;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
