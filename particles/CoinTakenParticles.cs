using Godot;
using System;

public class CoinTakenParticles : CPUParticles2D
{
    private static Vector2 offset=new Vector2(0f,0.5f);
    public static readonly AudioStream sfxSmall=ResourceLoader.Load<AudioStream>("res://sounds/ingame/PickUp/Retro PickUp Coin 04.wav");
    public static readonly AudioStream sfxBig=ResourceLoader.Load<AudioStream>("res://sounds/ingame/PickUp/Retro PickUp Coin 07.wav");
    public AudioStream sfx=sfxSmall;

    public override void _Ready()
    {
        SetProcess(false);
        SetProcessInput(false);

        Renderer.instance.PlaySfx(sfx,GlobalPosition,-10f);

        OneShot=true;
        Restart();
    }

    public override void _PhysicsProcess(float delta) 
    {
        if(!Emitting)
        {
            SetPhysicsProcess(false);
            QueueFree();
        }
        Position-=offset;
    }

}
