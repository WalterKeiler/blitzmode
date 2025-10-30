using Godot;
using System;
using System.Collections.Generic;

public partial class PlayDesignManager : Node3D
{
	public static PlayDesignManager Instance;

	[Export] public Play play;
	
	[Export] public PackedScene playerPrefab;
	[Export] public MeshInstance3D cursorIcon;

	[Export] Camera3D mainCamera;
	[Export] SubViewport sCamera;
	
	[Export] private PlayerStats[] playerTypes;

	public PlayerType selectedPlayerType;
	public bool playerSelected;

	public string playName;
	
	bool isOffencePlay = true;
	bool hoveringSelectable;
	bool canPlacePlayer;
	bool editingRoute;
	
	Vector3 CursorMinPos = new (-10, -1, -27);
	Vector3 CursorMaxPos = new (30, -1, 27);
	Vector3 cursorPos;
	Material mat;

	private List<Vector3> currentRoute;
	
	PlayDesignSelectable selectedObject;
	PlayDesignSelectable hoveredObject;
	List<PlayDesignPlayer> players;

	public List<PlayDesignSelectable> selectableObjects = new List<PlayDesignSelectable>();
	PDUIManager pdUI;
	RouteDesignManager rdm;
	
	public override void _EnterTree()
	{
		base._EnterTree();
		Instance = this;
	}

	public override void _Ready()
	{
		playerSelected = false;
		pdUI = PDUIManager.Instance;
		rdm = RouteDesignManager.Instance;
		players = new List<PlayDesignPlayer>();
		
		selectedPlayerType = PlayerType.Quarterback;
		cursorIcon.GlobalPosition = new Vector3(-3, 1, 0);
		SpawnNewPlayer();
		players[0].playerData.IsPlayer = true;
		players[0].canBeEditied = false;
		
		selectedPlayerType = PlayerType.OLineman;
		cursorIcon.GlobalPosition = new Vector3(-1, 1, 0);
		SpawnNewPlayer();
		players[1].canBeEditied = false;
		
		mat = (Material)cursorIcon.GetActiveMaterial(0).Duplicate();
		cursorIcon.MaterialOverride = mat;
	}
	
	public override void _Process(double delta)
	{
		canPlacePlayer = true;
		bool light = false;
		bool hover = false;
		foreach (var selectableObject in selectableObjects)
		{
			if (selectableObject.GlobalPosition.DistanceTo(cursorIcon.GlobalPosition) < 1)
			{
				hover = true;
				light = true;
				hoveredObject = selectableObject;
			}
		}

		if (hover != hoveringSelectable)
		{
			canPlacePlayer = !hover;
			hoveringSelectable = hover;
		}
		if(cursorIcon.GlobalPosition.X >= 0)
		{
			canPlacePlayer = false;
			light = true;
		}
		((ShaderMaterial) mat).SetShaderParameter("isSelected", light);

		if (editingRoute)
		{
			rdm.UpdateLine(mainCamera, cursorIcon.GlobalPosition, ((PlayDesignPlayer) selectedObject).routeIndex);
			((ShaderMaterial) mat).SetShaderParameter("isSelected", false);
		}
		
	}

