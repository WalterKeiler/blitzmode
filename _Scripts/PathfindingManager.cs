using Godot;
using System;

public partial class PathfindingManager : Node
{
    public static PathfindingManager Instance;
    
    private Vector3 target;

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

    public float QuerySDF(Vector3 samplePos, Vector3 target, PlayerController ignore)
    {
        this.target = target;
        return Mathf.Max(PlayerSDF(samplePos, ignore), TargetSDF(samplePos));
    }

    float PlayerSDF(Vector3 samplePos, PlayerController ignore)
    {
        float d = float.MaxValue;
        
        foreach (var player in Players)
        {
            if(player == ignore) continue;
            
            d = Mathf.Min(d, sdCircle(player.GlobalPosition - samplePos, .5f));
        }

        if (d < 0) d = -(d * 10000);
        else d = 0;

        return d;
    }

    float TargetSDF(Vector3 samplePos)
    {
        return sdCircle(target - samplePos, 0f);
    }
    
    float sdCircle( Vector3 p, float r )
    {
        Vector2 pxz = new Vector2(p.X, p.Z);
        return pxz.Length() - r;
    }
}