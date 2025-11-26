using Godot;
using System;

public partial class AIManager : Node
{
    public static float DISTANCE_FROM_SIDELINE = 1;
    public const int PATHFINDING_STEPS = 32;
    
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

    private PathfindingManager pm;
    
    RandomNumberGenerator rng;
    Vector3 ranDir;

    Node3D debugBox;
    
    public override void _EnterTree()
    {
        base._EnterTree();
        //PlayManager.InitPlay += Init;
        PlayManager.EndPlay += Stop;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        //PlayManager.InitPlay += Init;
        PlayManager.EndPlay += Stop;
    }

    public override void _Ready()
    {
        player = (PlayerController)GetParent();
        player.aiManager = this;
        pm = PathfindingManager.Instance;
        //currentZone = new Zone(new Vector3(15, 0, -10), 10);

        rng = new RandomNumberGenerator();
        
        overrideTargetPoint = Vector3.Inf;
        
        RandomDirection();
    }

    public void Init()
    {
        init = true;
        targetPlayer = null;
        overrideTargetPoint = Vector3.Inf;
        isOffence = player.isOffence;
    }

    void Stop(bool moveLineOfScrimmage)
    {
        
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
            
            if(player.GlobalPosition.DistanceTo(player.GetNearestPlayerToBall(false).GlobalPosition) < 1.65f)
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
                    if(MathF.Abs(player.GlobalPosition.Z) >= (GameManager.Instance.fieldWidth / 2f) - DISTANCE_FROM_SIDELINE) return QuerySDF(player.GlobalPosition + Vector3.Right * PlayManager.Instance.PlayDirection);
                    return QuerySDF(player.GlobalPosition + currentRoute.targetPoints[^2].DirectionTo(currentRoute.targetPoints[^1]));
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
        return currentRoute.currentIndex >= currentRoute.targetPoints.Length ? Vector3.Zero : QuerySDF(currentRoute.GetLOSTargetPoint(currentRoute.currentIndex));
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
        
        if (debugBox == null)
        {
            MeshInstance3D testMesh = new MeshInstance3D();
            testMesh.Mesh = new BoxMesh();
            testMesh.MaterialOverride = new Material();
            testMesh.Position = Vector3.Zero;
            GetParent().AddChild(testMesh);
            debugBox = testMesh;
        }
        
        PlayerController[] nearestPlayers = player.GetNearestPlayers(false);
        PlayerController nearestPlayer = targetPlayer;
        //GD.Print(nearestPlayers.Length);
        if(targetPlayer == null)
        {
            foreach (PlayerController p in nearestPlayers)
            {
                if (targetPlayer == null)
                {
                    if (p.IsTargeted)
                    {
                        GD.Print("Targeted Already");
                        continue;
                    }
                }
                else if (targetPlayer.GlobalPosition.DistanceTo(ball.GlobalPosition) <
                         p.GlobalPosition.DistanceTo(ball.GlobalPosition)
                         || p.IsBlocked || p.IsTargeted || (!p.isPlayerControlled && p.aiManager.rushBall < 1))
                {
                    GD.Print("Something");
                    continue;
                }

                if (targetPlayer != null) targetPlayer.IsTargeted = false;
                nearestPlayer = p;
                targetPlayer = p;
                p.IsTargeted = true;
                GD.Print(player.Name + " Targeting: " + p.Name);
                break;
            }
        }
        
        switch (nearestPlayer)
        {
            case null when targetPlayer == null:
                GD.Print("No nearest player");
                return Vector3.Zero;
            case null when targetPlayer != null:
                nearestPlayer = targetPlayer;
                break;
        }

        Vector3 dir = QuerySDF(nearestPlayer.GlobalPosition);

        if (player.GlobalPosition.DistanceTo(nearestPlayer.GlobalPosition) < 1f)
        {
            player.Block();
            if(player.IsBlocking)
                return Vector3.Zero;
        }
        debugBox.GlobalPosition = nearestPlayer.GlobalPosition;
        debugBox.Scale = Vector3.One;
        return dir;
    }
    
    Vector3 CoverZone()
    {
        if(coverZone == 0 || currentZone == null) return Vector3.Zero;

        return QueryZoneSDF(currentZone);
        
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
        if (player.GlobalPosition.DistanceTo(nearestPlayer) < 1.5f || 
            targetPlayer._moveDirection.Dot(targetPlayer.GlobalPosition.DirectionTo(player.GlobalPosition)) > .85f)
        {
            return Vector3.Zero;
        }

        return QuerySDF(nearestPlayer + targetPlayer._moveDirection);
        return player.GlobalPosition.DirectionTo(nearestPlayer + targetPlayer._moveDirection);
    }
    Vector3 RushBall()
    {
        if(rushBall == 0) return Vector3.Zero;
        

        return QuerySDF(ball.GlobalPosition);
        
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

    Vector3 QuerySDF(Vector3 target)
    {
        float minWeight = float.MaxValue;
        Vector3 finalDir = Vector3.Zero;
        for (int i = 0; i < PATHFINDING_STEPS; i++)
        {
            Vector3 unitDir = MathW.PointOnUnitCircleXZ(i,i,PATHFINDING_STEPS);
            
            float testWeight = pm.QuerySDF(player.GlobalPosition + unitDir * .1f, target, player);
            
            if (minWeight > testWeight)
            {
                minWeight = testWeight;
                finalDir = unitDir;
            }
        }

        return finalDir;
    }

    Vector3 QueryBlockSDF(Vector3 target)
    {
        float minWeight = float.MaxValue;
        Vector3 finalDir = Vector3.Zero;
        for (int i = 0; i < PATHFINDING_STEPS; i++)
        {
            Vector3 unitDir = MathW.PointOnUnitCircleXZ(i,i,PATHFINDING_STEPS);

            float testWeight = pm.QueryBlockSDF(player.GlobalPosition + unitDir * .1f, target, ball.GlobalPosition, player);
            
            if (minWeight > testWeight)
            {
                minWeight = testWeight;
                finalDir = unitDir;
            }
        }

        return finalDir;
    }
    
    Vector3 QueryZoneSDF(Zone zone)
    {
        float minWeight = float.MaxValue;
        Vector3 finalDir = Vector3.Zero;
        for (int i = 0; i < PATHFINDING_STEPS; i++)
        {
            Vector3 unitDir = MathW.PointOnUnitCircleXZ(i, i, PATHFINDING_STEPS);

            float testWeight = pm.QueryZone(player.GlobalPosition + unitDir * .1f, zone, player);

            float ballTarget = float.MaxValue;

            if (ball.ballState == BallState.Thrown)
                ballTarget = pm.QuerySDF(player.GlobalPosition + unitDir * .1f, ball.endPoint, player) - 10;

            testWeight = Mathf.Min(testWeight, ballTarget);
            if (minWeight > testWeight)
            {
                minWeight = testWeight;
                finalDir = unitDir;
            }
        }

        return finalDir;
    }
}

public class MathW
{
    public static Vector3 PointOnUnitCircleXZ(float x, float z, float steps)
    {
        return new Vector3(
            Mathf.Cos((2 * Mathf.Pi * (float) x) / (float) steps), 0,
            Mathf.Sin((2 * Mathf.Pi * (float) z) / (float) steps));
    }
}