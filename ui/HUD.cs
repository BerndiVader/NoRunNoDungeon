using Godot;
using System;

public class HUD : Control
{
    private static Texture heart_full=ResourceLoader.Load<Texture>("res://image/0x72/frames/ui_heart_full.png");
    private static Texture heart_empty=ResourceLoader.Load<Texture>("res://image/0x72/frames/ui_heart_empty.png");

    public static HUD instance;
    private HBoxContainer lives;
    private Label distance;
    private TextureProgress dash;
    private Label coins;

    public HUD():base()
    {
        instance=this;
    }

    public override void _Ready()
    {
        lives=GetNode<HBoxContainer>("Lives");
        distance=GetNode<HBoxContainer>("Distance").GetNode<Label>("Yards");
        dash=GetNode<HBoxContainer>("Modifiers").GetNode<TextureProgress>("Dash");
        coins=GetNode<HBoxContainer>("Values").GetNode<Label>("Coins");

        PopulateLives();
        PopulateDistance();
    }

    public void UpdateDistance(float dist)
    {
        distance.Text=Mathf.RoundToInt(dist)+" yards";
    }

    public void UpdateLives()
    {
        int hearts=lives.GetChildCount();
        for(int i=0;i<hearts;i++)
        {
            TextureRect heart=(TextureRect)lives.GetChild(i);
            heart.Texture=i<Player.LIVES?heart_full:heart_empty;
        }
    }

    public void UpdateDash(float value)
    {
        dash.Value=value;
    }
    public void UpdateCoins(int amount)
    {
        coins.Text=amount.ToString();
    }

    private void PopulateLives()
    {
        TextureRect heart;
        for(int i=0;i<ResourceUtils.PLAYER_LIVES_MAX;i++)
        {
            heart=new TextureRect
            {
                Texture=i<Player.LIVES?heart_full:heart_empty,
                Expand=false,
                StretchMode=TextureRect.StretchModeEnum.KeepCentered
            };
            lives.AddChild(heart);
        }
    }

    private void PopulateDistance()
    {
        distance.Text="0 meters";
    }

}
