using Godot;

[GlobalClass]
public partial class PlayerStats : Resource
{
    [Export] public bool canBeThrowTarget;
    [Export(PropertyHint.Range, "0,10,.5,")] public float Speed;
    [Export(PropertyHint.Range, "0,10,.5,")] public float Endurance;
    [Export(PropertyHint.Range, "0,10,.5,")] public float Strength;
    [Export(PropertyHint.Range, "0,10,.5,")] public float Catching;
    [Export(PropertyHint.Range, "0,10,.5,")] public float Agility;
}