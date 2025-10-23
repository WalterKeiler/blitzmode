using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class InputManager : Node
{
	[Export] public int PlayerID;
	[Export] InputCombo[] _inputCombos;
	[Export] float _inputBuffer;
	[Export] public bool isOffence;

	bool isFirstController;
	
	List<InputCombo> activeInputs;
	double deltaTime;

	public Vector3 directionalInput;
	
	public static event Action<PlayerActions, int, bool, bool> InputPressAction;
	public static event Action<PlayerActions, int, bool, bool> InputReleaseAction;
	public override void _Ready()
	{
		activeInputs = new List<InputCombo>();
		isFirstController = (PlayerID == 1);
		directionalInput = Vector3.Zero;
		
		GD.Print(Input.GetConnectedJoypads().Count);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		deltaTime = delta;
		CustomInputs();
	}

	public override void _Input(InputEvent iEvent)
	{
		base._Input(iEvent);
		
		
		if(iEvent.GetDevice() == PlayerID)
		{
			if(iEvent.IsAction("move_left") || iEvent.IsAction("move_right"))
			{
				if (iEvent is InputEventJoypadMotion joyEvent)
				{
					directionalInput.X = -joyEvent.GetAxisValue();
				}

				if (iEvent is InputEventKey keyEvent)
				{
					if (iEvent.IsAction("move_left") && keyEvent.Pressed)
					{
						directionalInput.X = -1;
					}
					else
					{
						directionalInput.X = 0;
					}

					if (iEvent.IsAction("move_right") && keyEvent.Pressed)
					{
						directionalInput.X = 1;
					}
					else
					{
						directionalInput.X = 0;
					}
				}
			}

			if (iEvent.IsAction("move_back") || iEvent.IsAction("move_forward"))
			{
				if (iEvent is InputEventJoypadMotion joyEvent)
				{
					directionalInput.Z = -joyEvent.GetAxisValue();
				}
				
				if (iEvent is InputEventKey keyEvent)
				{
					if (iEvent.IsAction("move_back") && keyEvent.Pressed)
					{
						directionalInput.Z = -1;
					}
					else
					{
						directionalInput.Z = 0;
					}

					if (iEvent.IsAction("move_forward") && keyEvent.Pressed)
					{
						directionalInput.Z = 1;
					}
					else
					{
						directionalInput.Z = 0;
					}
				}
				
			}
			
			
		}
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
				if (Input.IsActionJustReleased(_inputCombos[i].InputActions[j]) && (_inputCombos[i].isUniversal || _inputCombos[i].isOffence == isOffence))
				{
					WaitForMoreInput(_inputCombos[i]);
					if(_inputCombos[i].isActive)
						InputReleaseAction?.Invoke(_inputCombos[i].Action, PlayerID, false, true);
				}
			}
		}
	}

	async void WaitForMoreInput(InputCombo inputCombo)
	{
		await ToSignal(GetTree().CreateTimer(_inputBuffer), "timeout");
		activeInputs.Remove(inputCombo);
	}
	
	// public Vector3 GetDirectionalInput()
	// {
	// 	bool canMove = false;
	// 	
	// 	Vector2 input = directionalInput;
	// 	
	// 	
	// 	//if(!canMove) return Vector3.Zero;
	// 	//Vector2 input = Input.GetVector("move_right", "move_left", "move_back", "move_forward");
	// 	return new Vector3(input.X,0,input.Y);
	// }
	
	public Vector3 GetDirectionalInput()
	{
		bool canMove = false;
		float left = directionalInput.X;
		float right = directionalInput.X;
		float forw = directionalInput.Z;
		float back = directionalInput.Z;
		foreach (var inEvent in InputMap.ActionGetEvents("move_right"))
		{
			if (inEvent.GetDevice() == PlayerID)
			{
				// InputEventJoypadMotion joyEvent = (InputEventJoypadMotion) inEvent;
				//
				// right = joyEvent.AxisValue;
				
				// string e = inEvent.AsText();
				//
				// float val = 0;
				//
				// val += Convert.ToInt32(e[^1]) * .01f;
				// val += Convert.ToInt32(e[^2]) * .1f;
				// val += Convert.ToInt32(e[^4]) * 1f;
				// if (e[^5] == "-"[0]) val *= -1;
				//
				// right = val;
				break;
			}
		}
		foreach (var inEvent in InputMap.ActionGetEvents("move_left"))
		{
			if (inEvent.GetDevice() == PlayerID)
			{
				//((InputEventKey)inEvent).
				if(inEvent is InputEventJoypadMotion joyEvent)
				{
					GD.Print("FUck");
					left = joyEvent.GetAxisValue();
				}
				
				// string e = inEvent.AsText();
				//
				// float val = 0;
				//
				// val += Convert.ToInt32(e[^1]) * .01f;
				// val += Convert.ToInt32(e[^2]) * .1f;
				// val += Convert.ToInt32(e[^4]) * 1f;
				// if (e[^5] == "-"[0]) val *= -1;
				//
				// left = val;
				break;
			}
		}
		foreach (var inEvent in InputMap.ActionGetEvents("move_back"))
		{
			if (inEvent.GetDevice() == PlayerID)
			{
				// InputEventJoypadMotion joyEvent = (InputEventJoypadMotion) inEvent;
				//
				// back = joyEvent.AxisValue;
				
				// string e = inEvent.AsText();
				//
				// float val = 0;
				//
				// val += (int)e[^1] * .01f;
				// val += (int)e[^2] * .1f;
				// val += (int)e[^4] * 1f;
				// if (e[^5] == "-"[0]) val *= -1;
				//
				// forw = val;
				break;
			}
		}
		foreach (var inEvent in InputMap.ActionGetEvents("move_forward"))
		{
			if (inEvent.GetDevice() == PlayerID)
			{
				if(inEvent is InputEventJoypadMotion joyEvent)
				{
					forw = joyEvent.AxisValue;
				}
				
				// string e = inEvent.AsText();
				//
				// float val = 0;
				//
				// val += (int)e[^1] * .01f;
				// val += (int)e[^2] * .1f;
				// val += (int)e[^4] * 1f;
				// if (e[^5] == "-"[0]) val *= -1;
				//
				// back = val;
				break;
			}
		}

		directionalInput = new Vector3(left, 0, forw);
		GD.Print(directionalInput);
		//Vector2 input = Input.GetVector("move_right", "move_left", "move_back", "move_forward");
		return directionalInput;
		//return new Vector3(input.X,0,input.Y);
	}
}