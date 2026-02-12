using Godot;
using System;

public class MadRockParticles : CPUParticles2D
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
            QueueFree();
            SetPhysicsProcess(false);
        }
    }

}
