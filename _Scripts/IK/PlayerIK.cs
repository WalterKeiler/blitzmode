using Godot;
using System;

[Tool]
public partial class PlayerIK : Node3D
{
    
    [ExportToolButton("Set To Neutral")] public Callable SetToNeutralButton => Callable.From(SetToNeutral);
    [ExportToolButton("Set To Run")] public Callable SetToRunButton => Callable.From(SetToRun);
    [ExportToolButton("Set To StiffArm")] public Callable SetToStiffArmButton => Callable.From(SetToStiffArm);
    [ExportToolButton("Set To Throw")] public Callable SetToThrowButton => Callable.From(SetToThrow);
    
    [Export, ExportGroup("Targets")] public Node3D targetLegsL;
    [Export] public Node3D targetLegsR;
    [Export] public Node3D targetStiffArm;
    [Export] public Node3D targetHead;
    
    [Export, ExportGroup("IK Components")] public TwoBoneIK3D BodyIK;
    [Export] public TwoBoneIK3D HeadIK;
    [Export] public LookAtModifier3D HeadLookAt;
    [Export] public LookAtModifier3D ThrowBodyLookAt;
    [Export] public LookAtModifier3D ThrowHandLookAt;
    [Export] public TwoBoneIK3D StiffArmBodyIK;
    [Export] public TwoBoneIK3D StiffArmIK;
    [Export] public TwoBoneIK3D LeftLegIK;
    [Export] public TwoBoneIK3D RightLegIK;
    [Export] public TwoBoneIK3D LeftArmIK;
    [Export] public TwoBoneIK3D RightArmIK;
    [Export] public TwoBoneIK3D BallArmIK;
    [Export] public TwoBoneIK3D LeftArmThrowIK;
    [Export] public TwoBoneIK3D RightArmThrowIK;
    
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
    
    [Export, ExportGroup("Throw")] private PathFollow3D throwPathArms;
    [Export] private float throwSpeed;
    [Export] private bool isThrowable;
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
    
    public void ToggleHoldingBallRun()
    {
        Ball.Instance.GlobalPosition = ballPos.GlobalPosition;
        Ball.Instance.GlobalRotation = ballPos.GlobalRotation;
    }

    public void SetToStiffArm()
    {
        isStiffArming = true;
        StiffArmIK.Active = isStiffArming;
        StiffArmBodyIK.Active = isStiffArming;
        RightArmIK.Active = !isStiffArming;
        
        BallArmIK.Active = true;
    }

    public void SetToNeutral()
    {
        BodyIK.Active = false;
        HeadIK.Active = false;
        HeadLookAt.Active = false;
        LeftLegIK.Active = false;
        RightLegIK.Active = false;
        RightArmIK.Active = false;
        LeftArmIK.Active = false;
        
        isStiffArming = false;
        StiffArmIK.Active = false;
        StiffArmBodyIK.Active = false;

        isThrowable = false;
        ThrowBodyLookAt.Active = false;
        ThrowHandLookAt.Active = false;
        LeftArmThrowIK.Active = false;
        RightArmThrowIK.Active = false;

        BallArmIK.Active = false;
        
        stiffArmTarget = targetStiffArm;
    }

    public void SetToRun()
    {
        BodyIK.Active = true;
        HeadIK.Active = true;
        HeadLookAt.Active = true;
        LeftLegIK.Active = true;
        RightLegIK.Active = true;
        RightArmIK.Active = true;

        if (!isStiffArming)
        {
            isStiffArming = false;
            StiffArmIK.Active = false;
            StiffArmBodyIK.Active = false;
        }

        if (!isThrowable)
        {
            ThrowBodyLookAt.Active = false;
            ThrowHandLookAt.Active = false;
            LeftArmThrowIK.Active = false;
            RightArmThrowIK.Active = false;
            LeftArmIK.Active = true;
        }
        
        walkPathLegsL.ProgressRatio = 0;
        walkPathLegsR.ProgressRatio = legOffset;
        walkPathArmsL.ProgressRatio = legOffset;
        walkPathArmsR.ProgressRatio = 0;
        walkPathBody.ProgressRatio = 0;
    }

    public void SetToThrow()
    {
        isThrowable = true;
        
        LeftArmIK.Active = false;
        
        isStiffArming = false;
        StiffArmIK.Active = false;
        StiffArmBodyIK.Active = false;
        
        ThrowBodyLookAt.Active = true;
        ThrowHandLookAt.Active = true;
        LeftArmThrowIK.Active = true;
        RightArmThrowIK.Active = true;

        BallArmIK.Active = false;

    }
}
