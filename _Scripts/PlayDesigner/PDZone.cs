using Godot;
using System;

public partial class PDZone : Button
{
    public override void _Pressed()
    {
        base._Pressed();
        PDUIManager.Instance.NewZone();
    }
}
