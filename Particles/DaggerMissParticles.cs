using Godot;
using System;

public class DaggerMissParticles : CPUParticles2D
{
    public override void _Ready()
    {
        SetProcess(false);
    }

    public override void _Process(float delta) 
    {
        if(!Emitting) 
        {
            SetProcess(false);
            QueueFree();
        }
    }

    public void _enteredTree()
    {
        this.Emitting=true;
        SetProcess(true);
    }
}
