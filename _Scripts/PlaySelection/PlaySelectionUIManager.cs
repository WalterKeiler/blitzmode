using Godot;
using System;

public partial class PlaySelectionUIManager : Control
{
    const int PLAYS_PERPAGE = 9;
    
    public static PlaySelectionUIManager Instance;

    [Export] PSPlayUI[] team1Plays;
    [Export] PSPlayUI[] team2Plays;

    [Export] private Control team1SelectionIndicator;
    [Export] private Control team2SelectionIndicator;
    
    [Export] int team1Selection = 0;
    [Export] int team2Selection = 0;
    
    [Export] string OffencePlaysPath;
    [Export] string DefencePlaysPath;

    [Export] int offencePage = 0;
    [Export] int defencePage = 0;
    
    [Export] Play[] OffencePlays;
    [Export] Play[] DefencePlays;

    private int team1InputID = 0;
    private int team2InputID = 1;

    [Export] int maxPageNumOff;
    [Export] int lastPageNumOff;
    [Export] int maxPageNumDef;
    [Export] int lastPageNumDef;
    
    private GameManager gm;
    private PlayManager pm;

    bool h = false;
    bool holdingMotion = false;
    bool holdForRelease = false;
    
    public override void _Ready()
    {
        base._Ready();
        
        pm = PlayManager.Instance;
        gm = GameManager.Instance;

        if(gm != null)
        {
            if(gm.playerInputTeam1.Length > 0)
                team1InputID = gm.playerInputTeam1[0].PlayerID;
            if(gm.playerInputTeam2.Length > 0)
                team2InputID = gm.playerInputTeam2[0].PlayerID;
        }
        
        
        string[] off = DirAccess.GetFilesAt(OffencePlaysPath);
        OffencePlays = new Play[off.Length];
        for (int i = 0; i < off.Length; i++)
        {
            OffencePlays[i] = (Play) ResourceLoader.Load(OffencePlaysPath + off[i]);
        }
        
        string[] def = DirAccess.GetFilesAt(DefencePlaysPath);
        DefencePlays = new Play[def.Length];
        for (int i = 0; i < def.Length; i++)
        {
            DefencePlays[i] = (Play) ResourceLoader.Load(DefencePlaysPath + def[i]);
        }

        maxPageNumOff = Mathf.CeilToInt(OffencePlays.Length / PLAYS_PERPAGE);
        maxPageNumDef = Mathf.CeilToInt(DefencePlays.Length / PLAYS_PERPAGE);

        lastPageNumOff = OffencePlays.Length % PLAYS_PERPAGE;
        lastPageNumDef = DefencePlays.Length % PLAYS_PERPAGE;
        
        UpdateSelection();
        if(pm != null)
        {
            if (pm.PlayDirection == 1)
                LoadPlays(team1Plays, team2Plays);
            else
                LoadPlays(team2Plays, team1Plays);
        }
        else
        {
            LoadPlays(team1Plays, team2Plays);
        }
    }

    public override void _Input(InputEvent inEvent)
    {
        base._Input(inEvent);

        if (inEvent.GetDevice() == team1InputID)
        {
            HandleInput(ref team1Selection, inEvent);
        }

        if (inEvent.GetDevice() == team2InputID)
        {
            HandleInput(ref team2Selection, inEvent);
        }

        UpdateSelection();
    }

