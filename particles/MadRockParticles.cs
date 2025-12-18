using Godot;
using System;

public class MadRockParticles : Particles
{
    public override void _Ready()
    {
        base._Ready();
        OneShot=true;
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
