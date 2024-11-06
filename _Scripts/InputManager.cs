using Godot;
using System;

public partial class InputManager : Node
{
	[Export] public int PlayerID;
	[Export] InputCombo[] _inputCombos;
	
	public event Action<PlayerActions> InputPressAction;
	public event Action<PlayerActions> InputReleaseAction;
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		CustomInputs();
	}

	void CustomInputs()
	{
		for (int i = 0; i < _inputCombos.Length; i++)
		{
			for (int j = 0; j < _inputCombos[i].InputActions.Length; j++)
			{
				if (Input.IsActionJustPressed(_inputCombos[i].InputActions[j]))
				{
					InputPressAction?.Invoke(_inputCombos[i].Action);
				}
				if (Input.IsActionJustReleased(_inputCombos[i].InputActions[j]))
				{
					InputReleaseAction?.Invoke(_inputCombos[i].Action);
				}
			}
		}
	}
	
	public Vector3 GetDirectionalInput()
	{
		Vector2 input = Input.GetVector("move_right", "move_left", "move_back", "move_forward");
		
		return new Vector3(input.X,0,input.Y);
	}
}