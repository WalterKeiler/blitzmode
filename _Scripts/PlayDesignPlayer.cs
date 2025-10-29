using Godot;
using System;

public partial class PlayDesignPlayer : Node3D
{
    [Export] Material playerMaterial;
    [Export] MeshInstance3D mesh;

    [Export] public PlayerType playerType;
    
    [Export] public Route route;
    [Export] public Zone zone;

    private Material mat;

    private float square;
    private float triangle;
    private float circle;
    
    public override void _Ready()
    {
        base._Ready();
        mat = (Material)playerMaterial.Duplicate();
        mesh.MaterialOverride = mat;

        switch (playerType)
        {
            case PlayerType.Quarterback :
                triangle = 1;
                circle = 0;
                square = 0;
                break;
            case PlayerType.Receiver :
                triangle = 0;
                circle = 1;
                square = 0;
                break;
            case PlayerType.OLineman :
                triangle = 0;
                circle = 0;
                square = 1;
                break;
            case PlayerType.Safety :
                triangle = 0;
                circle = 1;
                square = 0;
                break;
            case PlayerType.DLineman :
                triangle = 0;
                circle = 0;
                square = 1;
                break;
        }
        
        ((ShaderMaterial)mat).SetShaderParameter("Square", square);
        ((ShaderMaterial)mat).SetShaderParameter("Triangle", triangle);
        ((ShaderMaterial)mat).SetShaderParameter("Circle", circle);

    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}
