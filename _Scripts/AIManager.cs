using Godot;
using System;

public partial class AIManager : Node
{
    public static float DISTANCE_FROM_SIDELINE = 1;
    
    [Export] public bool isOffence;

    [ExportCategory("Offensive Weights")] 
    [Export(PropertyHint.Range, "0,1,")] public float followRoute;
    [Export(PropertyHint.Range, "0,1,")] public float findOpenSpace;
    [Export(PropertyHint.Range, "0,1,")] public float block;
    
    [ExportCategory("Defensive Weights")] 
    [Export(PropertyHint.Range, "0,1,")] public float followPlayer;
    [Export(PropertyHint.Range, "0,1,")] public float coverZone;
    [Export(PropertyHint.Range, "0,1,")] public float rushBall;

    [Export] public Route currentRoute;
    [Export] public Zone currentZone;

    public Vector3 overrideTargetPoint;
    
    public PlayerController targetPlayer;
    public bool init = false;
    private PlayerController player;
    private Ball ball;
    private bool setup = false;

    RandomNumberGenerator rng;
    Vector3 ranDir;

    public override void _EnterTree()
    {
        base._EnterTree();
        PlayManager.InitPlay += Init;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        PlayManager.InitPlay += Init;
    }

    public override void _Ready()
    {
        player = (PlayerController)GetParent();
        player.aiManager = this;
        //currentZone = new Zone(new Vector3(15, 0, -10), 10);

        rng = new RandomNumberGenerator();
        
        overrideTargetPoint = Vector3.Inf;
        
        RandomDirection();
    }

    void Init()
    {
        init = true;
        targetPlayer = null;
        overrideTargetPoint = Vector3.Inf;
        isOffence = player.isOffence;
    }
    
    public override void _Process(double delta)
    {
        if(!init || !player.CanAct) return;

        if (!setup)
        {
            ball = Ball.Instance;
            setup = true;
        }
        
        if(player.isPlayerControlled) return;
        
        Vector3 finalDir = Vector3.Zero;
        
        if(isOffence)
        {
            followRoute *= 1;
            findOpenSpace *= 1;
            block *= 1;

            followPlayer *= 0;
            coverZone *= 0;
            rushBall *= 0;

            Vector3 route = followRoute * FollowRoute();
            Vector3 openSpace = findOpenSpace * FindOpenSpace();
            Vector3 blockPlayer = block * Block();

            finalDir = (route + openSpace + blockPlayer);
        }
        else
        {
            followRoute *= 0;
            findOpenSpace *= 0;
            block *= 0;

            followPlayer *= 1;
            coverZone *= 1;
            rushBall *= 1;
            finalDir = ((FollowPlayer() * followPlayer) + (CoverZone() * coverZone) + (RushBall() * rushBall));
            
            if(player.GlobalPosition.DistanceTo(ball.GlobalPosition) < 2f)
            {
                player.DoAction(PlayerActions.Tackle, player.playerID);
            }
            
        }

        

        if (overrideTargetPoint < Vector3.Inf && ball.ballState == BallState.Thrown)
        {
            if(player.GlobalPosition.DistanceTo(overrideTargetPoint) > .01f)
            {
                followRoute = 0;
                finalDir = player.GlobalPosition.DirectionTo(overrideTargetPoint);
                finalDir *= new Vector3(1, 0, 1);
                finalDir = finalDir.Normalized();
            }
            else
            {
                finalDir = Vector3.Zero;
            }
        }

        if (Vector3.Zero != finalDir) finalDir += ranDir;
        
        player.GetInput(finalDir.Normalized());
    }

    async void RandomDirection()
    {
        ranDir = (new Vector3(rng.Randf(), rng.Randf(), rng.Randf()) / 20);

        await ToSignal(GetTree().CreateTimer(1f), "timeout");
        
        RandomDirection();
    }
    
