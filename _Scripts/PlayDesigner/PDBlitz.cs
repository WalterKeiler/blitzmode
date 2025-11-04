using Godot;
using System;

public partial class PDBlitz : Button
{
    public override void _Pressed()
    {
        base._Pressed();
        PDUIManager.Instance.Blitz();
    }
}
