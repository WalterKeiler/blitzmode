using Godot;
using System;

public partial class PathfindingManager : Node
{
    public static PathfindingManager Instance;

    private PlayerController[] Players;
    
    public override void _EnterTree()
    {
        base._EnterTree();
        Instance = this;
    }

    
    public override void _Ready()
    {
        base._Ready();

        Players = GameManager.Instance.players;
    }

    public float QueryZone(Vector3 samplePos, Zone zone, PlayerController ignore)
    {
        float psdf = PlayerSDF(samplePos, ignore);
        
        float zoneSD = sdCircle(samplePos - zone.center, zone.radius);
        //if(zoneSD < 0) zoneSD += zone.radius;
        
        float targetSDF = float.MaxValue;
        foreach (var player in Players)
        {
            if (player.playerStats.PlayerType == PlayerType.Receiver ||
                player.playerStats.PlayerType == PlayerType.Quarterback)
            {
                targetSDF = Mathf.Min(targetSDF, TargetSDF(samplePos, player.GlobalPosition));
            }
        }

        if (zoneSD > 0) return (zoneSD + 1) * 10000;
        
        return Mathf.Max(psdf, targetSDF);
    }
    
    public float QuerySDF(Vector3 samplePos, Vector3 target, PlayerController ignore)
    {
        float tsdf = TargetSDF(samplePos, target);
        float psdf = PlayerSDF(samplePos, ignore);
        
        return Mathf.Max(tsdf + psdf, tsdf);
    }

    public float QueryBlockSDF(Vector3 samplePos, Vector3 targetA, Vector3 targetB, PlayerController ignore)
    {
        float tsdf = Mathf.Min(TargetSDF(samplePos, targetA), sdSegment(samplePos, targetA, targetB));
        float psdf = PlayerSDF(samplePos, ignore);
        
        return Mathf.Max(tsdf + psdf, tsdf);
    }
    
    float PlayerSDF(Vector3 samplePos, PlayerController ignore)
    {
        float d = float.MaxValue;
        
        foreach (var player in Players)
        {
            if(player == ignore) continue;
            
            d = Mathf.Min(d, sdCircle(player.GlobalPosition - samplePos, .65f));
        }

        if (d < 0) d = -(d * 10000);
        //else if(d < .5f) d = (d * 1000);
        else d = 0;

        return d;
    }

    float TargetSDF(Vector3 samplePos, Vector3 target)
    {
        return sdCircle(target - samplePos, 0f);
    }
    

    float TargetLineSDF(Vector3 samplePos, Vector3 target)
    {
        return sdCircle(target - samplePos, 0f);
    }
    
    float sdCircle( Vector3 p, float r )
    {
        Vector2 pxz = new Vector2(p.X, p.Z);
        return pxz.Length() - r;
    }
    
    float sdSegment(Vector3 p, Vector3 a, Vector3 b )
    {
        Vector2 pxz = new Vector2(p.X, p.Z);
        Vector2 axz = new Vector2(a.X, a.Z);
        Vector2 bxz = new Vector2(b.X, b.Z);
        
        Vector2 pa = pxz-axz, ba = bxz-axz;
        float h = (float) Mathf.Clamp((pa.Dot(ba) / ba.Dot(ba)), 0.0, 1.0 );
        return (pa - ba*h).Length();
    }
}