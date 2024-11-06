using Godot;
using System;

public partial class InputManager : Node
{
	[Export] public int PlayerID;
	[Export] InputCombo[] _inputCombos;
	
	public event Action<string> InputAction;
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
			for (int j = 0; j < _inputCombos[i].key.Length; j++)
				if (_inputCombos[i].key[j].IsPressed())
				{
					InputAction?.Invoke(_inputCombos[i].ResourceName);
				}
		}
	}
	
	public Vector3 GetDirectionalInput()
	{
		Vector3 inputDir = new Vector3();
		if (Input.IsKeyPressed(Key.W))
		{
			inputDir.Z = 1;
		}
		else if (Input.IsKeyPressed(Key.S))
		{
			inputDir.Z = -1;
		}
		else
		{
			inputDir.Z = 0;
		}
		
		if (Input.IsKeyPressed(Key.A))
		{
			inputDir.X = 1;
		}
		else if (Input.IsKeyPressed(Key.D))
		{
			inputDir.X = -1;
		}
		else
		{
			inputDir.X = 0;
		}

		return inputDir;
	}
}