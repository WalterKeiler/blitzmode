using Godot;
using System;

public partial class PlaySelectionUIManager : Control
{
    const int PLAYS_PERPAGE = 9;
    
    public static PlaySelectionUIManager Instance;

    [Export] PSPlayUI[] team1Plays;
    [Export] PSPlayUI[] team2Plays;

    [Export] string OffencePlaysPath;
    [Export] string DefencePlaysPath;

    private int offencePage = 0;
    private int defencePage = 0;
    
    private Play[] OffencePlays;
    private Play[] DefencePlays;

    public override void _Ready()
    {
        base._Ready();
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
        LoadPlays(team1Plays, team2Plays);
    }

    void TurnPage(bool isOffence)
    {
        if (isOffence) offencePage++;
        else defencePage++;
    }
    
    public void LoadPlays(PSPlayUI[] offence, PSPlayUI[] defence)
    {
        for (int i = offencePage * PLAYS_PERPAGE; i < offence.Length; i++)
        {
            if(OffencePlays.Length <= i || i > PLAYS_PERPAGE) break;
            
            offence[i].name.Text = OffencePlays[i].Name;

            if(OffencePlays[i].Image == null || OffencePlays[i].Image.Length <= 0) continue;
            
            Image img = new Image();

            img.LoadPngFromBuffer(OffencePlays[i].Image);

            Texture2D imtex = ImageTexture.CreateFromImage(img);
            
            offence[i].image.Texture = imtex;
        }
        
        for (int i = defencePage * PLAYS_PERPAGE; i < defence.Length; i++)
        {
            if(DefencePlays.Length <= i || i > PLAYS_PERPAGE) break;
            
            defence[i].name.Text = DefencePlays[i].Name;

            if(DefencePlays[i].Image == null || DefencePlays[i].Image.Length <= 0) continue;
            
            Image img = new Image();

            img.LoadPngFromBuffer(DefencePlays[i].Image);

            Texture2D imtex = ImageTexture.CreateFromImage(img);
            
            defence[i].image.Texture = imtex;
        }
    }
    
}
