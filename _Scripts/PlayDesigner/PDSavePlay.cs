using Godot;
using System;

public partial class PDSavePlay : Button
{
    public override void _Pressed()
    {
        base._Pressed();
        PlayDesignManager.Instance.SavePlay();
    }
}
