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

    [Export] private Label yardDownText;
    [Export] private Label quarterText;
    [Export] private Label timeInQuarterText;
    [Export] private Label selectionTimerText;
    [Export] private Label team1PageText;
    [Export] private Label team2PageText;
    [Export] private Label team1ScoreText;
    [Export] private Label team2ScoreText;
    
    [Export] int team1Selection = 0;
    [Export] int team2Selection = 0;
    
    [Export] string OffencePlaysPath;
    [Export] string DefencePlaysPath;

    [Export] int offencePage = 0;
    [Export] int defencePage = 0;
    
    Play[] OffencePlays;
    Play[] DefencePlays;

    private int team1InputID = 0;
    private int team2InputID = 1;

    float playSelectionTimer;
    
    int maxPageNumOff;
    int lastPageNumOff;
    int maxPageNumDef;
    int lastPageNumDef;
    
    GameManager gm;
    PlayManager pm;

    [Export] bool offenceSelected;
    [Export] bool defenceSelected;
    
    bool isSpecialTeams;
    bool h = false;
    bool holdingMotion = false;
    bool holdForRelease = false;
    bool AITeam;
    
    string[] suffixLookup = ["st","nd","rd","th"];

    bool ready = false;

    private Random rng;
    
    public override void _EnterTree()
    {
        base._EnterTree();
        PlayManager.UpdateScore += UpdateScoreBoard;
        Instance = this;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        PlayManager.UpdateScore -= UpdateScoreBoard;
    }

    
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
            else
            {
                AITeam = true;
                rng = new Random();
            }
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
        ready = true;
        UpdateSelection();
        //Init();
    }

    void UpdateScoreBoard()
    {
        team1ScoreText.Text = $"{pm.ScoreTeam1}";
        team2ScoreText.Text = $"{pm.ScoreTeam2}";
    }
    
    public void Init(bool specialTeams)
    {
        if(!ready) _Ready();
        
        team1Selection = 0;
        team2Selection = 0;
        offencePage = 0;
        defencePage = 0;

        offenceSelected = false;
        defenceSelected = false;

        isSpecialTeams = specialTeams;
        
        int min = Mathf.FloorToInt(pm.quarterTimer / 60);
        int sec = Mathf.FloorToInt(pm.quarterTimer) - (min * 60);
        
        int suffixD = Mathf.Clamp((gm.DownsTillTurnover - (pm.CurrentDown)), 0, suffixLookup.Length - 1);
        int suffix = Mathf.Clamp(pm.quarterNumber - 1, 0, suffixLookup.Length - 1);

        int first = Mathf.RoundToInt(Mathf.Abs(pm.firstDownLine - pm.lineOfScrimmage));
        int currentYard = Mathf.RoundToInt((pm.PlayDirection * pm.lineOfScrimmage) + gm.fieldLength / 2f);
        
        string yardsTillFirst = first + currentYard < gm.fieldLength ? first.ToString() : "Goal";
        yardDownText.Text =
            $"{gm.DownsTillTurnover - (pm.CurrentDown - 1)}{suffixLookup[suffixD]} & " +
            $"{yardsTillFirst} on the " +
            $"{currentYard}";
        quarterText.Text = $"{pm.quarterNumber}{suffixLookup[suffix]} Quarter";
        timeInQuarterText.Text = $"{min}:{sec:D2}";
        
        team1ScoreText.Text = $"{pm.ScoreTeam1}";
        team2ScoreText.Text = $"{pm.ScoreTeam2}";
        
        playSelectionTimer = gm.timeToPickPlaySec;
        
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
        
        if(AITeam)
        {
            if (pm.PlayDirection == 1) team2Selection = rng.Next(0, DefencePlays.Length);
            if (pm.PlayDirection != 1) team2Selection = rng.Next(0, OffencePlays.Length);
            LockTeamSelection(pm.PlayDirection != 1);
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if(!Visible) return;
        playSelectionTimer -= (float) delta;
        selectionTimerText.Text = $"{Mathf.CeilToInt(playSelectionTimer):D2}";
        if(playSelectionTimer < 0) StartPlay();
    }

    public override void _Input(InputEvent inEvent)
    {
        base._Input(inEvent);
        if(!Visible) return;
        if (inEvent.GetDevice() == team1InputID)
        {
            HandleInput(ref team1Selection, inEvent, inEvent.GetDevice());
        }

        if (inEvent.GetDevice() == team2InputID)
        {
            HandleInput(ref team2Selection, inEvent, inEvent.GetDevice());
        }

        UpdateSelection();
    }

    void HandleInput(ref int team, InputEvent inEvent, int deviceID)
    {
        bool isOffence = (deviceID == team1InputID && pm.PlayDirection == 1);

        int lastPageIndex = isOffence ? lastPageNumOff : lastPageNumDef;
        int maxPage = isOffence ? maxPageNumOff : maxPageNumDef;
        int currentPageIndex = isOffence ? offencePage : defencePage;
        
        bool isOnLastPage = currentPageIndex == maxPage;
        // bool useCanMoveDown = true;
        // if(isOnLastPage)
        
        if (inEvent.IsAction("ui_accept"))
        {
            LockTeamSelection(isOffence);
        }
        // if (inEvent.IsAction("ui_cancel"))
        // {
        //     UnlockTeamSelection(isOffence);
        // }
        
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
        //GD.Print(inEvent.GetDevice() + " is off: " + isOffence);
        //if(isOffence && offenceSelected || !isOffence && defenceSelected) return;
        
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
                        if(MathF.Abs(joyEvent.GetAxisValue()) > InputManager.JOYSTICKDEADZONE + .75f)
                        {
                            team += (joyEvent.GetAxisValue() > 0 ? 1 : -1);
                            UnlockTeamSelection(isOffence);
                        }
                        
                        if (MathF.Abs(joyEvent.GetAxisValue()) > InputManager.JOYSTICKDEADZONE + .75f)
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
                        {
                            team += -1;
                            UnlockTeamSelection(isOffence);
                        }
                    }

                    if (inEvent.IsAction("move_right") && keyEvent.Pressed)
                    {
                        if((team + 1) % 3 != 0 && !(isOnLastPage && team + 1 >= lastPageIndex))
                        {
                            team += 1;
                            UnlockTeamSelection(isOffence);
                        }
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
                    {
                        if(MathF.Abs(joyEvent.GetAxisValue()) > InputManager.JOYSTICKDEADZONE + .75f)
                        {
                            team += -(joyEvent.GetAxisValue() > 0 ? -3 : 3);
                            UnlockTeamSelection(isOffence);

                        }
                        
                        if (MathF.Abs(joyEvent.GetAxisValue()) > InputManager.JOYSTICKDEADZONE + .75f)
                        {
                            holdingMotion = true;
                        }
                    }

                    break;
                }
                case InputEventKey keyEvent:
                {
                    if (inEvent.IsAction("move_back") && keyEvent.Pressed)
                    {
                        if(team + 3 < PLAYS_PERPAGE && !(isOnLastPage && team + 3 >= lastPageIndex))
                        {
                            team += 3;
                            UnlockTeamSelection(isOffence);

                        }
                    }

                    if (inEvent.IsAction("move_forward") && keyEvent.Pressed)
                    {
                        if(team - 3 >= 0)
                        {
                            team += -3;
                            UnlockTeamSelection(isOffence);
                        }
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

    void LockTeamSelection(bool isOffence)
    {
        GD.Print("Lock isOff: " + isOffence);
        if (isOffence) offenceSelected = true;
        else defenceSelected = true;
        
        if(offenceSelected && defenceSelected) StartPlay();
    }
    void UnlockTeamSelection(bool isOffence)
    {
        GD.Print("UnlockisOff: " + isOffence);
        if (isOffence) offenceSelected = false;
        else defenceSelected = false;
    }
    
    void UpdateSelection()
    {
        team1SelectionIndicator.Reparent(team1Plays[team1Selection]);
        team1SelectionIndicator.Position = Vector2.Zero;
        if (AITeam)
        {
            team2SelectionIndicator.Visible = false;
            return;
        }
        team2SelectionIndicator.Reparent(team2Plays[team2Selection]);
        team2SelectionIndicator.Position = Vector2.Zero;
    }
    
    public void LoadPlays(PSPlayUI[] offence, PSPlayUI[] defence)
    {
        int team1Page = offence[0] == team1Plays[0] ? offencePage + 1 : defencePage + 1;
        int team2Page = offence[0] == team2Plays[0] ? offencePage + 1 : defencePage + 1;

        team1PageText.Text = $"Page {team1Page}";
        team2PageText.Text = $"Page {team2Page}";
        int o = 0;
        for (int i = 0; i < offence.Length; i++)
        {
            if(i > PLAYS_PERPAGE) break;
            int play = offencePage * PLAYS_PERPAGE + i + o;
            if (play >= OffencePlays.Length)
            {
                offence[i].Visible = false;
                continue;
            }

            if (OffencePlays[play].IsSpecialTeams && !isSpecialTeams)
            {
                o++;
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

        int d = 0;
        for (int i = defencePage * PLAYS_PERPAGE; i < defence.Length; i++)
        {
            if(i > (defencePage + 1) * PLAYS_PERPAGE) break;
            int play = defencePage * PLAYS_PERPAGE + i + d;
            if (play >= DefencePlays.Length)
            {
                defence[i].Visible = false;
                continue;
            }
            
            if (DefencePlays[play].IsSpecialTeams && !isSpecialTeams)
            {
                d++;
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

    public async void StartPlay()
    {
        if(!Visible) return;
        await ToSignal(GetTree().CreateTimer(.25f), "timeout");
        if(!Visible) return;
        Visible = false;
        if (pm.PlayDirection == 1)
        {
            pm.OffencePlay = OffencePlays[team1Selection];
            pm.DefencePlay = DefencePlays[team2Selection];
        }
        else
        {
            pm.OffencePlay = OffencePlays[team2Selection];
            pm.DefencePlay = DefencePlays[team1Selection];
        }
        
        pm.StartPlay();
        team1SelectionIndicator.Reparent(team1Plays[0]);
        team1SelectionIndicator.Position = Vector2.Zero;
        team2SelectionIndicator.Reparent(team2Plays[0]);
        team2SelectionIndicator.Position = Vector2.Zero;
    }
}
