using Godot;
using System;

public partial class PDCancel : Button
{
    public override void _Pressed()
    {
        base._Pressed();
        PDUIManager.Instance.Cancel();
    }
}
