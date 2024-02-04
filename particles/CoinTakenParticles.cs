using Godot;
using System;

public class CoinTakenParticles : CPUParticles2D
{
    static Vector2 offset=new Vector2(0f,0.5f);

    static AudioStream sfx=ResourceLoader.Load<AudioStream>("res://sounds/ingame/10_UI_Menu_SFX/001_Hover_01.wav");

    public override void _Ready()
    {
        SfxPlayer audio=new SfxPlayer();
        audio.Stream=sfx;
        audio.Position=Position;
        World.level.AddChild(audio);

        Emitting=true;
    }

    public override void _PhysicsProcess(float delta) 
    {
        if(!Emitting) 
        {
            QueueFree();
        }

        Position-=offset;
    }

}
