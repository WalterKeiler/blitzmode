using Godot;
using System;

public partial class PlayDesignPlayer : Node3D
{
    [Export] PlayerType playerType;
    
    [Export] Route route;
    [Export] Zone zone;
    
    public override void _Ready()
    {
        base._Ready();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}
