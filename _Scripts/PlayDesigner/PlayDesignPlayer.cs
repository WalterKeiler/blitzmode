using Godot;
using System;
public partial class PlayDesignPlayer : PlayDesignSelectable
{
    [Export] Material playerMaterial;
    [Export] MeshInstance3D mesh;

    [Export] public PlayerDataOffence playerDataOff;
    [Export] public PlayerDataDefence playerDataDef;
    [Export] public Zone zone;
    
    public int routeIndex = -1;
    public int zoneIndex = -1;
    
    private Material mat;

    private float square;
    private float triangle;
    private float circle;
    
    public override void _Ready()
    {
        base._Ready();
    }

    public void Init()
    {
        mat = (Material)playerMaterial.Duplicate();
        mesh.MaterialOverride = mat;

        if(playerDataOff != null)
        {
            switch (playerDataOff.PlayerType.PlayerType)
            {
                case PlayerType.Quarterback:
                    triangle = 1;
                    circle = 0;
                    square = 0;
                    break;
                case PlayerType.Receiver:
                    triangle = 0;
                    circle = 1;
                    square = 0;
                    break;
                case PlayerType.OLineman:
                    triangle = 0;
                    circle = 0;
                    square = 1;
                    break;
            }
        }

        if (playerDataDef != null)
        {
            switch (playerDataDef.PlayerType.PlayerType)
            {
                case PlayerType.Safety :
                    triangle = 1;
                    circle = 0;
                    square = 0;
                    break;
                case PlayerType.DLineman :
                    triangle = 0;
                    circle = 0;
                    square = 1;
                    break;
            }
        }
        
        ((ShaderMaterial)mat).SetShaderParameter("Square", square);
        ((ShaderMaterial)mat).SetShaderParameter("Triangle", triangle);
        ((ShaderMaterial)mat).SetShaderParameter("Circle", circle);
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public void Selected()
    {
        ((ShaderMaterial)mat).SetShaderParameter("isSelected", true);
    }
    
    public void DeSelected()
    {
        ((ShaderMaterial)mat).SetShaderParameter("isSelected", false);
    }
}
