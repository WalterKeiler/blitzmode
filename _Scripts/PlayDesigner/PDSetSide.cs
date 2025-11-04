using Godot;
using System;

public partial class PDSetSide : Button
{
    [Export] private bool isOffence;
    public override void _Pressed()
    {
        base._Pressed();
        PlayDesignManager.Instance.Reset();
        PlayDesignManager.Instance.Init(isOffence);
    }
}
