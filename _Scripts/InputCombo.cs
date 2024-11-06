using Godot;
using System;

public partial class InputCombo : Resource
{
    [Export] public InputEvent[] key;
    [Export] public int pressCount;
}
