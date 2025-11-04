using Godot;
using System;

public partial class PDUIManager : Control
{
    public static PDUIManager Instance;
    [Export] public Control selectPlayerTypeOff;
    [Export] public Control selectedPlayerUIOff;
    
    [Export] public Control selectPlayerTypeDef;
    [Export] public Control selectedPlayerUIDef;

    bool selectPosition;
    PlayDesignManager pdm;

    public override void _EnterTree()
    {
        base._EnterTree();
        Instance = this;
    }

    public override void _Ready()
    {
        selectPlayerTypeOff.Visible = false;
        selectedPlayerUIOff.Visible = false;
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
        if(pdm.isOffencePlay)
        {
            selectedPlayerUIOff.Visible = true;
            selectedPlayerUIOff.Position = cursorPos - (selectedPlayerUIOff.Size / 2);
        }
        else
        {
            selectedPlayerUIDef.Visible = true;
            selectedPlayerUIDef.Position = cursorPos - (selectedPlayerUIDef.Size / 2);
        }
    }
    
    public void SelectPlayerType(Vector2 cursorPos)
    {
        if(pdm.isOffencePlay)
        {
            selectPlayerTypeOff.Visible = true;
            selectPlayerTypeOff.Position = cursorPos - (selectPlayerTypeOff.Size / 2);
        }
        else
        {
            selectPlayerTypeDef.Visible = true;
            selectPlayerTypeDef.Position = cursorPos - (selectPlayerTypeDef.Size / 2);
        }
    }
    
    
    public void SelectionMade()
    {
        selectPlayerTypeOff.Visible = false;
        selectedPlayerUIOff.Visible = false;

        selectPlayerTypeDef.Visible = false;
        selectedPlayerUIDef.Visible = false;
        
        pdm.SpawnNewPlayer();
    }

    public void TurnOffAll()
    {
        selectPlayerTypeOff.Visible = false;
        selectedPlayerUIOff.Visible = false;

        selectPlayerTypeDef.Visible = false;
        selectedPlayerUIDef.Visible = false;
    }

    public void Blitz()
    {
        selectedPlayerUIDef.Visible = false;
        pdm.Blitz();
    }
    
    public void NewZone()
    {
        selectedPlayerUIDef.Visible = false;
        pdm.MakeNewZone();
    }
    
    public void NewRoute()
    {
        selectedPlayerUIOff.Visible = false;
        pdm.MakeNewRoute();
    }
}
