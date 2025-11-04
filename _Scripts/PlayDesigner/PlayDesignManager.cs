using Godot;
using System;
using System.Collections.Generic;

public partial class PlayDesignManager : Node3D
{
	public static PlayDesignManager Instance;

	[Export] public Play play;
	
	[Export] public PackedScene playerPrefab;
	[Export] public MeshInstance3D cursorIcon;
	[Export] public PackedScene zonePrefab;

	[Export] Camera3D mainCamera;
	[Export] SubViewport sCamera;
	
	[Export] private PlayerStats[] playerTypes;

	public PlayerType selectedPlayerType;
	public bool playerSelected;

	public string playName;
	
	[Export] public bool isOffencePlay = true;
	bool hoveringSelectable;
	bool canPlacePlayer;
	[Export] bool editingRoute;
	[Export] bool editingZone;
	
	Vector3 CursorMinPos = new (-10, -1, -27);
	Vector3 CursorMaxPos = new (30, -1, 27);
	Vector3 cursorPos;
	Vector3 QBStartPos = new Vector3(-3, 1, 0);
	Vector3 CBStartPos = new Vector3(-1, 1, 0);
	Material mat;

	private List<Vector3> currentRoute;
	private List<Zone> zones;
	private List<MeshInstance3D> zonesObjects;
	
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
		if(isOffencePlay)
		{
			if (cursorIcon.GlobalPosition.X >= 0)
			{
				canPlacePlayer = false;
				light = true;
			}
		}
		else
		{
			if (cursorIcon.GlobalPosition.X <= 0)
			{
				canPlacePlayer = false;
				light = true;
			}
		}
		((ShaderMaterial) mat).SetShaderParameter("isSelected", light);

		if (editingRoute)
		{
			rdm.UpdateLine(mainCamera, cursorIcon.GlobalPosition, ((PlayDesignPlayer) selectedObject).routeIndex);
			((ShaderMaterial) mat).SetShaderParameter("isSelected", false);
		}

