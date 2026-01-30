using Godot;
using System;

public class HUD : Control
{
    public static HUD instance;
    private HBoxContainer lives;
    private static Texture live_full=ResourceLoader.Load<Texture>("res://image/0x72/frames/ui_heart_full.png");

    public HUD():base()
    {
        instance=this;
    }

    public override void _Ready()
    {
        lives=GetNode<HBoxContainer>("Lives");
    }
    public void UpdateLives()
    {
        foreach(Node live in lives.GetChildren())
        {
            live.QueueFree();
        }

        for(int i=0;i<Player.LIVES;i++)
        {
            TextureRect live=new TextureRect();
            live.Texture=live_full;
            live.Expand=false;
            live.StretchMode=TextureRect.StretchModeEnum.KeepCentered;
            lives.AddChild(live);
        }
        
    }

}
