using Godot;
using System;
using System.Collections.Generic;

public partial class PlayDesignManager : Node3D
{
	public static PlayDesignManager Instance;
	
	[Export] public PackedScene playerPrefab;
	[Export] public MeshInstance3D cursorIcon;

	public PlayerType selectedPlayerType;
	public bool playerSelected;

	bool hoveringSelectable;
	bool canPlacePlayer;
	
	Vector3 CursorMinPos = new (-10, -1, -27);
	Vector3 CursorMaxPos = new (30, -1, 27);
	Vector3 cursorPos;
	Material mat;

	PlayDesignSelectable selectedObject;
	List<PlayDesignPlayer> players;

	public List<PlayDesignSelectable> selectableObjects = new List<PlayDesignSelectable>();
	PDUIManager pdUI;
	
	public override void _EnterTree()
	{
		base._EnterTree();
		Instance = this;
	}

	public override void _Ready()
	{
		playerSelected = false;
		pdUI = PDUIManager.Instance;
		players = new List<PlayDesignPlayer>();

		selectedPlayerType = PlayerType.Quarterback;
		cursorIcon.GlobalPosition = new Vector3(-3, 1, 0);
		SpawnNewPlayer();
		selectedPlayerType = PlayerType.OLineman;
		cursorIcon.GlobalPosition = new Vector3(-1, 1, 0);
		SpawnNewPlayer();
		
		mat = (Material)cursorIcon.GetActiveMaterial(0).Duplicate();
		cursorIcon.MaterialOverride = mat;
	}
	
	public override void _Process(double delta)
	{
		canPlacePlayer = true;
		bool light = true;
		bool hover = false;
		foreach (var selectableObject in selectableObjects)
		{
			if (selectableObject.GlobalPosition.DistanceTo(cursorIcon.GlobalPosition) < 1)
			{
				hover = true;
			}
		}

		if (hover != hoveringSelectable)
		{
			canPlacePlayer = false;
			hoveringSelectable = hover;
			light = hover;
		}
		if(cursorIcon.GlobalPosition.X > 0)
		{
			canPlacePlayer = false;
			light = true;
		}
		
		((ShaderMaterial) mat).SetShaderParameter("isSelected", light);
	}

	public override void _Input(InputEvent inEvent)
	{
		Viewport viewport = GetViewport();
		Camera3D camera = viewport.GetCamera3D();
		
		if (inEvent.IsAction("ui_accept") && !playerSelected)
		{
			if(canPlacePlayer)
			{
				playerSelected = true;
				if (camera != null)
				{
					pdUI.SelectPlayerType(camera.UnprojectPosition(cursorPos));
				}
			}
		}
		
		if(playerSelected) return;
		
		if (inEvent is InputEventMouseMotion mouseMotion)
		{

			if (camera != null)
			{
				Vector2 mouseScreenPosition = mouseMotion.Position;

				Vector3 rayOrigin = camera.ProjectRayOrigin(mouseScreenPosition);
				Vector3 rayNormal = camera.ProjectRayNormal(mouseScreenPosition);

				float rayLength = camera.Far;
				Vector3 rayEnd = rayOrigin + rayNormal * rayLength;

				PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
				Godot.Collections.Dictionary result = GetWorld3D().DirectSpaceState.IntersectRay(query);

				if (result.Count > 0)
				{
					cursorPos = (Vector3)result["position"];
				}
				
			}
		}
		if(inEvent.IsAction("move_left") || inEvent.IsAction("move_right"))
        {
        	if (inEvent is InputEventJoypadButton joyEvent)
        	{
		        if (inEvent.IsAction("move_left") && joyEvent.Pressed)
		        {
			        cursorPos.Z += -.5f;
		        }
		        else
		        {
			        cursorPos.Z += 0;
		        }
  
		        if (inEvent.IsAction("move_right") && joyEvent.Pressed)
		        {
			        cursorPos.Z += .5f;
		        }
		        else
		        {
			        cursorPos.Z += 0;
		        }
        	}
  
        	if (inEvent is InputEventKey keyEvent)
        	{
        		if (inEvent.IsAction("move_left") && keyEvent.Pressed)
        		{
        			cursorPos.Z += -.5f;
        		}
        		else
        		{
        			cursorPos.Z += 0;
        		}
  
        		if (inEvent.IsAction("move_right") && keyEvent.Pressed)
        		{
        			cursorPos.Z += .5f;
        		}
        		else
        		{
        			cursorPos.Z += 0;
        		}
        	}
        }
  
        if (inEvent.IsAction("move_back") || inEvent.IsAction("move_forward"))
        {
        	if (inEvent is InputEventJoypadButton joyEvent)
        	{
		        if (inEvent.IsAction("move_back") && joyEvent.Pressed)
		        {
			        cursorPos.X += -.5f;
		        }
		        else
		        {
			        cursorPos.X += 0;
		        }
  
		        if (inEvent.IsAction("move_forward") && joyEvent.Pressed)
		        {
			        cursorPos.X += .5f;
		        }
		        else
		        {
			        cursorPos.X += 0;
		        }
        	}
        	
        	if (inEvent is InputEventKey keyEvent)
        	{
        		if (inEvent.IsAction("move_back") && keyEvent.Pressed)
        		{
        			cursorPos.X += -.5f;
        		}
        		else
        		{
        			cursorPos.X += 0;
        		}
  
        		if (inEvent.IsAction("move_forward") && keyEvent.Pressed)
        		{
        			cursorPos.X += .5f;
        		}
        		else
        		{
        			cursorPos.X += 0;
        		}
        	}
        	
        }
        
        cursorPos *= 2;
        cursorPos = cursorPos.Round() / 2;
        cursorPos = cursorPos.Clamp(CursorMinPos, CursorMaxPos);
        cursorIcon.GlobalPosition = new Vector3(cursorPos.X, 1, cursorPos.Z);
	}
	
	public void SpawnNewPlayer()
	{
		PlayDesignPlayer player = (PlayDesignPlayer)playerPrefab.Instantiate<Node3D>();
		player.Name = "PlayDesignPlayer" + selectedPlayerType;
		AddChild(player);
		player.GlobalPosition = cursorIcon.GlobalPosition;
		player.playerType = selectedPlayerType;
		
		player.Init();
		
		players.Add(player);
		playerSelected = false;
	}
}
