using Godot;
using System;

public class MadRockParticles : CPUParticles2D
{
    public override void _Ready()
    {
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
