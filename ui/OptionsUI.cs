using Godot;
using System;

public class OptionsUI : BaseUI
{
    private readonly AudioStream sfxTest=ResourceLoader.Load<AudioStream>("res://sounds/ingame/10_UI_Menu_SFX/051_use_item_01.wav");
    private AudioStreamPlayer2D player;

    public PackedScene back;
    
    private Button acceptBtn, defaultBtn, cancelBtn;
    private CheckBox fullScreen, vSync, fx;
    private HSlider volume, sfx, background;

    public override void _Ready()
    {
        base._Ready();
        
        acceptBtn=GetNode<Button>("Accept");
        defaultBtn=GetNode<Button>("Default");
        cancelBtn=GetNode<Button>("Cancel");
        volume=GetNode<HSlider>("Sound/Overall");
        sfx=GetNode<HSlider>("Sound/Sfx");
        background=GetNode<HSlider>("Sound/Background");

        fullScreen=GetNode<CheckBox>("Screen/Fullscreen");
        vSync=GetNode<CheckBox>("Screen/VSync");
        fx=GetNode<CheckBox>("Screen/FX");
        
        acceptBtn.Connect("button_up",this,nameof(OnButtonUp),new Godot.Collections.Array(acceptBtn));
        defaultBtn.Connect("button_up",this,nameof(OnButtonUp),new Godot.Collections.Array(defaultBtn));
        cancelBtn.Connect("button_up",this,nameof(OnButtonUp),new Godot.Collections.Array(cancelBtn));
        fx.Connect("button_up",this,nameof(OnButtonUp),new Godot.Collections.Array(fx));

        acceptBtn.Connect("button_down",this,nameof(PlaySfx),new Godot.Collections.Array(sfxButtons));
        defaultBtn.Connect("button_down",this,nameof(PlaySfx),new Godot.Collections.Array(sfxButtons));
        cancelBtn.Connect("button_down",this,nameof(PlaySfx),new Godot.Collections.Array(sfxButtons));
        acceptBtn.Connect("mouse_entered",this,nameof(PlaySfx),new Godot.Collections.Array(sfxHover));
        defaultBtn.Connect("mouse_entered",this,nameof(PlaySfx),new Godot.Collections.Array(sfxHover));
        cancelBtn.Connect("mouse_entered",this,nameof(PlaySfx),new Godot.Collections.Array(sfxHover));

        volume.Connect("drag_ended",this,nameof(ChangeVolume));
        sfx.Connect("drag_ended",this,nameof(ChangeSfx));
        background.Connect("drag_ended",this,nameof(ChangeBackground));

        fullScreen.Connect("button_up",this,nameof(OnButtonUp),new Godot.Collections.Array(fullScreen));
        vSync.Connect("button_up",this,nameof(OnButtonUp),new Godot.Collections.Array(vSync));

        UpdateButtons();

        player=new AudioStreamPlayer2D();
        player.Stream=sfxTest;
        AddChild(player);
    }

    private void ChangeVolume(bool changed)
    {
        AudioServer.SetBusVolumeDb(GameSettings.current.masterBus,(float)volume.Value);
        player.Bus="Master";
        player.Play();
    }
    private void ChangeSfx(bool changed)
    {
        AudioServer.SetBusVolumeDb(GameSettings.current.sfxBus,(float)sfx.Value);
        player.Bus="Sfx";
        player.Play();
    }
    private void ChangeBackground(bool changed)
    {
        AudioServer.SetBusVolumeDb(GameSettings.current.backgroundBus,(float)background.Value);
        player.Bus="Background";
        player.Play();
    }
    private void OnButtonUp(Button button)
    {
        switch(button.Name)
        {
            case "Accept":
                GameSettings.current.masterVolume=(float)volume.Value;
                GameSettings.current.sfxVolume=(float)sfx.Value;
                GameSettings.current.BackgroundVolume=(float)background.Value;
                GameSettings.current.fullscreen=fullScreen.Pressed;
                GameSettings.current.vsync=vSync.Pressed;
                if(!fullScreen.Pressed)
                {
                    GameSettings.current.windowPositionX=OS.WindowPosition.x;
                    GameSettings.current.windowPositionY=OS.WindowPosition.y;
                    GameSettings.current.windowSizeX=OS.WindowSize.x;
                    GameSettings.current.windowPositionY=OS.WindowSize.y;
                }
                GameSettings.current.usage=fx.Pressed?Viewport.UsageEnum.Usage2d:Viewport.UsageEnum.Usage3d;
                GameSettings.SaveConfig(GameSettings.current);
                GameSettings.current.SetAll(this);
                Back();
                break;
            case "Default":
                GameSettings.DefaultConfig();
                GameSettings.current.SetAll(this);
                UpdateButtons();
                break;
            case "Cancel":
                Back();
                break;
            case "Fullscreen":
                PlaySfx(sfxClick);
                OS.WindowFullscreen=fullScreen.Pressed;
                if(!OS.WindowFullscreen)
                {
                    OS.WindowSize=new Vector2(GameSettings.current.windowSizeX,GameSettings.current.windowSizeY);
                    OS.WindowPosition=new Vector2(GameSettings.current.windowPositionX,GameSettings.current.windowPositionY);
                }
                break;
            case "VSync":
                PlaySfx(sfxClick);
                OS.VsyncEnabled=vSync.Pressed;
                break;
            case "FX":
                PlaySfx(sfxClick);
                GetViewport().Usage=fx.Pressed?Viewport.UsageEnum.Usage2d:Viewport.UsageEnum.Usage3d;
                break;
        }
    }

    private void UpdateButtons()
    {
        volume.Value=GameSettings.current.masterVolume;
        sfx.Value=GameSettings.current.sfxVolume;
        background.Value=GameSettings.current.BackgroundVolume;
        fullScreen.Pressed=GameSettings.current.fullscreen;
        vSync.Pressed=GameSettings.current.vsync;
        fx.Pressed=GameSettings.current.usage==Viewport.UsageEnum.Usage2d;
    }

    private void Back()
    {
        GameSettings.current.SetAll(this);

        if(back!=null)
        {
            PauseUI pause=PausePack.Instance<PauseUI>();
            pause.PauseMode=PauseModeEnum.Process;
            World.instance.uiLayer.AddChild(pause);
        }
        QueueFree();

    }
}
