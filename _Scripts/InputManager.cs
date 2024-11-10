using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class InputManager : Node
{
	[Export] public int PlayerID;
	[Export] InputCombo[] _inputCombos;
	[Export] float _inputBuffer;
	[Export] public bool isOffence;

	List<InputCombo> activeInputs;
	double deltaTime;
	
	public event Action<PlayerActions> InputPressAction;
	public event Action<PlayerActions> InputReleaseAction;
	public override void _Ready()
	{
		activeInputs = new List<InputCombo>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		deltaTime = delta;
		CustomInputs();
	}

	void CustomInputs()
	{
		for (int i = 0; i < _inputCombos.Length; i++)
		{
			for (int j = 0; j < _inputCombos[i].InputActions.Length; j++)
			{
				if (Input.IsActionJustPressed(_inputCombos[i].InputActions[j]) && (_inputCombos[i].isUniversal || _inputCombos[i].isOffence == isOffence))
				{
					if(_inputCombos[i].PressCount > 1)
					{
						activeInputs.Add(_inputCombos[i]);
						if (activeInputs.FindAll(x => x.InputActions == _inputCombos[i].InputActions).Count ==
						    _inputCombos[i].PressCount)
						{
							InputPressAction?.Invoke(_inputCombos[i].Action);
							_inputCombos[i].isActive = true;
						}
					}
					else
					{
						InputPressAction?.Invoke(_inputCombos[i].Action);
						_inputCombos[i].isActive = true;
					}
				}
				if (Input.IsActionJustReleased(_inputCombos[i].InputActions[j]) && (_inputCombos[i].isUniversal || _inputCombos[i].isOffence == isOffence))
				{
					WaitForMoreInput(_inputCombos[i]);
					if(_inputCombos[i].isActive)
						InputReleaseAction?.Invoke(_inputCombos[i].Action);
				}
			}
		}
	}

	async void WaitForMoreInput(InputCombo inputCombo)
	{
		await ToSignal(GetTree().CreateTimer(_inputBuffer), "timeout");
		activeInputs.Remove(inputCombo);
	}
	
	public Vector3 GetDirectionalInput()
	{
		Vector2 input = Input.GetVector("move_right", "move_left", "move_back", "move_forward");
		
		return new Vector3(input.X,0,input.Y);
	}
}