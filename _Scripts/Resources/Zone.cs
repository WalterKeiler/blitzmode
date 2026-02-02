using Godot;
using System;

[GlobalClass]
public partial class Zone : Resource
{
    [Export] public Vector3 center;
    [Export] public float radius;
    
    public Vector3 GetLOSCenter()
    {
        Vector3 pos = center;
        pos *= PlayManager.Instance.PlayDirection;
        pos += Vector3.Right * PlayManager.Instance.lineOfScrimmage;

        return pos;
    }
}