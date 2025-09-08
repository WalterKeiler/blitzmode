using Godot;
using System;

public partial class Ball : Node3D
{
    public float ballSpeed;
    public Vector3 startPoint;
    public Vector3 endPoint;
    public bool isThrown;
    
    public override void _Process(double delta)
    {
        if (isThrown)
        {
            Move(delta);
        }
    }

    void Move(double delta)
    {
        Vector3 moveDirection = CalculateBallDirection();
        if(GlobalPosition.Y > 0)
            GlobalPosition += moveDirection * ballSpeed;
    }
    
    public Vector3 CalculateBallDirection()
    {
        Vector3 midPoint = endPoint.Lerp(startPoint, .5f);
        float distance = startPoint.DistanceTo(endPoint);
        midPoint.Y = Mathf.Clamp(.5f * distance, 1, 10);
        Vector3 upDir = startPoint.DirectionTo(midPoint);
        Vector3 downDir = midPoint.DirectionTo(endPoint);

        Vector2 s = new Vector2(startPoint.X, startPoint.Z);
        Vector2 c = new Vector2(GlobalPosition.X, GlobalPosition.Z);

        Vector3 dir = upDir.Lerp(downDir, s.DistanceTo(c) / distance);

        return dir.Normalized();
    }
}
