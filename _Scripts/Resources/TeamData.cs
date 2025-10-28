using Godot;
using System;

public partial class TeamData : Resource
{
    [Export] public string teamName;
    [Export] public string teamAbreviation;
    [Export(PropertyHint.Range, "0,1,.1,")] public float Passing;
    [Export(PropertyHint.Range, "0,1,.1,")] public float Running;
    [Export(PropertyHint.Range, "0,1,.1,")] public float Linemen;
    [Export(PropertyHint.Range, "0,1,.1,")] public float Defence;
    [Export(PropertyHint.Range, "0,1,.1,")] public float SPTeams;
}
