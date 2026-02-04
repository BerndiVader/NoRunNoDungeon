using Godot;
using System;

public class ExplodeParticles : Particles
{
    public override void _Ready()
    {
        base._Ready();

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
