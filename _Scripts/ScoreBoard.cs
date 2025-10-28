using Godot;
using System;

public partial class ScoreBoard : Label
{
    [Export] public bool isTeam1;

    private string teamName;
    
    private GameManager gm;
    private PlayManager pm;

    public override void _EnterTree()
    {
        base._EnterTree();
        PlayManager.UpdateScore += UpdateScoreBoard;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        PlayManager.UpdateScore -= UpdateScoreBoard;
    }

    public override void _Ready()
    {
        base._Ready();
        gm = GameManager.Instance;
        pm = PlayManager.Instance;

        teamName = isTeam1 ? gm.team1.teamAbreviation : gm.team2.teamAbreviation;
        UpdateScoreBoard();
    }


    void UpdateScoreBoard()
    {
        int score = isTeam1 ? pm.ScoreTeam1 : pm.ScoreTeam2;
        Text = $"{teamName} {score}";
    }
}
