using Godot;
using System;
using System.Collections.Generic;

public partial class RouteDesignManager : Node
{
    public static RouteDesignManager Instance;

    public List<Line2D> lines;
    
    private PlayDesignManager pdm;
    public override void _EnterTree()
    {
        base._EnterTree();
        Instance = this;
    }

    public override void _Ready()
    {
        base._Ready();
        pdm = PlayDesignManager.Instance;
        lines = new List<Line2D>();
    }

    public void SubViewLineRendering(Viewport view, Camera3D oldCam)
    {
        Camera3D cam = view.GetCamera3D();
        
        foreach (var line in lines)
        {
            Line2D newLine = line.Duplicate() as Line2D;
            view.AddChild(newLine);
            for (int i = 0; i < line.Points.Length; i++)
            {
                Vector2 pos = newLine.Points[i];
                Vector3 worldPos = oldCam.ProjectPosition(pos, 1);
                Vector2 newPos = cam.UnprojectPosition(worldPos);
                newLine.SetPointPosition(i, newPos);
            }
        }
    }
    
    public void NewRoute(Vector2 startPoint, int replaceIndex = -1)
    {
        Line2D newLine;
        
        if (replaceIndex == -1)
        {
            newLine = new Line2D();
            AddChild(newLine);
            lines.Add(newLine);
        }
        else
            newLine = lines[replaceIndex];
        
        newLine.ClearPoints();
        newLine.AddPoint(startPoint);
        newLine.AddPoint(startPoint);
        newLine.SetJointMode(Line2D.LineJointMode.Round);
        newLine.SetBeginCapMode(Line2D.LineCapMode.Round);
        newLine.SetEndCapMode(Line2D.LineCapMode.Round);
        newLine.SetDefaultColor(Colors.Red);
        newLine.SetWidth(4.5f);
    }

    public void RemoveRoute(int index)
    {
        lines[index].ClearPoints();
    }
    
    public void UpdateLine(Camera3D cam, Vector3 pos, int index)
    {
        Vector2 viewPos = cam.UnprojectPosition(pos);
        lines[index].SetPointPosition(lines[index].Points.Length - 1, viewPos);
    }

    public void PlacePoint(int index)
    {
        if(lines[index].Points[^1].Floor() != lines[index].Points[^2].Floor())
            lines[index].AddPoint(lines[index].Points[^1]);
    }

    public void EndEdit(int index, PlayerType playerType)
    {
        lines[index].RemovePoint(lines[index].Points.Length - 1);

        Vector2 startPoint = lines[index].Points[^1];
        
        Vector2 dir = lines[index].Points[^2].DirectionTo(startPoint);

        if(playerType == PlayerType.Receiver || playerType == PlayerType.Safety)
        {
            Vector2 top = startPoint + dir * 8;
            Vector2 left = startPoint + ((dir.Orthogonal() * 4) - (dir * 1.0f) );
            Vector2 right = startPoint + ((-dir.Orthogonal() * 4) - (dir * 1.0f) );

            lines[index].AddPoint(left);
            lines[index].AddPoint(top);
            lines[index].AddPoint(right);
            lines[index].AddPoint(startPoint);
        }

        if (playerType == PlayerType.OLineman)
        {
            Vector2 left = startPoint + dir.Orthogonal() * 6;
            Vector2 right = startPoint - dir.Orthogonal() * 6;

            lines[index].AddPoint(left);
            lines[index].AddPoint(right);
        }
    }
}
