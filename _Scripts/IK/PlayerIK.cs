using Godot;
using System;

public partial class PlayerIK : Node3D
{
    [Export, ExportGroup("Targets")] public Node3D targetLegsL;
    [Export] public Node3D targetLegsR;
    [Export, ExportGroup("Walk")] private PathFollow3D walkPathLegsL;
    [Export] private PathFollow3D walkPathLegsR;
    [Export] private PathFollow3D walkPathArmsL;
    [Export] private PathFollow3D walkPathArmsR;
    [Export] private PathFollow3D walkPathBody;
    [Export] private float speed;
    [Export] private float legOffset;

    public override void _Ready()
    {
        base._Ready();
        walkPathLegsL.ProgressRatio = 0;
        walkPathLegsR.ProgressRatio = legOffset;
        walkPathArmsL.ProgressRatio = legOffset;
        walkPathArmsR.ProgressRatio = 0;
        walkPathBody.ProgressRatio = 0;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        float movementStep = (float)delta * speed;

        walkPathLegsL.ProgressRatio += movementStep;
        walkPathLegsR.ProgressRatio += movementStep;
        walkPathArmsL.ProgressRatio += movementStep;
        walkPathArmsR.ProgressRatio += movementStep;
        walkPathBody.ProgressRatio += movementStep;
    }
}
