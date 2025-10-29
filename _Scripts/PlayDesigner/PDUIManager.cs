using Godot;
using System;

public partial class PDUIManager : Control
{
    public static PDUIManager Instance;
    [Export] public Control selectPlayerType;

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
        selectPosition = true;
        pdm = PlayDesignManager.Instance;
        base._Ready();
    }

    // public override void _Input(InputEvent inEvent)
    // {
    //     base._Input(inEvent);
    //     
    // }

    public void SelectPlayerType(Vector2 cursorPos)
    {
        selectPosition = false;
        selectPlayerType.Visible = true;
        selectPlayerType.Position = cursorPos - (selectPlayerType.Size / 2);
    }
    
    public void SelectionMade()
    {
        selectPosition = false;
        selectPlayerType.Visible = false;
        pdm.SpawnNewPlayer();
    }
}
