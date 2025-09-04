using Godot;
using System;

[GlobalClass]
public partial class PlayerDataOffence : Resource
{
    [Export] public bool IsPlayer;
    [Export] public bool IsBlocker;
    [Export] public Route Route;
    [Export] public Vector2 Position;
}
