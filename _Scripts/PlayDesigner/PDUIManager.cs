using Godot;
using System;

public partial class PDUIManager : Control
{
    public static PDUIManager Instance;
    [Export] public Button selectRoute;
    [Export] public Control selectedPlayerUIOff;
    
    [Export] public Button selectBlitz;
    [Export] public Button selectZone;
    [Export] public Control selectedPlayerUIDef;

    [Export] public LineEdit name;
    
    bool selectPosition;
    PlayDesignManager pdm;

    public override void _EnterTree()
    {
        base._EnterTree();
        Instance = this;
    }

    public override void _Ready()
    {
        selectRoute.Disabled = true;
        selectBlitz.Disabled = true;
        selectZone.Disabled = true;
        selectedPlayerUIOff.Visible = false;
        selectedPlayerUIDef.Visible = false;
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
            selectRoute.Disabled = false;
            selectedPlayerUIOff.Position = cursorPos - (selectedPlayerUIOff.Size / 2);
        }
        else
        {
            selectZone.Disabled = false;
            selectBlitz.Disabled = false;
            selectedPlayerUIDef.Visible = true;
            selectedPlayerUIDef.Position = cursorPos - (selectedPlayerUIDef.Size / 2);
        }
    }
    
    public void SelectPlayerType(Vector2 cursorPos)
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
    
    
    public void SelectionMade()
    {
        selectRoute.Disabled = true;
        selectBlitz.Disabled = true;
        selectZone.Disabled = true;
        selectedPlayerUIOff.Visible = false;
        selectedPlayerUIDef.Visible = false;
        
        pdm.SpawnNewPlayer();
    }

    public void Cancel()
    {
        pdm.playerSelected = false;
        TurnOffAll();
    }
    
    public void TurnOffAll()
    {
        selectRoute.Disabled = true;
        selectBlitz.Disabled = true;
        selectZone.Disabled = true;
        selectedPlayerUIOff.Visible = false;

        selectedPlayerUIDef.Visible = false;
    }

    public void Blitz()
    {
        selectedPlayerUIDef.Visible = false;
        selectRoute.Disabled = true;
        selectBlitz.Disabled = true;
        selectZone.Disabled = true;
        pdm.Blitz();
    }
    
    public void NewZone()
    {
        selectedPlayerUIDef.Visible = false;
        selectRoute.Disabled = true;
        selectBlitz.Disabled = true;
        selectZone.Disabled = true;
        pdm.MakeNewZone();
    }
    
    public void NewRoute()
    {
        selectedPlayerUIOff.Visible = false;
        selectRoute.Disabled = true;
        selectBlitz.Disabled = true;
        selectZone.Disabled = true;
        pdm.MakeNewRoute();
    }
}
