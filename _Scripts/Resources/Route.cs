using Godot;
using System;

[GlobalClass]
public partial class Route : Resource
{
    [Export] public Vector3[] targetPoints;
    [Export] public int currentIndex = 0;

    Vector3 GetPointByLength(Vector3 currentPos, float length)
    {
        if (currentIndex >= targetPoints.Length) return currentPos;
        
        float distanceFromLastPoint =
            targetPoints[Math.Clamp(currentIndex - 1, 0, Int32.MaxValue)].DistanceTo(currentPos);

        if (length < targetPoints[currentIndex].DistanceTo(currentPos))
        {
            Vector3 dir = targetPoints[Math.Clamp(currentIndex - 1, 0, Int32.MaxValue)].DirectionTo(targetPoints[currentIndex]);
            
            return currentPos + dir * length;
        }

        float totalDistance = distanceFromLastPoint + length;
        
        for (int i = Math.Clamp(currentIndex - 1, 0, Int32.MaxValue); i < targetPoints.Length - 1; i++)
        {
            float dist = targetPoints[i].DistanceTo(targetPoints[i + 1]);
            float tempDist = totalDistance;
            
            tempDist -= dist;

            if (tempDist <= 0)
            {
                Vector3 dir = targetPoints[i].DirectionTo(targetPoints[i + 1]);
                return targetPoints[i] + dir * totalDistance;
            }
            totalDistance = tempDist;
        }

        return targetPoints[^1];
    }
    
    public Vector3 GetThrowToPoint(float distance, Vector3 receiverPos, Vector3 qbPos, float receiverSpeed, ref float ballSpeed)
    {
        if (currentIndex > targetPoints.Length) return receiverPos;
        float d = qbPos.DistanceTo(receiverPos) + Mathf.Clamp(Ball.BALLHEIGHTMULTIPLIER * distance, 1, 10);
        float time = d / (ballSpeed);

        //time *= delta;
        Vector3 targetPos = receiverPos;
        if(currentIndex < targetPoints.Length) targetPos = GetPointByLength(receiverPos, time * receiverSpeed);
        distance = qbPos.DistanceTo(targetPos);
        float neededSpeed = (distance + Mathf.Clamp((Ball.BALLHEIGHTMULTIPLIER / ((2.0f / 3) * 10)) * distance, 1, 10)) / time;
        ballSpeed = neededSpeed;
        targetPos.Y = 1;
        return targetPos;
    }
}
