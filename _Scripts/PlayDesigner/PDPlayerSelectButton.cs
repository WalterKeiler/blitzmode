using Godot;
using System;

public partial class PDPlayerSelectButton : Button
{
    [Export] private PlayerType playerType;

    public override void _Pressed()
    {
        base._Pressed();
        GD.Print("Spawn: " + playerType);
        PlayDesignManager.Instance.selectedPlayerType = playerType;
        PDUIManager.Instance.SelectionMade();
    }
}