	public override void _Input(InputEvent inEvent)
	{
		if (inEvent.IsAction("ui_accept") && !playerSelected)
		{
			if(canPlacePlayer && !hoveringSelectable && !editingRoute)
			{
				selectedObject = null;
				playerSelected = true;
				if (mainCamera != null)
				{
					pdUI.SelectPlayerType(mainCamera.UnprojectPosition(cursorPos));
				}
			}

			if (hoveringSelectable && !editingRoute)
			{
				if(!hoveredObject.canBeEditied) return;
				
				selectedObject = hoveredObject;
				if (selectedObject is PlayDesignPlayer)
				{
					playerSelected = true;
					if (mainCamera != null) pdUI.SelectPlayer(mainCamera.UnprojectPosition(cursorPos));
				}
			}
			
			if(editingRoute)
			{
				if(currentRoute.Count < 1 || cursorIcon.GlobalPosition.Floor() != currentRoute[^1].Floor())
				{
					rdm.PlacePoint((((PlayDesignPlayer) selectedObject)!).routeIndex);
					currentRoute.Add(cursorIcon.GlobalPosition);
				}
			}
		}
		
		if (inEvent.IsAction("ui_cancel"))
		{
			if (editingRoute)
			{
				editingRoute = false;
				(((PlayDesignPlayer) selectedObject)!).playerData.Route.targetPoints = currentRoute.ToArray();
				rdm.EndEdit((((PlayDesignPlayer) selectedObject)!).routeIndex);
				//rdm.SubViewLineRendering(sCamera, mainCamera);
				selectedObject = null;
			}
			else
			{
				if (hoveringSelectable && !editingRoute)
				{
					if(!hoveredObject.canBeEditied) return;

					DeleteObject(hoveredObject);
				}
			}
		}
		
		if(playerSelected && !editingRoute) return;
		
		if (inEvent is InputEventMouseMotion mouseMotion)
		{

			if (mainCamera != null)
			{
				Vector2 mouseScreenPosition = mouseMotion.Position;

				Vector3 rayOrigin = mainCamera.ProjectRayOrigin(mouseScreenPosition);
				Vector3 rayNormal = mainCamera.ProjectRayNormal(mouseScreenPosition);

				float rayLength = mainCamera.Far;
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

	public async void SavePlay()
	{
		if (isOffencePlay)
		{
			play = new Play();
			play.IsOffence = true;
			play.PlayerDataOffence = new PlayerDataOffence[players.Count];
			for (int i = 0; i < players.Count; i++)
			{
				players[i].playerData.Position = new Vector2(players[i].GlobalPosition.X, players[i].GlobalPosition.Z);
				play.PlayerDataOffence[i] = players[i].playerData;
			}

			//sCamera.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
			rdm.SubViewLineRendering(sCamera, mainCamera);

			await ToSignal(GetTree().CreateTimer(.1f), "timeout");
			var tex = sCamera.GetTexture();
			var img = tex.GetImage();
			//Error compressed = img.Compress(Image.CompressMode.Max);
			//GD.Print(compressed);
			
			play.Image = img.SavePngToBuffer();
			
			play.Name = playName;
			
			Error error = ResourceSaver.Save(play, $"res://Resources/Plays/Offence/{play.Name}.tres", ResourceSaver.SaverFlags.None);
			
			if (error != Error.Ok)
			{
				GD.PrintErr($"Failed to save PlayerData: {error}");
			}
			else
			{
				GD.Print("PlayerData saved successfully!");
			}
		}
		
		GetTree().ReloadCurrentScene();
	}

	void DeleteObject(PlayDesignSelectable obj)
	{
		players.Remove((PlayDesignPlayer) obj);
		selectableObjects.Remove(obj);
		if (((PlayDesignPlayer) obj).routeIndex != -1)
		{
			rdm.RemoveRoute(((PlayDesignPlayer) obj).routeIndex);
		}
		hoveredObject = null;
		selectedObject = null;
		obj.QueueFree();
	}
	
	public void MakeNewRoute()
	{
		playerSelected = false;
		PlayDesignPlayer player = (PlayDesignPlayer) selectedObject;

		if (player.routeIndex == -1) player.routeIndex = rdm.lines.Count;
		rdm.NewRoute(mainCamera.UnprojectPosition(selectedObject.GlobalPosition));
		player.playerData.Route = new Route();

		currentRoute = new List<Vector3>();
		
		editingRoute = true;
	}
	
	public void SpawnNewPlayer()
	{
		PlayerStats ps = null;
		foreach (var p in playerTypes)
		{
			if (p.PlayerType == selectedPlayerType)
			{
				ps = p;
				break;
			}
		}
		
		if(selectedObject != null)
		{
			((PlayDesignPlayer) selectedObject).playerData.PlayerType = ps;
			((PlayDesignPlayer) selectedObject).Init();
			playerSelected = false;
			return;
		}
		
		PlayDesignPlayer player = (PlayDesignPlayer)playerPrefab.Instantiate<Node3D>();
		player.Name = "PlayDesignPlayer" + selectedPlayerType;
		AddChild(player);
		player.GlobalPosition = cursorIcon.GlobalPosition;
		player.playerData = new PlayerDataOffence();
		player.playerData.PlayerType = ps;
		
		player.Init();
		
		players.Add(player);
		playerSelected = false;
	}
}
