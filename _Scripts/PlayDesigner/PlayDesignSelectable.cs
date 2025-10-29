using Godot;
using System;

public partial class PlayDesignSelectable : Node3D
{
    private PlayDesignManager pdm; 
    public override void _Ready()
    {
        base._Ready();
        pdm = PlayDesignManager.Instance;
        pdm.selectableObjects.Add(this);
    }
}