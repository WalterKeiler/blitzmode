using Godot;
using System;

public partial class PDNewRoute : Button
{

    public override void _Pressed()
    {
        base._Pressed();
        PDUIManager.Instance.NewRoute();
    }
}
