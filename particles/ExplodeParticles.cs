using Godot;
using System;

public class ExplodeParticles : CPUParticles2D
{
    public override void _Ready()
    {
        SetProcess(false);
        SetProcessInput(false);
        
        OneShot=true;
        Emitting=true;
    }

    public override void _PhysicsProcess(float delta)
    {
        if(!Emitting)
        {
            SetPhysicsProcess(false);
            QueueFree();
        }
    }

}
