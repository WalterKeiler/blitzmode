using Godot;
using System;

[GlobalClass]
public partial class PlayerDataOffence : Resource
{
    [Export] public bool IsPlayer;
    [Export(PropertyHint.Range, "0,1,")] public float followRoute;
    [Export(PropertyHint.Range, "0,1,")] public float findOpenSpace;
    [Export(PropertyHint.Range, "0,1,")] public float block;
    [Export] public Route Route;
    [Export] public Vector2 Position;
}
