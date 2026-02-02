using Godot;
using System;

public partial class PDPlayName : LineEdit
{
    private string name;
    private bool editing = false;
    public override void _EnterTree()
    {
        base._EnterTree();
        TextChanged += UpdateText;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        TextChanged -= UpdateText;
    }

    void UpdateText(string text)
    {
        PlayDesignManager.Instance.playName = text;
    }
}
