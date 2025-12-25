using Godot;
using System;

public class BulletMiss : Particles
{
    private static AudioStream sfx=ResourceLoader.Load<AudioStream>("res://sounds/ingame/10_Battle_SFX/35_Miss_Evade_02.wav");
    public override void _Ready()
    {
        base._Ready();
        World.instance.renderer.PlaySfx(sfx,Position);
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
    }

}