		if (editingZone)
		{
			UpdateZone(((PlayDesignPlayer) selectedObject).zoneIndex);
			((ShaderMaterial) mat).SetShaderParameter("isSelected", false);
		}
	}

	public void Init(bool isOffence)
	{
		isOffencePlay = isOffence;
		zones = new List<Zone>();
		zonesObjects = new List<MeshInstance3D>();
		if(isOffencePlay)
		{
			selectedPlayerType = PlayerType.Quarterback;
			cursorIcon.GlobalPosition = QBStartPos;
			SpawnNewPlayer();
			players[0].playerDataOff.IsPlayer = true;
			players[0].playerDataOff.StartsWithBall = true;
			players[0].canBeEditied = false;

			selectedPlayerType = PlayerType.OLineman;
			cursorIcon.GlobalPosition = CBStartPos;
			SpawnNewPlayer();
			players[1].canBeEditied = false;
		}
	}

	public void Reset()
	{
		for (int i = players.Count - 1; i >= 0; i--)
		{
			DeleteObject(players[i]);
		}
		rdm.Reset();
		pdUI.TurnOffAll();
		rdm.lines = new List<Line2D>();
		foreach (var c in sCamera.GetChildren())
		{
			if(c is Line2D)
				c.QueueFree();
		}

	}
	
	public override void _Input(InputEvent inEvent)
	{
		if (inEvent.IsAction("ui_accept") && !playerSelected)
		{
			if (inEvent is InputEventMouseButton mouseButton)
			{
				Vector3 mouseScreenPosition = mainCamera.ProjectPosition(mouseButton.Position, 0);
				if (mouseScreenPosition.X > CursorMaxPos.X || mouseScreenPosition.Z > CursorMaxPos.Z || 
				    mouseScreenPosition.X < CursorMinPos.X || mouseScreenPosition.Z < CursorMinPos.Z) return;
			}
			if(canPlacePlayer && !hoveringSelectable && !editingRoute && !editingZone)
			{
				selectedObject = null;
				playerSelected = true;
				if (mainCamera != null)
				{
					pdUI.SelectPlayerType(mainCamera.UnprojectPosition(cursorPos));
				}
			}

			else if (hoveringSelectable && !editingRoute && !editingZone)
			{
				if(!hoveredObject.canBeEditied) return;
				
				selectedObject = hoveredObject;
				if (selectedObject is PlayDesignPlayer)
				{
					playerSelected = true;
					if (mainCamera != null) pdUI.SelectPlayer(mainCamera.UnprojectPosition(cursorPos));
				}
			}
			
			else if(editingRoute)
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
				(((PlayDesignPlayer) selectedObject)!).playerDataOff.Route.targetPoints = currentRoute.ToArray();
				rdm.EndEdit((((PlayDesignPlayer) selectedObject)!).routeIndex, (((PlayDesignPlayer) selectedObject)!).playerDataOff.PlayerType.PlayerType);
				//rdm.SubViewLineRendering(sCamera, mainCamera);

				switch ((((PlayDesignPlayer) selectedObject)!).playerDataOff.PlayerType.PlayerType)
				{
					case PlayerType.Receiver :
						(((PlayDesignPlayer) selectedObject)!).playerDataOff.Route.endAction = EndRouteAction.Continue;
						break;
					case PlayerType.OLineman :
						(((PlayDesignPlayer) selectedObject)!).playerDataOff.Route.endAction = EndRouteAction.Block;
						break;
					case PlayerType.Safety :
						(((PlayDesignPlayer) selectedObject)!).playerDataOff.Route.endAction = EndRouteAction.Zone;
						break;
				}
				
				selectedObject = null;
			}
			else if (editingZone)
			{
				EndZoneEdit((((PlayDesignPlayer) selectedObject)!).zoneIndex);
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
				players[i].playerDataOff.Position = new Vector2(players[i].GlobalPosition.Z, players[i].GlobalPosition.X);
				if (players[i].routeIndex != -1)
					players[i].playerDataOff.followRoute = 1;
				else
				{
					players[i].playerDataOff.block = 1;
				}
				play.PlayerDataOffence[i] = players[i].playerDataOff;
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

		else
		{
			play = new Play();
			play.IsOffence = false;
			play.PlayerDataDefence = new PlayerDataDefence[players.Count];
			for (int i = 0; i < players.Count; i++)
			{
				players[i].playerDataDef.Position = new Vector2(players[i].GlobalPosition.Z, players[i].GlobalPosition.X);
				if (players[i].zoneIndex != -1)
				{
					players[i].playerDataDef.coverZone = 1;
					players[i].playerDataDef.Zone = players[i].zone;
				}
				else if(players[i].playerDataDef.PlayerType.PlayerType == PlayerType.Safety && players[i].playerDataDef.rushBall < 1)
				{
					players[i].playerDataDef.followPlayer = 1;
				}
				else
				{
					players[i].playerDataDef.rushBall = 1;
				}
				play.PlayerDataDefence[i] = players[i].playerDataDef;
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
			
			Error error = ResourceSaver.Save(play, $"res://Resources/Plays/Defence/{play.Name}.tres", ResourceSaver.SaverFlags.None);
			
			if (error != Error.Ok)
			{
				GD.PrintErr($"Failed to save PlayerData: {error}");
			}
			else
			{
				GD.Print("PlayerData saved successfully!");
			}
		}
		
		Reset();
	}

	void DeleteObject(PlayDesignSelectable obj)
	{
		players.Remove((PlayDesignPlayer) obj);
		selectableObjects.Remove(obj);
		
		if (((PlayDesignPlayer) obj).routeIndex != -1)
		{
			rdm.RemoveRoute(((PlayDesignPlayer) obj).routeIndex);
		}
		
		if (((PlayDesignPlayer) obj).zoneIndex != -1)
		{
			zonesObjects[((PlayDesignPlayer) obj).zoneIndex].QueueFree();
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
		player.playerDataOff.Route = new Route();

		currentRoute = new List<Vector3>();
		currentRoute.Add(player.GlobalPosition);
		
		editingRoute = true;
	}

	public void Blitz()
	{
		playerSelected = false;
		PlayDesignPlayer player = (PlayDesignPlayer) selectedObject;
		player.playerDataDef.rushBall = 1;

		Vector3 dir = (player.GlobalPosition * new Vector3(0, 1, 1)).DirectionTo(QBStartPos);
		
		rdm.NewRoute(mainCamera.UnprojectPosition(selectedObject.GlobalPosition));
		rdm.UpdateLine(mainCamera, player.GlobalPosition * new Vector3(0,1,1), -1);
		rdm.PlacePoint(-1);
		rdm.UpdateLine(mainCamera, player.GlobalPosition * new Vector3(0,1,1) + (dir * 5), -1);
		rdm.PlacePoint(-1);
		rdm.EndEdit(-1, PlayerType.Safety);
	}

	public void MakeNewZone()
	{
		playerSelected = false;
		PlayDesignPlayer player = (PlayDesignPlayer) selectedObject;
		if (player.zoneIndex == -1) player.zoneIndex = zones.Count;
		player.zone = new Zone();
		player.zone.center = player.GlobalPosition;
		zones.Add(player.zone);

		MeshInstance3D mesh = (MeshInstance3D)zonePrefab.Instantiate();
		AddChild(mesh);
		mesh.GlobalPosition = player.GlobalPosition;
		Material zmat = (Material)mesh.GetActiveMaterial(0).Duplicate();
		mesh.MaterialOverride = zmat;
		zonesObjects.Add(mesh);
		
		editingZone = true;
	}

	public void UpdateZone(int index)
	{
		zonesObjects[index].Scale = Vector3.One * cursorIcon.GlobalPosition.DistanceTo(selectedObject.GlobalPosition) * 2;
		((ShaderMaterial)zonesObjects[index].GetActiveMaterial(0)).SetShaderParameter("Scale", cursorIcon.GlobalPosition.DistanceTo(selectedObject.GlobalPosition) * 2);
		(((PlayDesignPlayer) selectedObject)!).zone.radius =
			cursorIcon.GlobalPosition.DistanceTo(selectedObject.GlobalPosition) * 2;
	}
	
	public void EndZoneEdit(int index)
	{
		editingZone = false;
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
			((PlayDesignPlayer) selectedObject).playerDataOff.PlayerType = ps;
			((PlayDesignPlayer) selectedObject).Init();
			playerSelected = false;
			return;
		}
		
		PlayDesignPlayer player = (PlayDesignPlayer)playerPrefab.Instantiate<Node3D>();
		player.Name = "PlayDesignPlayer" + selectedPlayerType;
		AddChild(player);
		player.GlobalPosition = cursorIcon.GlobalPosition;
		if(isOffencePlay)
		{
			player.playerDataOff = new PlayerDataOffence();
			player.playerDataOff.PlayerType = ps;
		}
		else
		{
			player.playerDataDef = new PlayerDataDefence();
			player.playerDataDef.PlayerType = ps;
		}
		
		player.Init();
		
		players.Add(player);
		playerSelected = false;
	}
}
