using Godot;
using System;
using System.Collections.Generic;

public partial class Ball : Node3D
{
    public const float BALLHEIGHTMULTIPLIER = .5f;

    public static Ball Instance { get; private set; }
    
    public BallState ballState;
    public float ballSpeed;
    public Vector3 startPoint;
    public Vector3 endPoint;

    private BallCatchData bestOption;
    
    public List<BallCatchData> catchOptions;
    public override void _Ready()
    {
        Instance = this;
    }
    
    public override void _Process(double delta)
    {
        if (ballState == BallState.Thrown)
        {
            Move(delta);
        }
    }

    void Move(double delta)
    {
        Vector3 moveDirection = CalculateBallDirection();
        if(GlobalPosition.Y > 0)
            GlobalPosition += moveDirection * ballSpeed;
    }

    public void ResetCatchData()
    {
        catchOptions = new List<BallCatchData>();
    }
    
    public void AddCatchOption(BallCatchData data)
    {
        catchOptions.Add(data);
    }

    public void EvaluateCatchOptions()
    {
        
    }
    
    public Vector3 CalculateBallDirection()
    {
        Vector3 midPoint = endPoint.Lerp(startPoint, .5f);
        float distance = startPoint.DistanceTo(endPoint);
        midPoint.Y = Mathf.Clamp(BALLHEIGHTMULTIPLIER * distance, 1, 10);
        Vector3 upDir = startPoint.DirectionTo(midPoint);
        Vector3 downDir = midPoint.DirectionTo(endPoint);

        Vector2 s = new Vector2(startPoint.X, startPoint.Z);
        Vector2 c = new Vector2(GlobalPosition.X, GlobalPosition.Z);

        Vector3 dir = upDir.Lerp(downDir, s.DistanceTo(c) / distance);

        return dir.Normalized();
    }
}

public class BallCatchData
{
    public PlayerController Player;
    public float CatchPriority;
    public float DistanceToTarget;
    public float DistanceToBall;
    public float BallDot;
}

public enum BallState
{
    Held,
    Thrown,
    Free,
    Fumbled
}