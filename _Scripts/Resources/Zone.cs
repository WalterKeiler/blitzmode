using Godot;
using System;

[GlobalClass]
public partial class Zone : Resource
{
    [Export] public Vector3 center;
    [Export] public float radius;

    public Zone(Vector3 center, float radius)
    {
        this.center = center;
        this.radius = radius;
    }
}