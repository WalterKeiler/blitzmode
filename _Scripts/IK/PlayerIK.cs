using Godot;
using System;

[Tool]
public partial class PlayerIK : Node3D
{
    [ExportToolButton("Set To Neutral")] public Callable SetToNeutralButton => Callable.From(SetToNeutral);
    [ExportToolButton("Set To Run")] public Callable SetToRunButton => Callable.From(SetToRun);
    [ExportToolButton("Set To StiffArm")] public Callable SetToStiffArmButton => Callable.From(SetToStiffArm);
    [ExportToolButton("Set To Throw")] public Callable SetToThrowButton => Callable.From(SetToThrow);
    
    [Export] public MeshInstance3D mesh;
    //[Export] public PhysicalBoneSimulator3D ragdoll;
    
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
    [Export] public TwoBoneIK3D LeftLegThrowIK;
    [Export] public TwoBoneIK3D RightLegThrowIK;
    
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
    [Export] private bool isHoldingBall;
    
    
    [Export, ExportGroup("Throw")] private PathFollow3D throwPathArms;
    [Export] private float throwSpeed;
    [Export] private bool isThrowable;
    [Export] private bool isThrowing;
    public override void _Ready()
    {
        base._Ready();
        walkPathLegsL.ProgressRatio = 0;
        walkPathLegsR.ProgressRatio = legOffset;
        walkPathArmsL.ProgressRatio = legOffset;
        walkPathArmsR.ProgressRatio = 0;
        walkPathBody.ProgressRatio = 0;
        
        isStiffArming = false;
        StiffArmIK.Influence = 0;
        StiffArmBodyIK.Influence = 0;
        stiffArmTarget = targetStiffArm;
        
        //ToggleStiffArm(targetStiffArm);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        float movementStep = (float)delta * speed;
        float throwStep = (float)delta * throwSpeed;

        walkPathLegsL.ProgressRatio += movementStep;
        walkPathLegsR.ProgressRatio += movementStep;
        walkPathArmsL.ProgressRatio += movementStep;
        walkPathArmsR.ProgressRatio += movementStep;
        walkPathBody.ProgressRatio += movementStep * 2;
        
        if (isStiffArming)
        {
            targetStiffArm.GlobalPosition = stiffArmTarget.GlobalPosition;
        }

        if (isThrowing && throwPathArms.ProgressRatio <= .95f)
        {
            throwPathArms.ProgressRatio += throwStep;
        }
        else
        {
            isThrowing = false;
            isThrowable = false;
            throwPathArms.ProgressRatio = 0;
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
        StiffArmIK.Influence = isStiffArming ? 1 : 0;
        StiffArmBodyIK.Influence = isStiffArming ? 1 : 0;
        RightArmIK.Influence = !isStiffArming ? 1 : 0;
        
        BallArmIK.Influence = 1;
    }

    public void SetToNeutral()
    {
        BodyIK.Influence = 0;
        HeadIK.Influence = 0;
        HeadLookAt.Influence = 0;
        LeftLegIK.Influence = 0;
        RightLegIK.Influence = 0;
        RightArmIK.Influence = 0;
        LeftArmIK.Influence = 0;
        
        isStiffArming = false;
        StiffArmIK.Influence = 0;
        StiffArmBodyIK.Influence = 0;

        isThrowable = false;
        ThrowBodyLookAt.Influence = 0;
        ThrowHandLookAt.Influence = 0;
        LeftArmThrowIK.Influence = 0;
        RightArmThrowIK.Influence = 0;
        LeftLegThrowIK.Influence = 0;
        RightLegThrowIK.Influence = 0;

        BallArmIK.Influence = 0;

        isHoldingBall = false;
        
        //ragdoll.PhysicalBonesStopSimulation();
        
        stiffArmTarget = targetStiffArm;
    }

    public void SetToRun()
    {
        BodyIK.Influence = 1;
        HeadIK.Influence = .25f;
        HeadLookAt.Influence = 1;
        LeftLegIK.Influence = 1;
        RightLegIK.Influence = 1;
        RightArmIK.Influence = 1;

        if (!isStiffArming)
        {
            isStiffArming = false;
            StiffArmIK.Influence = 0;
            StiffArmBodyIK.Influence = 0;
        }

        if (!isThrowable)
        {
            ThrowBodyLookAt.Influence = 0;
            ThrowHandLookAt.Influence = 0;
            LeftArmThrowIK.Influence = 0;
            RightArmThrowIK.Influence = 0;
            LeftLegThrowIK.Influence = 0;
            RightLegThrowIK.Influence = 0;
            LeftArmIK.Influence = 1;
        }
        
        if (!isThrowing)
        {
            LeftLegThrowIK.Influence = 0;
            RightLegThrowIK.Influence = 0;
        }

        if (isHoldingBall)
        {
            BallArmIK.Influence = 1;
        }
        else
        {
            BallArmIK.Influence = 0;
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

        isThrowing = true;
        
        LeftArmIK.Influence = 0;
        RightArmIK.Influence = 0;
        
        isStiffArming = false;
        StiffArmIK.Influence = 0;
        StiffArmBodyIK.Influence = 0;
        
        ThrowBodyLookAt.Influence = 1;
        ThrowHandLookAt.Influence = 1;
        LeftArmThrowIK.Influence = 1;
        RightArmThrowIK.Influence = 1;
        LeftLegThrowIK.Influence = 1;
        RightLegThrowIK.Influence = 1;

        if (isThrowing)
        {
            LeftLegIK.Influence = 0;
            RightLegIK.Influence = 0;
            BodyIK.Influence = 0;
        }
        
        BallArmIK.Influence = 0;
    }

    public void SetRagdoll()
    {
        //ragdoll.PhysicalBonesStartSimulation();
    }
}
