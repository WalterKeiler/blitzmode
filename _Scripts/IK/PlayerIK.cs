using Godot;
using System;

public partial class PlayerIK : Node3D
{
    [Export, ExportGroup("Targets")] public Node3D targetLegsL;
    [Export] public Node3D targetLegsR;
    [Export] public Node3D targetStiffArm;
    [Export] public Node3D targetHead;
    
    [Export, ExportGroup("IK Components")] public TwoBoneIK3D BodyIK;
    [Export] public TwoBoneIK3D HeadIK;
    [Export] public LookAtModifier3D HeadLookAt;
    [Export] public TwoBoneIK3D StiffArmBodyIK;
    [Export] public TwoBoneIK3D StiffArmIK;
    [Export] public TwoBoneIK3D LeftLegIK;
    [Export] public TwoBoneIK3D RightLegIK;
    [Export] public TwoBoneIK3D LeftArmIK;
    [Export] public TwoBoneIK3D RightArmIK;
    [Export] public TwoBoneIK3D BallArmIK;
    
    [Export, ExportGroup("Walk")] private PathFollow3D walkPathLegsL;
    [Export] private PathFollow3D walkPathLegsR;
    [Export] private PathFollow3D walkPathArmsL;
    [Export] private PathFollow3D walkPathArmsR;
    [Export] private PathFollow3D walkPathBody;
    [Export] private float speed;
    [Export] private float legOffset;

    [Export, ExportGroup("StiffArm")] private bool isStiffArming;
    private Node3D stiffArmTarget;

    [Export, ExportGroup("HoldingBall")] private Node3D ballPos;
    
    public override void _Ready()
    {
        base._Ready();
        walkPathLegsL.ProgressRatio = 0;
        walkPathLegsR.ProgressRatio = legOffset;
        walkPathArmsL.ProgressRatio = legOffset;
        walkPathArmsR.ProgressRatio = 0;
        walkPathBody.ProgressRatio = 0;
        
        isStiffArming = false;
        StiffArmIK.Active = false;
        StiffArmBodyIK.Active = false;
        stiffArmTarget = targetStiffArm;
        
        //ToggleStiffArm(targetStiffArm);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        float movementStep = (float)delta * speed;

        walkPathLegsL.ProgressRatio += movementStep;
        walkPathLegsR.ProgressRatio += movementStep;
        walkPathArmsL.ProgressRatio += movementStep;
        walkPathArmsR.ProgressRatio += movementStep;
        walkPathBody.ProgressRatio += movementStep * 2;
        
        if (isStiffArming)
        {
            targetStiffArm.GlobalPosition = stiffArmTarget.GlobalPosition;
        }
    }

    public void ToggleStiffArm(Node3D target)
    {
        isStiffArming = !isStiffArming;
        StiffArmIK.Active = isStiffArming;
        StiffArmBodyIK.Active = isStiffArming;
        RightArmIK.Active = !isStiffArming;
    }

    public void ToggleHoldingBallRun()
    {
        Ball.Instance.GlobalPosition = ballPos.GlobalPosition;
        Ball.Instance.GlobalRotation = ballPos.GlobalRotation;
    }
}
