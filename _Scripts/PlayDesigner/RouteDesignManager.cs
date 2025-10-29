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
    }

    public void UpdateLine(Vector2 pos, int index)
    {
        lines[index].SetPointPosition(lines[index].Points.Length - 1, pos);
    }

    public void PlacePoint(int index)
    {
        if(lines[index].Points[^1].Floor() != lines[index].Points[^2].Floor())
            lines[index].AddPoint(lines[index].Points[^1]);
    }

    public void EndEdit(int index)
    {
        lines[index].RemovePoint(lines[index].Points.Length - 1);
        lines[index].RemovePoint(lines[index].Points.Length - 1);
    }
}