    Vector3 FollowRoute()
    {
        if(followRoute == 0) return Vector3.Zero;
        
        if (currentRoute == null) return player.GlobalPosition;
        if(currentRoute.currentIndex >= currentRoute.targetPoints.Length)
        {
            currentRoute.currentIndex = (int.MaxValue - 100);
            switch (currentRoute.endAction)
            {
                //followRoute = 0;
                //findOpenSpace = 1;
                case EndRouteAction.Continue:
                    if(MathF.Abs(player.GlobalPosition.Z) >= (GameManager.Instance.fieldWidth / 2f) - DISTANCE_FROM_SIDELINE) return Vector3.Right * PlayManager.Instance.PlayDirection;
                    return currentRoute.targetPoints[^2].DirectionTo(currentRoute.targetPoints[^1]);
                case EndRouteAction.Block:
                    followRoute = 0;
                    block = 1;
                    break;
                case EndRouteAction.Zone:
                    followRoute = 0;
                    coverZone = 1;
                    break;
            }

            return Vector3.Zero;
        }
        if (player.GlobalPosition.DistanceTo(currentRoute.GetLOSTargetPoint(currentRoute.currentIndex)) < 1.5f)
        {
            currentRoute.currentIndex++;
            //GD.Print("Moving to next Index: " + currentRoute.currentIndex);
        }
        return currentRoute.currentIndex >= currentRoute.targetPoints.Length ? Vector3.Zero : player.GlobalPosition.DirectionTo(currentRoute.GetLOSTargetPoint(currentRoute.currentIndex));
    }
    
    Vector3 FindOpenSpace()
    {
        if(findOpenSpace == 0) return Vector3.Zero;

        Vector3 nearestPlayer = player.GetNearestPlayer(false).GlobalPosition;
        if (player.GlobalPosition.DistanceTo(nearestPlayer) < 10f)
            return -player.GlobalPosition.DirectionTo(nearestPlayer);
        return Vector3.Zero;
    }

    Vector3 Block()
    {
        if(block == 0) return Vector3.Zero;

        PlayerController nearestPlayer = player.GetNearestPlayer(false);
        Vector3 dir = player.GlobalPosition.DirectionTo(nearestPlayer.GlobalPosition);
        if (player.GlobalPosition.DistanceTo(nearestPlayer.GlobalPosition) < 1f)
        {
            player.Block();
            if(player.IsBlocking)
                return Vector3.Zero;
        }

        return dir;
    }
    
    Vector3 CoverZone()
    {
        if(coverZone == 0 || currentZone == null) return Vector3.Zero;
        PlayerController nearestPlayer = player.GetNearestPlayerByType(false, PlayerType.Receiver);
        PlayerController nearestQB = player.GetNearestPlayerByType(false, PlayerType.Quarterback);

        if (player.GlobalPosition.DistanceTo(nearestQB.GlobalPosition) < player.GlobalPosition.DistanceTo(nearestPlayer.GlobalPosition))
            nearestPlayer = nearestQB;
        
        if (player.GlobalPosition.DistanceTo(nearestPlayer.GlobalPosition) < 1.5f)
        {
            return Vector3.Zero;
        }
        if (currentZone.GetLOSCenter().DistanceTo(nearestPlayer.GlobalPosition) < currentZone.radius)
        {
            return player.GlobalPosition.DirectionTo(nearestPlayer.GlobalPosition + nearestPlayer._moveDirection);
        }
        else
        {
            return player.GlobalPosition.DirectionTo(currentZone.GetLOSCenter());
        }
    }
    Vector3 FollowPlayer()
    {
        if(followPlayer == 0) return Vector3.Zero;

        if (targetPlayer == null)
        {
            PlayerController[] targets = player.GetNearestPlayersByType(false, PlayerType.Receiver);
            int i = 0;
            foreach (PlayerController p in GameManager.Instance.defencePlayers)
            {
                if (p.aiManager.targetPlayer == targets[i])
                {
                    i++;
                    continue;
                }
                //GD.Print(i);
                targetPlayer = targets[i];
                break;
            }
        }

        if (targetPlayer == null) return Vector3.Zero;
        
        Vector3 nearestPlayer = targetPlayer.GlobalPosition;
        if (player.GlobalPosition.DistanceTo(nearestPlayer) < 1.5f)
        {
            return Vector3.Zero;
        }
        return player.GlobalPosition.DirectionTo(nearestPlayer + targetPlayer._moveDirection);
    }
    Vector3 RushBall()
    {
        if(rushBall == 0) return Vector3.Zero;

        if(ball.ballState == BallState.Free)
        {
            return player.GlobalPosition.DirectionTo(ball.GlobalPosition);
        }
        
        Vector3 nearestPlayer = player.GetNearestPlayerToBall(false).GlobalPosition;
        
        if (player.GlobalPosition.DistanceTo(nearestPlayer) < 1.5f)
        {
            return Vector3.Zero;
        }
        
        return player.GlobalPosition.DirectionTo(ball.GlobalPosition);;
    }
}