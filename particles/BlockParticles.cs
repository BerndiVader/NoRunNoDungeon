using Godot;
using System;

public class BlockParticles : CPUParticles2D
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
