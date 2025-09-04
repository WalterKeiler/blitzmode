using Godot;
using System;

[GlobalClass]
public partial class PlayerDataDefence : Resource
{
    [Export] public bool IsPlayer;
    [Export(PropertyHint.Range, "0,1,")] public float followPlayer;
    [Export(PropertyHint.Range, "0,1,")] public float coverZone;
    [Export(PropertyHint.Range, "0,1,")] public float rushBall;
    [Export] public Vector2 Position;
}
