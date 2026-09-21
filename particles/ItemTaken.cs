using System;
using Godot;

public class ItemTaken : CPUParticles2D
{
    private static readonly AudioStream sfxDefault=ResourceLoader.Load<AudioStream>("res://sounds/ingame/PowerUp/Retro PowerUP StereoUP 05.wav");
    private static readonly Vector2 offset=new Vector2(0f,0.05f);

    public AudioStream sfx=sfxDefault;

    public override void _Ready()
    {
        SetProcess(false);
        SetProcessInput(false);

        Renderer.instance.PlaySfx(sfx,GlobalPosition,-10f);
        
        OneShot=true;
        Emitting=true;
    }

    public override void _PhysicsProcess(float delta) 
    {
        if(!Emitting)
        {
            QueueFree();
            SetPhysicsProcess(false);
        }
        Position-=offset;
    }

}
