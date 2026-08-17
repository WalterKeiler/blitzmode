using Godot;
using System;
using System.Collections.Generic;

public partial class Ball : RigidBody3D
{
    public const float BALLHEIGHTMULTIPLIER = .5f;

    public static Ball Instance { get; private set; }

    private RigidBody3D rb;
    
    [Export] public BallState ballState;
    [Export] public float ballSpeed;
    [Export] public Vector3 startPoint;
    [Export] public Vector3 endPoint;

    public PlayerController throwingPlayer;
    
    private BallCatchData bestOption;
    
    public List<BallCatchData> catchOptions;
    public static event Action<bool> BallCaught;

    private bool crossedLOS = false;
    
    private bool init = false;
    private bool isSnap = false;

    private PlayManager pm;
    
    public override void _Ready()
    {
        Freeze = true;
        pm = PlayManager.Instance;
    }
    
    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
        PlayManager.InitPlay += Init;
        PlayerController.Snapped += Snap;
        PlayerController.CrossedLOS += PlayerControllerOnCrossedLOS;
        PlayManager.EndPlay += EndPlay;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        PlayManager.InitPlay -= Init;
        PlayerController.Snapped -= Snap;
        PlayerController.CrossedLOS -= PlayerControllerOnCrossedLOS;
        PlayManager.EndPlay -= EndPlay;
    }

    private void PlayerControllerOnCrossedLOS(bool obj)
    {
        crossedLOS = true;
    }

    void Init(bool isST)
    {
        init = true;
        Freeze = true;
        crossedLOS = false;
        if(!isST)
        {
            ballState = BallState.Held;
            endPoint = Vector3.Inf;
            startPoint = Vector3.Inf;
        }
    }

    void EndPlay(bool b)
    {
        init = false;
    }

    void Snap(bool isSpecialTeams)
    {
        if(ballState == BallState.Free) return;
        //if(init) return;
        if(isSpecialTeams)
        {
            init = true;
            ballState = ballState;
            //ballState = BallState.Thrown;
            return;
        }
        
        
        GD.Print("Snap");
        
        PlayerController qb = GameManager.Instance.offencePlayers.Find(x => x.playerStats.PlayerType == PlayerType.Quarterback);
        startPoint = GlobalPosition;
        endPoint = qb.GlobalPosition + Vector3.Up * .75f;
        BallCatchData data = new BallCatchData
        {
            BallDot = 1,
            CatchPriority = qb.playerStats.Catching + 100,
            DistanceToBall = qb.GlobalPosition.DistanceTo(Ball.Instance.GlobalPosition),
            DistanceToTarget = qb.GlobalPosition.DistanceTo(Ball.Instance.GlobalPosition),
            Player = qb
        };
        AddCatchOption(data);
        //Vector3 moveDirection = GlobalPosition.DirectionTo(endPoint);
        ballState = BallState.Snapped;
        //Freeze = false;
        //GlobalPosition = endPoint;
        //ApplyCentralImpulse(moveDirection * 10);
        isSnap = true;
        //ApplyImpulse(moveDirection * ballSpeed);
    }
    
    public override void _PhysicsProcess(double delta)
    {
        if (isSnap)
        {
            Move(delta);
            
            if (bestOption != null && ((GlobalPosition.DistanceTo(bestOption.Player.GlobalPosition) <= 5f &&
                                        GlobalPosition.DistanceTo(endPoint) <= (startPoint.DistanceTo(endPoint) / 2)) || bestOption.CalculateScore() >= 600))
            {
                //endPoint = bestOption.Player.GlobalPosition;

                if (bestOption.Player.isOffence)
                {
                    bestOption.Player.aiManager.overrideTargetPoint = endPoint;
                }
                // GD.Print("Updated End Point");
            }
            if (bestOption != null && GlobalPosition.DistanceTo(bestOption.Player.GlobalPosition) <= 1f)
            {
                GD.Print("Caught");
                //bestOption.Player.HasBall = true;
                Caught(bestOption.Player);
            }
            
        }
        
        if(!init) return;
        //GD.Print(ballState);
        if (ballState == BallState.Thrown)
        {
            Move(delta);
            if (bestOption != null && ((GlobalPosition.DistanceTo(bestOption.Player.GlobalPosition) <= 5f &&
                                        GlobalPosition.DistanceTo(endPoint) <= (startPoint.DistanceTo(endPoint) / 2)) || bestOption.CalculateScore() >= 600))
            {
                //endPoint = bestOption.Player.GlobalPosition;

                if (bestOption.Player.isOffence)
                {
                    bestOption.Player.aiManager.overrideTargetPoint = endPoint;
                }
                // GD.Print("Updated End Point");
            }
            if (bestOption != null && GlobalPosition.DistanceTo(bestOption.Player.GlobalPosition) <= 1f)
            {
                GD.Print("Caught");
                //bestOption.Player.HasBall = true;
                Caught(bestOption.Player);
            }
        }

        if (ballState == BallState.Free)
        {
            Freeze = false;
        }
        
        //GD.Print(GlobalPosition.X * pm.PlayDirection >= GameManager.Instance.fieldLength / 2f);
        if (ballState == BallState.Held && !pm.midTurnover && !pm.inbetweenPlays && 
            (GlobalPosition.X * pm.PlayDirection >= GameManager.Instance.fieldLength / 2f &&
             GlobalPosition.X * pm.PlayDirection <=
             (GameManager.Instance.fieldLength / 2f) + GameManager.Instance.EndzoneDepth))
        {
            pm.InvokeEndPlay(true);
        }

        if (ballState == BallState.Free && pm.isKickoff && !pm.inbetweenPlays && 
            (GlobalPosition.X * -pm.PlayDirection >= GameManager.Instance.fieldLength / 2f &&
             GlobalPosition.X * -pm.PlayDirection <=
             (GameManager.Instance.fieldLength / 2f) + GameManager.Instance.EndzoneDepth))
        {
            float ballPos = pm.PlayDirection == 1 ? -(GameManager.Instance.fieldLength / 2f - GameManager.Instance.touchBackDistance) : GameManager.Instance.fieldLength - GameManager.Instance.touchBackDistance;
            
            GlobalPosition = Vector3.Right * ballPos;
            
            pm.Turnover(false, pm.PlayDirection);
            pm.InvokeEndPlay(true);
        }
        
        if (Mathf.Abs(GlobalPosition.Z) >= (GameManager.Instance.fieldWidth + 1.5f) / 2f && !pm.inbetweenPlays)
        {
            pm.InvokeEndPlay(ballState == BallState.Held);
        }
    }

    public void Caught(PlayerController catchPlayer)
    {
        init = true;
        catchPlayer.HasBall = true;
        Reparent(catchPlayer);
        
        Position = Vector3.Up;
        ballState = BallState.Held;

        if (isSnap)
        {
            isSnap = false;
            return;
        }
        
        if (throwingPlayer != null && throwingPlayer.PlayerAction.Contains(PlayerActions.Throw))
            throwingPlayer.PlayerAction.Remove(PlayerActions.Throw);
        
        if(throwingPlayer != null && catchPlayer.isOffence == throwingPlayer.isOffence)
        {
            throwingPlayer ??= GameManager.Instance.GetPlayerControlledPlayer(true);
            
            if(throwingPlayer != catchPlayer)
                throwingPlayer.ChangePlayer(catchPlayer);
            
            if(ballState == BallState.Thrown || crossedLOS)
                BallCaught?.Invoke(true);
        }
        else
        {
            BallCaught?.Invoke(false);
        }
        
        //Freeze = true;
        //endPoint = Vector3.Inf;
        //startPoint = Vector3.Inf;
    }
    
    void Move(double delta)
    {
        Vector3 moveDirection = CalculateBallDirection();
        if(GlobalPosition.Y >= .15f)
        {
            float s = isSnap ? ballSpeed / 5 : ballSpeed;
            GlobalPosition += moveDirection * s;

            LookAt(GlobalPosition + moveDirection);
            ((Node3D)GetChild(0)).RotateZ(this.ballSpeed);
        }
        else
        {
            if(ballState == BallState.Thrown && !pm.isKickoff && !pm.inbetweenPlays)
            {
                GD.Print("Incomplete Pass");
                pm.InvokeEndPlay(false);
                return;
            }
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
        catchOptions ??= new List<BallCatchData>();
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
        // GD.Print("Best Calculated is Off: " + bestPick.Player.isOffence + ", Score: " + bestScore);
    }
    
    public Vector3 CalculateBallDirection()
    {
        if (bestOption is {CatchPriority: > 10000000}) endPoint = bestOption.Player.GlobalPosition;
        
        Vector3 midPoint = endPoint.Lerp(startPoint, .5f);
        float distance = startPoint.DistanceTo(endPoint);
        midPoint.Y = Mathf.Clamp(BALLHEIGHTMULTIPLIER * distance, 1, 10);
        if (pm.isKickoff) midPoint.Y *= 2;
        if (isSnap) midPoint.Y = 2;
        Vector3 upDir = startPoint.DirectionTo(midPoint);
        Vector3 downDir = GlobalPosition.DirectionTo(endPoint);

        Vector2 s = new Vector2(startPoint.X, startPoint.Z);
        Vector2 c = new Vector2(GlobalPosition.X, GlobalPosition.Z);

        float t = s.DistanceTo(c) / distance;
        
        t = Mathf.Clamp(t, 0, 1);
        
        Vector3 dir = upDir.Lerp(downDir, t);
        
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
        if (!Player.CanCatch) return 0;
        
        float mod = 1;
        if (Player.isOffence) mod = 1.05f;
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
    Snapped,
    Fumbled
}