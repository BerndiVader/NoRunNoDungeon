using Godot;
using System;

public class OptionsUI : BaseUI
{
    private AudioStream exampleSfx=ResourceLoader.Load<AudioStream>("res://sounds/ingame/10_UI_Menu_SFX/001_Hover_01.wav");
    private AudioStreamPlayer2D player;
    
    private Button acceptBtn, defaultBtn, cancelBtn;
    private CheckBox fullScreen, vSync, fx;
    private HSlider volume, sfx, background;

    private GameSettings.Config localConfig;

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
        

        acceptBtn.Connect("button_up",this,nameof(onButtonUp),new Godot.Collections.Array(acceptBtn));
        defaultBtn.Connect("button_up",this,nameof(onButtonUp),new Godot.Collections.Array(defaultBtn));
        cancelBtn.Connect("button_up",this,nameof(onButtonUp),new Godot.Collections.Array(cancelBtn));
        fx.Connect("button_up",this,nameof(onButtonUp),new Godot.Collections.Array(fx));
        

        volume.Connect("drag_ended",this,nameof(changeVolume));
        sfx.Connect("drag_ended",this,nameof(changeSfx));
        background.Connect("drag_ended",this,nameof(changeBackground));

        fullScreen.Connect("button_up",this,nameof(onButtonUp),new Godot.Collections.Array(fullScreen));
        vSync.Connect("button_up",this,nameof(onButtonUp),new Godot.Collections.Array(vSync));

        updateButtons();

        player=new AudioStreamPlayer2D();
        player.Stream=exampleSfx;
        AddChild(player);
    }

    private void changeVolume(bool changed)
    {
        AudioServer.SetBusVolumeDb(GameSettings.current.masterBus,(float)volume.Value);
        player.Bus="Master";
        player.Play();
    }
    private void changeSfx(bool changed)
    {
        AudioServer.SetBusVolumeDb(GameSettings.current.sfxBus,(float)sfx.Value);
        player.Bus="Sfx";
        player.Play();
    }
    private void changeBackground(bool changed)
    {
        AudioServer.SetBusVolumeDb(GameSettings.current.backgroundBus,(float)background.Value);
        player.Bus="Background";
        player.Play();
    }
    private void onButtonUp(Button button)
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
                    GameSettings.current.windowPosition=new Tuple<float, float>(OS.WindowPosition.x,OS.WindowPosition.y);
                    GameSettings.current.windowSize=new Tuple<float, float>(OS.WindowSize.x,OS.WindowSize.y);
                }
                GameSettings.current.usage=fx.Pressed?Viewport.UsageEnum.Usage2d:Viewport.UsageEnum.Usage3d;
                GameSettings.saveConfig(GameSettings.current);
                GameSettings.current.setAll(this);
                back();
                break;
            case "Default":
                GameSettings.defaultConfig();
                GameSettings.current.setAll(this);
                updateButtons();
                break;
            case "Cancel":
                back();
                break;
            case "Fullscreen":
                OS.WindowFullscreen=fullScreen.Pressed;
                if(!OS.WindowFullscreen)
                {
                    OS.WindowSize=new Vector2(GameSettings.current.windowSize.Item1,GameSettings.current.windowSize.Item2);
                    OS.WindowPosition=new Vector2(GameSettings.current.windowPosition.Item1,GameSettings.current.windowPosition.Item2);
                }
                break;
            case "VSync":
                OS.VsyncEnabled=vSync.Pressed;
                break;
            case "FX":
                GetViewport().Usage=fx.Pressed?Viewport.UsageEnum.Usage2d:Viewport.UsageEnum.Usage3d;
                break;
        }
    }

    private void updateButtons()
    {
        volume.Value=GameSettings.current.masterVolume;
        sfx.Value=GameSettings.current.sfxVolume;
        background.Value=GameSettings.current.BackgroundVolume;
        fullScreen.Pressed=GameSettings.current.fullscreen;
        vSync.Pressed=GameSettings.current.vsync;
        fx.Pressed=GameSettings.current.usage==Viewport.UsageEnum.Usage2d;
    }

    private void back()
    {
        GameSettings.current.setAll(this);
        
        PauseUI pause=PausePack.Instance<PauseUI>();
        pause.PauseMode=PauseModeEnum.Process;
        GetParent().AddChild(pause);
        sprite.QueueFree();
        QueueFree();

    }
}
