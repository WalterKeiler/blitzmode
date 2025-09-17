using Godot;
using System;
using System.Collections.Generic;

public partial class Ball : RigidBody3D
{
    public const float BALLHEIGHTMULTIPLIER = .5f;

    public static Ball Instance { get; private set; }

    private RigidBody3D rb;
    
    public BallState ballState;
    public float ballSpeed;
    public Vector3 startPoint;
    public Vector3 endPoint;

    public PlayerController throwingPlayer;
    
    private BallCatchData bestOption;
    
    public List<BallCatchData> catchOptions;
    
    public override void _EnterTree()
    {
        base._EnterTree();
        Instance = this;
    }

    public override void _Ready()
    {
        Freeze = true;
    }
    
    public override void _Process(double delta)
    {
        if (ballState == BallState.Thrown)
        {
            Move(delta);
            if (bestOption != null && ((GlobalPosition.DistanceTo(bestOption.Player.GlobalPosition) <= 5f &&
                                        GlobalPosition.DistanceTo(endPoint) <= (startPoint.DistanceTo(endPoint) / 2)) || bestOption.CalculateScore() >= 500))
            {
                endPoint = bestOption.Player.GlobalPosition;
                GD.Print("Updated End Point");
            }
            if (bestOption != null && GlobalPosition.DistanceTo(bestOption.Player.GlobalPosition) <= 1.25f)
            {
                GD.Print("Caught");
                Reparent(bestOption.Player);
                if (throwingPlayer.PlayerAction.Contains(PlayerActions.Throw))
                    throwingPlayer.PlayerAction.Remove(PlayerActions.Throw);
                
                if(bestOption.Player.isOffence)
                    throwingPlayer.ChangePlayer(bestOption.Player);
                ballState = BallState.Held;
            }
        }
    }

    void Move(double delta)
    {
        Vector3 moveDirection = CalculateBallDirection();
        if(GlobalPosition.Y >= .25f)
        {
            GlobalPosition += moveDirection * ballSpeed;

            LookAt(GlobalPosition + moveDirection);
            ((Node3D)GetChild(0)).RotateZ(this.ballSpeed);
        }
        else
        {
            ballState = BallState.Free;
            Freeze = false;
            ApplyImpulse(moveDirection * ballSpeed * 100);
        }
    }

    public void ResetCatchData()
    {
        catchOptions = new List<BallCatchData>();
        bestOption = null;
    }
    
    public void AddCatchOption(BallCatchData data)
    {
        catchOptions.Add(data);
        EvaluateCatchOptions();
    }

    public void EvaluateCatchOptions()
    {
        BallCatchData bestPick = catchOptions[0];
        float bestScore = bestPick.CalculateScore();
        foreach (var data in catchOptions)
        {
            float score = data.CalculateScore();
            if (bestScore <= score)
            {
                bestPick = data;
                bestScore = score;
                //GD.Print("Best Local: " + bestPick.Player.isOffence + ", Score: " + bestScore);
            }
        }
        bestOption = bestPick;
        GD.Print("Best Calculated: " + bestPick.Player.isOffence + ", Score: " + bestScore);
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
    public const float CHECKDISTANCE = 5f;

    public PlayerController Player;
    public float CatchPriority;
    public float DistanceToTarget;
    public float DistanceToBall;
    public float BallDot;

    public float CalculateScore()
    {
        float mod = 1;
        if (Player.isOffence) mod = 1.1f;
        float dt = 1;
        if (DistanceToTarget <= CHECKDISTANCE) dt = Mathf.Lerp(1, 10, (CHECKDISTANCE - DistanceToTarget) / CHECKDISTANCE);
        
        float db = 1;
        if (DistanceToBall <= CHECKDISTANCE) db = Mathf.Lerp(1, 10,  (CHECKDISTANCE - DistanceToBall) / CHECKDISTANCE);
        
        float score = CatchPriority * dt * db * mod;

        return score;
    }
}

public enum BallState
{
    Held,
    Thrown,
    Free,
    Fumbled
}