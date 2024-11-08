using Godot;
using System;

public partial class InputCombo : Resource
{
    [Export] public PlayerActions Action;
    [Export] public string[] InputActions;
    [Export] public int PressCount = 1;
    [Export] public bool isOffence;
    [Export] public bool isUniversal;
    public bool isActive = false;
}
