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

    public Route currentRoute;
    public Zone currentZone;

    public PlayerController targetPlayer;
    
    private PlayerController player;
    
    public override void _Ready()
    {
        player = (PlayerController)GetParent();
        player.aiManager = this;
        isOffence = player.isOffence;
        currentRoute = new Route(new[]
        {
            new Vector3(0, 0, 20),
            new Vector3(20, 0, 20),
            new Vector3(30, 0, 10),
            new Vector3(30, 0, 0),
            new Vector3(20, 0, -10)
        });
        currentZone = new Zone(new Vector3(15, 0, -10), 10);
    }
    
    public override void _Process(double delta)
    {
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

            finalDir = (route + openSpace).Normalized();
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
        }

        player.GetInput(finalDir);
    }

    Vector3 FollowRoute()
    {
        if(currentRoute.currentIndex >= currentRoute.targetPoints.Length)
        {
            followRoute = 0;
            findOpenSpace = 1;
            return Vector3.Zero;
        }
        if (player.GlobalPosition.DistanceTo(currentRoute.targetPoints[currentRoute.currentIndex]) < 1.5f)
        {
            currentRoute.currentIndex++;
        }
        return currentRoute.currentIndex >= currentRoute.targetPoints.Length ? Vector3.Zero : player.GlobalPosition.DirectionTo(currentRoute.targetPoints[currentRoute.currentIndex]);
    }
    
    Vector3 FindOpenSpace()
    {
        Vector3 nearestPlayer = player.GetNearestPlayer(false).GlobalPosition;
        if (player.GlobalPosition.DistanceTo(nearestPlayer) < 10f)
            return -player.GlobalPosition.DirectionTo(nearestPlayer);
        return Vector3.Zero;
    }
    
    Vector3 CoverZone()
    {
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
        Vector3 nearestPlayer = player.GetNearestPlayer(false).GlobalPosition;
        if (player.GlobalPosition.DistanceTo(nearestPlayer) < 1.5f)
        {
            return Vector3.Zero;
        }
        return player.GlobalPosition.DirectionTo(nearestPlayer);
    }
    Vector3 RushBall()
    {
        Vector3 nearestPlayer = player.GetNearestPlayerToBall(false).GlobalPosition;
        if (player.GlobalPosition.DistanceTo(nearestPlayer) < 1.5f)
        {
            return Vector3.Zero;
        }
        return player.GlobalPosition.DirectionTo(nearestPlayer);
    }
}