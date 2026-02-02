using Godot;
using System;

public partial class PDReset : Button
{
    public override void _Pressed()
    {
        base._Pressed();
        PlayDesignManager.Instance.Reset();
    }
}
