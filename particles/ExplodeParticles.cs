using Godot;
using System;

public class ExplodeParticles : Particles
{
    public override void _Ready()
    {
        base._Ready();
        Emitting=true;
    }

    public override void _Process(float delta)
    {
        if(!Emitting)
        {
            QueueFree();
        }
    }

}
