using Godot;
using System;

[GlobalClass]
public partial class Route : Resource
{
    [Export] public Vector3[] targetPoints;
    [Export] public int currentIndex = 0;

    public Vector3 GetPointByLength(Vector3 currentPos, float length)
    {
        float distanceFromLastPoint =
            targetPoints[Math.Clamp(currentIndex - 1, 0, int.MaxValue)].DistanceTo(currentPos);
        float pastLength = 0;
        for (int i = 0; i < currentIndex; i++)
        {
            pastLength += targetPoints[i].DistanceTo(targetPoints[i + 1]);
        }
        
        float totalLength = pastLength + distanceFromLastPoint + length;
        for (int i = 0; i < targetPoints.Length; i++)
        {
            float dist = targetPoints[i].DistanceTo(targetPoints[Math.Clamp(i - 1, 0, int.MaxValue)]);

            totalLength -= dist;
            
            if (totalLength <= 0)
            {
                return targetPoints[i - 1].Lerp(targetPoints[i], Math.Abs(totalLength) / dist);
            }
        }

        return targetPoints[^1];
    }
    
    public Vector3 GetThrowToPoint(Vector3 receiverPos, Vector3 qbPos, float receiverSpeed, ref float ballSpeed)
    {
        if (currentIndex >= targetPoints.Length) return receiverPos;
        
        float time = qbPos.DistanceTo(receiverPos) / ballSpeed;

        Vector3 targetPos = GetPointByLength(receiverPos, time * receiverSpeed);
        float neededSpeed = (qbPos.DistanceTo(targetPos)) / time;
        ballSpeed = neededSpeed * 2;
        targetPos.Y = 1;
        return targetPos;
    }
}
