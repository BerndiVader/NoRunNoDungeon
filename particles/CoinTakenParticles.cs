using Godot;
using System;

public class CoinTakenParticles : CPUParticles2D
{
    static Vector2 offset=new Vector2(0f,0.5f);

    public static AudioStream sfxSmall=ResourceLoader.Load<AudioStream>("res://sounds/ingame/PickUp/Retro PickUp Coin 04.wav");
    public static AudioStream sfxBig=ResourceLoader.Load<AudioStream>("res://sounds/ingame/PickUp/Retro PickUp Coin 07.wav");

    public SfxPlayer audio=new SfxPlayer();

    public override void _Ready()
    {
        audio.Position=Position;
        audio.VolumeDb=-10;
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
