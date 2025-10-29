using Godot;
using System;

public partial class PDUIManager : Control
{
    public static PDUIManager Instance;
    [Export] public Control selectPlayerType;
    [Export] public Control selectedPlayerUI;

    bool selectPosition;
    PlayDesignManager pdm;

    public override void _EnterTree()
    {
        base._EnterTree();
        Instance = this;
    }

    public override void _Ready()
    {
        selectPlayerType.Visible = false;
        selectedPlayerUI.Visible = false;
        pdm = PlayDesignManager.Instance;
        base._Ready();
    }

    // public override void _Input(InputEvent inEvent)
    // {
    //     base._Input(inEvent);
    //     
    // }

    public void SelectPlayer(Vector2 cursorPos)
    {
        selectedPlayerUI.Visible = true;
        selectedPlayerUI.Position = cursorPos - (selectPlayerType.Size / 2);
    }
    
    public void SelectPlayerType(Vector2 cursorPos)
    {
        selectPlayerType.Visible = true;
        selectPlayerType.Position = cursorPos - (selectPlayerType.Size / 2);
    }
    
    public void SelectionMade()
    {
        selectPlayerType.Visible = false;
        pdm.SpawnNewPlayer();
    }

    public void NewRoute()
    {
        selectedPlayerUI.Visible = false;
        pdm.MakeNewRoute();
    }
}