    void HandleInput(ref int team, InputEvent inEvent)
    {
        bool isOffence = team == team1Selection;
        //isOffence = team == team1Selection && pm.PlayDirection == 1;

        int lastPageIndex = isOffence ? lastPageNumOff : lastPageNumDef;
        int maxPage = isOffence ? maxPageNumOff : maxPageNumDef;
        int currentPageIndex = isOffence ? offencePage : defencePage;

        bool isOnLastPage = currentPageIndex == maxPage;
        // bool useCanMoveDown = true;
        // if(isOnLastPage)
        
        if (inEvent.IsAction("ui_accept"))
        {
            LockTeamSelection(isOffence);
            return;
        }
        if (inEvent.IsAction("ui_cancel"))
        {
            UnlockTeamSelection(isOffence);
            return;
        }
        if (inEvent.IsAction("ui_turn_page_left"))
        {
            switch (isOffence)
            {
                case true:
                {
                    if(offencePage - 1 >= 0 && holdForRelease)
                    {
                        holdForRelease = false;
                        offencePage--;
                        if(offencePage == maxPageNumOff && team >= lastPageNumOff)
                            team = 0;
                    }
                    if (offencePage - 1 >= 0 && !holdForRelease)
                    {
                        holdForRelease = true;
                    }

                    break;
                }
                case false:
                {
                    if(defencePage - 1 >= 0 && holdForRelease)
                    {
                        holdForRelease = false;
                        defencePage--;
                        if(defencePage == maxPageNumDef && team >= lastPageNumDef)
                            team = 0;
                    }
                    if (defencePage - 1 >= 0 && !holdForRelease)
                    {
                        holdForRelease = true;
                    }

                    break;
                }
            }

            if(pm != null)
            {
                if (pm.PlayDirection == 1)
                    LoadPlays(team1Plays, team2Plays);
                else
                    LoadPlays(team2Plays, team1Plays);
            }
            else
            {
                LoadPlays(team1Plays, team2Plays);
            }
            return;
        }
        if (inEvent.IsAction("ui_turn_page_right"))
        {
            switch (isOffence)
            {
                case true:
                {
                    if(offencePage + 1 <= maxPageNumOff && holdForRelease)
                    {
                        holdForRelease = false;
                        offencePage++;
                        if(offencePage == maxPageNumOff && team >= lastPageNumOff)
                            team = 0;
                    }
                    if (offencePage + 1 <= maxPageNumOff && !holdForRelease)
                    {
                        holdForRelease = true;
                    }

                    break;
                }
                case false:
                {
                    if(defencePage + 1 <= maxPageNumOff && holdForRelease)
                    {
                        holdForRelease = false;
                        defencePage++;
                        if(defencePage == maxPageNumDef && team >= lastPageNumDef)
                            team = 0;
                    }
                    if (defencePage + 1 <= maxPageNumOff && !holdForRelease)
                    {
                        holdForRelease = true;
                    }

                    break;
                }
            }

            if(pm != null)
            {
                if (pm.PlayDirection == 1)
                    LoadPlays(team1Plays, team2Plays);
                else
                    LoadPlays(team2Plays, team1Plays);
            }
            else
            {
                LoadPlays(team1Plays, team2Plays);
            }
            return;
        }
        
        if(inEvent.IsAction("move_left") || inEvent.IsAction("move_right"))
        {
            switch (inEvent)
            {
                case InputEventJoypadMotion joyEvent:
                {
                    if(((joyEvent.GetAxisValue() > 0 && (team + 1) % 3 != 0 &&
                         !(isOnLastPage && team + 1 >= lastPageIndex)) ||
                        (joyEvent.GetAxisValue() < 0 && team % 3 != 0)) && holdingMotion == false)
                    {
                        team += (MathF.Abs(joyEvent.GetAxisValue()) > InputManager.JOYSTICKDEADZONE + .25f
                            ? (joyEvent.GetAxisValue() > 0 ? 1 : -1)
                            : 0);
                        if (MathF.Abs(joyEvent.GetAxisValue()) > InputManager.JOYSTICKDEADZONE + .25f)
                        {
                            holdingMotion = true;
                        }
                        
                    }

                    break;
                }
                case InputEventKey keyEvent:
                {
                    if (inEvent.IsAction("move_left") && keyEvent.Pressed)
                    {
                        if(team % 3 != 0)
                            team += -1;
                    }

                    if (inEvent.IsAction("move_right") && keyEvent.Pressed)
                    {
                        if((team + 1) % 3 != 0 && !(isOnLastPage && team + 1 >= lastPageIndex))
                            team += 1;
                    }

                    break;
                }
            }
        }

        if (inEvent.IsAction("move_back") || inEvent.IsAction("move_forward"))
        {
            switch (inEvent)
            {
                case InputEventJoypadMotion joyEvent:
                {
                    if(((joyEvent.GetAxisValue() > 0 && team + 3 < PLAYS_PERPAGE && !(isOnLastPage && team + 3 >= lastPageIndex)) ||
                        (joyEvent.GetAxisValue() < 0 && team - 3 >= 0)) && holdingMotion == false)
                        team += -(MathF.Abs(joyEvent.GetAxisValue()) > InputManager.JOYSTICKDEADZONE + .25f ? 
                            (joyEvent.GetAxisValue() > 0 ? -3 : 3) : 0);
                    if (MathF.Abs(joyEvent.GetAxisValue()) > InputManager.JOYSTICKDEADZONE + .25f)
                    {
                        holdingMotion = true;
                    }

                    break;
                }
                case InputEventKey keyEvent:
                {
                    if (inEvent.IsAction("move_back") && keyEvent.Pressed)
                    {
                        if(team + 3 < PLAYS_PERPAGE && !(isOnLastPage && team + 3 >= lastPageIndex))
                            team += 3;
                    }

                    if (inEvent.IsAction("move_forward") && keyEvent.Pressed)
                    {
                        if(team - 3 >= 0)
                            team += -3;
                    }

                    break;
                }
            }
        }

        if (inEvent is InputEventJoypadMotion joyE)
        {
            if (MathF.Abs(joyE.GetAxisValue()) > InputManager.JOYSTICKDEADZONE + .25f && !h) EndHold();
        }
        
        team = Mathf.Clamp(team, 0, PLAYS_PERPAGE - 1);
    }

