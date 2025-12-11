using Godot;
using System;
using System.IO;

public abstract class BaseUI : Panel
{
    public static PackedScene OptionsPack=ResourceLoader.Load<PackedScene>("res://ui/Options.tscn");
    public static PackedScene PausePack=ResourceLoader.Load<PackedScene>("res://ui/PauseUI.tscn");
    protected static AudioStream sfxHover=ResourceLoader.Load<AudioStream>("res://sounds/ingame/10_UI_Menu_SFX/001_Hover_01.wav");
    protected static AudioStream sfxClick=ResourceLoader.Load<AudioStream>("res://sounds/ingame/10_UI_Menu_SFX/013_Confirm_03.wav");
    protected static AudioStream sfxButtons=ResourceLoader.Load<AudioStream>("res://sounds/ingame/10_UI_Menu_SFX/029_Decline_09.wav");
    protected int selected;
    [Export] protected float SCALE_SIZE=1f;

    public override void _Ready()
    {
        RectScale=Vector2.One*SCALE_SIZE;
        RectPosition*=RectScale;
        RectPosition+=World.RESOLUTION*0.5f;
    }

    protected void PlaySfx(AudioStream effect)
    {
        SfxPlayer sfx=new SfxPlayer
        {
            Stream=effect,
            Position=RectPosition
        };
        AddChild(sfx);
        sfx.Play();
    }

}
