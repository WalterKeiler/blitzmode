using Godot;
using System;

[GlobalClass]
public partial class Zone : Resource
{
    [Export] public Vector3 center;
    [Export] public float radius;
}