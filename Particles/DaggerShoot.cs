using Godot;
using System;

public class DaggerShoot : Particles
{
    public override void _Process(float delta)
    {
        if(!Emitting)
        {
            QueueFree();
        }
    }

}
