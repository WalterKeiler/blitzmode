using Godot;
using System;

[GlobalClass]
public partial class Route : Resource
{
    [Export] public Vector3[] targetPoints;
    public int currentIndex;

    public Route(Vector3[] targetPoints)
    {
        this.targetPoints = targetPoints;
        currentIndex = 0;
    }
}