    async void EndHold()
    {
        h = true;
        await ToSignal(GetTree().CreateTimer(.25f), "timeout");
        h = false;
        holdingMotion = false;
    }

    void LockTeamSelection(bool isTeam1)
    {
        
    }
    void UnlockTeamSelection(bool isTeam1)
    {
        
    }
    
    void UpdateSelection()
    {
        team1SelectionIndicator.Reparent(team1Plays[team1Selection]);
        team1SelectionIndicator.Position = Vector2.Zero;
        team2SelectionIndicator.Reparent(team2Plays[team2Selection]);
        team2SelectionIndicator.Position = Vector2.Zero;
    }
    
    public void LoadPlays(PSPlayUI[] offence, PSPlayUI[] defence)
    {
        for (int i = 0; i < offence.Length; i++)
        {
            if(i > PLAYS_PERPAGE) break;
            int play = offencePage * PLAYS_PERPAGE + i;
            if (play >= OffencePlays.Length)
            {
                offence[i].Visible = false;
                continue;
            }
            
            offence[i].Visible = true;
            offence[i].name.Text = OffencePlays[play].Name;

            if(OffencePlays[play].Image == null || OffencePlays[play].Image.Length <= 0) continue;
            
            Image img = new Image();

            img.LoadPngFromBuffer(OffencePlays[play].Image);

            Texture2D imtex = ImageTexture.CreateFromImage(img);
            
            offence[i].image.Texture = imtex;
        }
        
        for (int i = defencePage * PLAYS_PERPAGE; i < defence.Length; i++)
        {
            if(i > (defencePage + 1) * PLAYS_PERPAGE) break;
            int play = defencePage * PLAYS_PERPAGE + i;
            if (play >= DefencePlays.Length)
            {
                defence[i].Visible = false;
                continue;
            }
            defence[i].Visible = true;
            defence[i].name.Text = DefencePlays[play].Name;

            if(DefencePlays[play].Image == null || DefencePlays[play].Image.Length <= 0) continue;
            
            Image img = new Image();

            img.LoadPngFromBuffer(DefencePlays[play].Image);

            Texture2D imtex = ImageTexture.CreateFromImage(img);
            
            defence[play].image.Texture = imtex;
        }
    }
    
}
