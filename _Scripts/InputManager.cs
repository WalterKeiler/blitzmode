using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class InputManager : Node
{
	public static float JOYSTICKDEADZONE = .1f;
	
	[Export] public int PlayerID;
	[Export] InputCombo[] _inputCombos;
	[Export] float _inputBuffer;
	[Export] public bool isOffence;

	bool[] inputPressed;
	
	double deltaTime;

	bool keyChanged = false;

	private bool canTakeInput = false;
	
	public static event Action<PlayerActions, int, bool, bool, bool> InputPressAction;
	public static event Action<PlayerActions, int, bool, bool> InputReleaseAction;

	public override void _EnterTree()
	{
		base._EnterTree();
		PlayManager.InitPlay += Start;
		PlayManager.EndPlay -= Stop;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		PlayManager.InitPlay += Start;
		PlayManager.EndPlay -= Stop;
	}

	void Start(bool isSpecialTeams)
	{
		canTakeInput = true;
	}

	void Stop(bool b)
	{
		canTakeInput = false;
	}
	
	public override void _Ready()
	{
		for (int i = 0; i < _inputCombos.Length; i++)
		{
			_inputCombos[i] = (InputCombo) _inputCombos[i].Duplicate();
		}
		
		inputPressed = new bool[_inputCombos.Length];
		
		GD.Print(Input.GetConnectedJoypads().Count);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!canTakeInput) return;
		deltaTime = delta;
		CustomInputs();
	}

	void CustomInputs()
	{
		for (int i = 0; i < _inputCombos.Length; i++)
		{

			bool isPrimaryInput = Input.IsJoyButtonPressed(PlayerID, _inputCombos[i].InputActionsJoy) ||
			                      Input.IsKeyPressed(_inputCombos[i].InputActionsKey);
			bool isSecondaryInput = Input.IsJoyButtonPressed(PlayerID, _inputCombos[i].SecondaryInputActionsJoy) ||
			                        Input.IsKeyPressed(_inputCombos[i].SecondaryInputActionsKey);
			
			if ((isPrimaryInput || isSecondaryInput)
			    && (_inputCombos[i].isUniversal || _inputCombos[i].isOffence == isOffence))
			{
				if(_inputCombos[i].PressCount > 1 && !inputPressed[i])
				{
					_inputCombos[i].isActive = true;
					_inputCombos[i].currentPress++;
					inputPressed[i] = true;
					if (_inputCombos[i].currentPress == _inputCombos[i].PressCount)
					{
						InputPressAction?.Invoke(_inputCombos[i].Action, PlayerID, false, true, isSecondaryInput);
						_inputCombos[i].isActive = true;
					}
				}
				else if(!inputPressed[i])
				{
					//GD.Print("Add: " + _inputCombos[i].Action);
					InputPressAction?.Invoke(_inputCombos[i].Action, PlayerID, false, true, isSecondaryInput);
					_inputCombos[i].isActive = true;
					_inputCombos[i].currentPress++;
					inputPressed[i] = true;
					_inputCombos[i].isActive = true;
				}
			}
			
			if(!isPrimaryInput && !isSecondaryInput && (_inputCombos[i].isUniversal || _inputCombos[i].isOffence == isOffence) && inputPressed[i])
			{
				WaitForMoreInput(i);
			}
		}
		
	}

	async void WaitForMoreInput(int inputCombo)
	{
		inputPressed[inputCombo] = false;
		
		await ToSignal(GetTree().CreateTimer(_inputBuffer), "timeout");
		
		bool isPrimaryInput = Input.IsJoyButtonPressed(PlayerID, _inputCombos[inputCombo].InputActionsJoy) ||
		                      Input.IsKeyPressed(_inputCombos[inputCombo].InputActionsKey);
		bool isSecondaryInput = Input.IsJoyButtonPressed(PlayerID, _inputCombos[inputCombo].SecondaryInputActionsJoy) ||
		                        Input.IsKeyPressed(_inputCombos[inputCombo].SecondaryInputActionsKey);
		
		if(!isPrimaryInput && !isSecondaryInput && (_inputCombos[inputCombo].isUniversal || _inputCombos[inputCombo].isOffence == isOffence))
		{
			InputReleaseAction?.Invoke(_inputCombos[inputCombo].Action, PlayerID, false, true);
			_inputCombos[inputCombo].isActive = false;
			_inputCombos[inputCombo].currentPress = 0;
		}


		//activeInputs[inputCombo].isActive = false;
	}
	
	
	public Vector3 GetDirectionalInput()
	{
		if(!canTakeInput) return Vector3.Zero;
		bool canMove = false;
		Vector3 input = Vector3.Zero;

		input.X = Mathf.Abs(Input.GetJoyAxis(PlayerID, JoyAxis.LeftX)) > JOYSTICKDEADZONE
			? -Input.GetJoyAxis(PlayerID, JoyAxis.LeftX) : 0;
		input.Z = Mathf.Abs(Input.GetJoyAxis(PlayerID, JoyAxis.LeftY)) > JOYSTICKDEADZONE
			? -Input.GetJoyAxis(PlayerID, JoyAxis.LeftY) : 0;

		if (PlayerID != 0) return input;
		if (Input.IsKeyPressed(Key.W)) input.Z = 1;
		if (Input.IsKeyPressed(Key.S)) input.Z = -1;
		if (Input.IsKeyPressed(Key.A)) input.X = 1;
		if (Input.IsKeyPressed(Key.D)) input.X = -1;
		
		return input;
	}
}