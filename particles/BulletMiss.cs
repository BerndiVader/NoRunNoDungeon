using Godot;
using System;

public class BulletMiss : CPUParticles2D
{
    private static readonly AudioStream SFX=ResourceLoader.Load<AudioStream>("res://sounds/ingame/10_Battle_SFX/35_Miss_Evade_02.wav");
    public override void _Ready()
    {
        SetProcess(false);
        SetProcessInput(false);
        
        Renderer.instance.PlaySfx(SFX,GlobalPosition);
        OneShot=true;
        Emitting=true;
    }

    public override void _PhysicsProcess(float delta) 
    {
        if(!Emitting) 
        {
            SetPhysicsProcess(false);
            CallDeferred("queue_free");
        }
    }

}
