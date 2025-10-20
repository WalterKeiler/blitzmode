using Godot;
using System;

public partial class AIManager : Node
{
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
    
    public override void _Ready()
    {
        player = (PlayerController)GetParent();
        player.aiManager = this;
        isOffence = player.isOffence;
        //currentZone = new Zone(new Vector3(15, 0, -10), 10);
        
        overrideTargetPoint = Vector3.Inf;
    }
    
    public override void _Process(double delta)
    {
        if(!init) return;

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

            finalDir = (route + openSpace + blockPlayer).Normalized();
        }
        else
        {
            followRoute *= 0;
            findOpenSpace *= 0;
            block *= 0;

            followPlayer *= 1;
            coverZone *= 1;
            rushBall *= 1;
            finalDir = ((FollowPlayer() * followPlayer) + (CoverZone() * coverZone) + (RushBall() * rushBall)).Normalized();
            
            if(player.GlobalPosition.DistanceTo(ball.GlobalPosition) < 1.6f)
            {
                player.DoAction(PlayerActions.Tackle, -1);
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
        player.GetInput(finalDir);
    }
    
    Vector3 FollowRoute()
    {
        if (currentRoute == null) return player.GlobalPosition;
        if(currentRoute.currentIndex >= currentRoute.targetPoints.Length)
        {
            //followRoute = 0;
            //findOpenSpace = 1;
            return Vector3.Zero;
        }
        if (player.GlobalPosition.DistanceTo(currentRoute.GetLOSTargetPoint(currentRoute.currentIndex)) < 1.5f)
        {
            currentRoute.currentIndex++;
            GD.Print("Moving to next Index: " + currentRoute.currentIndex);
        }
        return currentRoute.currentIndex >= currentRoute.targetPoints.Length ? Vector3.Zero : player.GlobalPosition.DirectionTo(currentRoute.GetLOSTargetPoint(currentRoute.currentIndex));
    }
    
    Vector3 FindOpenSpace()
    {
        Vector3 nearestPlayer = player.GetNearestPlayer(false).GlobalPosition;
        if (player.GlobalPosition.DistanceTo(nearestPlayer) < 10f)
            return -player.GlobalPosition.DirectionTo(nearestPlayer);
        return Vector3.Zero;
    }

    Vector3 Block()
    {
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
        Vector3 nearestPlayer = player.GetNearestPlayer(false).GlobalPosition;
        
        if (player.GlobalPosition.DistanceTo(nearestPlayer) < 1.5f)
        {
            return Vector3.Zero;
        }
        if (currentZone.center.DistanceTo(nearestPlayer) < currentZone.radius)
        {
            return player.GlobalPosition.DirectionTo(nearestPlayer);
        }
        else
        {
            return player.GlobalPosition.DirectionTo(currentZone.center);
        }
    }
    Vector3 FollowPlayer()
    {
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
        Vector3 nearestPlayer = targetPlayer.GlobalPosition;
        if (player.GlobalPosition.DistanceTo(nearestPlayer) < 1.5f)
        {
            return Vector3.Zero;
        }
        return player.GlobalPosition.DirectionTo(nearestPlayer);
    }
    Vector3 RushBall()
    {
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