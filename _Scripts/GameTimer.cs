using Godot;
using System;

public partial class GameTimer : Label
{
    private GameManager gm;
    private PlayManager pm;

    string[] suffixLookup = {"st","nd","rd","th"};
    
    public override void _Ready()
    {
        base._Ready();
        gm = GameManager.Instance;
        pm = PlayManager.Instance;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        int min = Mathf.FloorToInt(pm.quarterTimer / 60);
        int sec = Mathf.FloorToInt(pm.quarterTimer) - (min * 60);
        int suffix = Mathf.Clamp(pm.quarterNumber - 1, 0, suffixLookup.Length - 1);
        Text = $"{min}:{sec:D2}\n{pm.quarterNumber}{suffixLookup[suffix]}";
    }
}
