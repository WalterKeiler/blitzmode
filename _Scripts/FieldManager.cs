using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class FieldManager : Node
{
	[Export] public int fieldLength = 100;
	[Export] public int fieldWidth = 53;
	[Export] int EndzoneDepth = 30;

	[Export] int LineOfScrimmage = 100;
	[Export] public Play OffencePlay;
	[Export] public Play DefencePlay;
	
	[Export] Material fieldMaterial;
	[Export] MeshInstance3D fieldMesh;
	[Export] SceneTree endzonePrefab;
	
	List<Node3D> offencePlayers;
	List<Node3D> defencePlayers;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PlaneMesh m = (PlaneMesh)fieldMesh.Mesh;
		var mat = m.GetMaterial();
		((ShaderMaterial)mat).SetShaderParameter("yardsLength", fieldLength);
		((ShaderMaterial)mat).SetShaderParameter("yardsWidth", fieldWidth);
		((ShaderMaterial)mat).SetShaderParameter("lineOfScrimmage", 0);
		((ShaderMaterial)mat).SetShaderParameter("firstDownLine", 20);
		m.SetMaterial(mat);// = mat;
		m.Size = new Vector2(fieldLength, fieldWidth + 7);

		fieldMesh.Mesh = m;
		
		PlayerController[] players = GetParent().GetChildren().OfType<PlayerController>().ToArray();
		offencePlayers = new List<Node3D>();
		defencePlayers = new List<Node3D>();
		int o = 0;
		int d = 0;
		for (int i = 0; i < players.Length; i++)
		{
			if (players[i].isOffence)
			{
				GD.Print(players[i].Name);
				offencePlayers.Add(players[i]);
				Vector2 pos = OffencePlay.PlayerDataOffence[o].Position;
				players[i].Position = new Vector3(pos.Y, 1, pos.X);
				o++;
			}
			else if(!players[i].isOffence)
			{
				defencePlayers.Add(players[i]);
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
