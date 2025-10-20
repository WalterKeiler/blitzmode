using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class FieldManager : Node
{
	public static FieldManager Instance { get; private set; }

	[Export] Material fieldMaterial;
	[Export] MeshInstance3D fieldMesh;
	[Export] SceneTree endzonePrefab;
	
	int fieldLength = 100;
	int fieldWidth = 53;
	int EndzoneDepth = 30;

	private GameManager gm;
	
	public override void _EnterTree()
	{
		base._EnterTree();
		Instance = this;
	}
	
	public override void _Ready()
	{
		gm = GameManager.Instance;

		fieldLength = gm.fieldLength;
		fieldWidth = gm.fieldWidth;
		EndzoneDepth = gm.EndzoneDepth;
		
		PlaneMesh m = (PlaneMesh)fieldMesh.Mesh;
		var mat = m.GetMaterial();
		((ShaderMaterial)mat).SetShaderParameter("yardsLength", fieldLength);
		((ShaderMaterial)mat).SetShaderParameter("yardsWidth", fieldWidth);
		((ShaderMaterial)mat).SetShaderParameter("lineOfScrimmage", 0);
		((ShaderMaterial)mat).SetShaderParameter("firstDownLine", 20);
		m.SetMaterial(mat);// = mat;
		m.Size = new Vector2(fieldLength, fieldWidth + 7);
		
		fieldMesh.Mesh = m;
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void SetFieldLines(float lineOfScrimmage)
	{
		PlaneMesh m = (PlaneMesh)fieldMesh.Mesh;
		var mat = m.GetMaterial();
		((ShaderMaterial)mat).SetShaderParameter("lineOfScrimmage", lineOfScrimmage);
		((ShaderMaterial)mat).SetShaderParameter("firstDownLine", lineOfScrimmage + gm.yardsToFirstDown);
		m.SetMaterial(mat);
	}
}
