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
	
	List<InputCombo> activeInputs;
	double deltaTime;

	bool keyChanged = false;

	private bool canTakeInput = false;
	
	public static event Action<PlayerActions, int, bool, bool> InputPressAction;
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

	void Start()
	{
		canTakeInput = true;
	}

	void Stop(bool b)
	{
		canTakeInput = false;
	}
	
	public override void _Ready()
	{
		activeInputs = new List<InputCombo>();
		
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
			if ((Input.IsJoyButtonPressed(PlayerID, _inputCombos[i].InputActionsJoy) || Input.IsKeyPressed(_inputCombos[i].InputActionsKey)) && (_inputCombos[i].isUniversal || _inputCombos[i].isOffence == isOffence))
			{
				//if(Input.Ac)
				if(_inputCombos[i].PressCount > 1)
				{
					activeInputs.Add(_inputCombos[i]);
					if (activeInputs.FindAll(x => x.InputActions == _inputCombos[i].InputActions).Count ==
					    _inputCombos[i].PressCount)
					{
						InputPressAction?.Invoke(_inputCombos[i].Action, PlayerID, false, true);
						_inputCombos[i].isActive = true;
					}
				}
				else
				{
					InputPressAction?.Invoke(_inputCombos[i].Action, PlayerID, false, true);
					_inputCombos[i].isActive = true;
				}
			}
		}

		for (int i = 0; i < activeInputs.Count; i++)
		{
			if(!(Input.IsJoyButtonPressed(PlayerID, activeInputs[i].InputActionsJoy) && !Input.IsKeyPressed(activeInputs[i].InputActionsKey)) && (_inputCombos[i].isUniversal || _inputCombos[i].isOffence == isOffence))
			{
				WaitForMoreInput(activeInputs[i]);
				if(activeInputs[i].isActive)
					InputReleaseAction?.Invoke(activeInputs[i].Action, PlayerID, false, true);
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