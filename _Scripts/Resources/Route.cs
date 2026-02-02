using Godot;
using System;

[GlobalClass]
public partial class Route : Resource
{
    [Export] public Vector3[] targetPoints;
    [Export] public int currentIndex = 0;
    [Export] public EndRouteAction endAction;

    Vector3 GetPointByLength(Vector3 currentPos, float length)
    {
        Vector3 routeDir;
        Vector3 endRouteTarget;
        if (currentIndex >= targetPoints.Length - 1)
        {
            float dist = length;
            
            routeDir = targetPoints[^2].DirectionTo(targetPoints[^1]);
        
            endRouteTarget = currentPos +
                             (routeDir * dist);
            if(MathF.Abs(endRouteTarget.Z) < (GameManager.Instance.fieldWidth / 2f) - AIManager.DISTANCE_FROM_SIDELINE)
            {
                return endRouteTarget;
            }
            Vector3 finalPos;
            int side = currentPos.Z <= 0 ? -1 : 1;
            if (MathF.Abs(endRouteTarget.Z) > (GameManager.Instance.fieldWidth / 2f) - AIManager.DISTANCE_FROM_SIDELINE && 
                MathF.Abs(currentPos.Z) <= (GameManager.Instance.fieldWidth / 2f) - (AIManager.DISTANCE_FROM_SIDELINE + 2))
            {
                
                float nz = ((GameManager.Instance.fieldWidth / 2f) - AIManager.DISTANCE_FROM_SIDELINE) * side;
                Vector3 lastRoutePos = GetLOSTargetPoint(targetPoints.Length - 1);
                float nx = (routeDir.X / routeDir.Z) * (nz - lastRoutePos.Z) + lastRoutePos.X;
                Vector3 sidelinePos = new Vector3(nx, 0, nz);
                dist -= currentPos.DistanceTo(sidelinePos);
                finalPos = sidelinePos + (Vector3.Right * PlayManager.Instance.PlayDirection) * dist;
                
                return finalPos;
            }
            
            currentPos.Z = ((GameManager.Instance.fieldWidth / 2f) - AIManager.DISTANCE_FROM_SIDELINE) * side;
            finalPos = currentPos + (Vector3.Right * PlayManager.Instance.PlayDirection) * dist;
            return finalPos;
            
            //return endRouteTarget;
        }
        
        float distanceFromLastPoint =
            GetLOSTargetPoint(Math.Clamp(currentIndex - 1, 0, Int32.MaxValue)).DistanceTo(currentPos);

        if (length < GetLOSTargetPoint(currentIndex).DistanceTo(currentPos))
        {
            Vector3 dir = GetLOSTargetPoint(Math.Clamp(currentIndex - 1, 0, Int32.MaxValue)).DirectionTo(GetLOSTargetPoint(currentIndex));
            
            return currentPos + dir * length;
        }

        float totalDistance = distanceFromLastPoint + length;
        
        for (int i = Math.Clamp(currentIndex - 1, 0, Int32.MaxValue); i < targetPoints.Length - 1; i++)
        {
            float dist = GetLOSTargetPoint(i).DistanceTo(GetLOSTargetPoint(i + 1));
            float tempDist = totalDistance;
            
            tempDist -= dist;

            if (tempDist <= 0)
            {
                Vector3 dir = GetLOSTargetPoint(i).DirectionTo(GetLOSTargetPoint(i + 1));
                return GetLOSTargetPoint(i) + dir * totalDistance;
            }
            totalDistance = tempDist;
        }

        routeDir = targetPoints[^2].DirectionTo(targetPoints[^1]);
        
        endRouteTarget = GetLOSTargetPoint(targetPoints.Length - 1) +
                         (routeDir * totalDistance);

        if (MathF.Abs(endRouteTarget.Z) > (GameManager.Instance.fieldWidth / 2f) - AIManager.DISTANCE_FROM_SIDELINE)
        {
            float nz = (GameManager.Instance.fieldWidth / 2f) - AIManager.DISTANCE_FROM_SIDELINE;
            float nx = (routeDir.X / routeDir.Z) * (nz - GetLOSTargetPoint(targetPoints.Length - 1).Z) +
                       GetLOSTargetPoint(targetPoints.Length - 1).X;
            Vector3 sidelinePos = new Vector3(nx, 0, nz);
            totalDistance -= GetLOSTargetPoint(targetPoints.Length - 1).DistanceTo(sidelinePos);
            Vector3 finalPos = sidelinePos + (Vector3.Right * PlayManager.Instance.PlayDirection) * totalDistance;
            return finalPos;
        }

        return endRouteTarget;
    }
    
    public Vector3 GetThrowToPoint(float distance, Vector3 receiverPos, Vector3 qbPos, float receiverSpeed, ref float ballSpeed)
    {
        //if (currentIndex > targetPoints.Length) return receiverPos;
        float d = qbPos.DistanceTo(receiverPos) + Mathf.Clamp(Ball.BALLHEIGHTMULTIPLIER * distance, 1, 10);
        float time = d / (ballSpeed);

        //time *= delta;
        Vector3 targetPos = receiverPos;
        targetPos = GetPointByLength(receiverPos, time * receiverSpeed);
        distance = qbPos.DistanceTo(targetPos);
        float neededSpeed = (distance + Mathf.Clamp((Ball.BALLHEIGHTMULTIPLIER / ((2.0f / 3) * 20)) * distance, 1, 10)) / time;
        ballSpeed = neededSpeed;
        targetPos.Y = 1;
        return targetPos;
    }

    public Vector3 GetLOSTargetPoint(int i)
    {
        if (i > targetPoints.Length - 1 || i < 0) return targetPoints[0];
        Vector3 pos = targetPoints[i];
        pos.X *= PlayManager.Instance.PlayDirection;
        pos += Vector3.Right * PlayManager.Instance.lineOfScrimmage;

        return pos;
    }
}

public enum EndRouteAction
{
    Continue,
    Block,
    Zone
}