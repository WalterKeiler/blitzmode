using Godot;
using System;

[GlobalClass]
public partial class Play : Resource
{
    [Export] public string Name;
    [Export] public byte[] Image;
    [Export] public bool IsOffence;
    [Export] public PlayerDataOffence[] PlayerDataOffence;
    [Export] public PlayerDataDefence[] PlayerDataDefence;
}

