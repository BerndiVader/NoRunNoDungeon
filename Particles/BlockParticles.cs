using Godot;
using System;

public class BlockParticles : Particles
{

    public override void _Ready()
    {
    }

    public override void _Process(float delta) 
    {
        if(!Emitting) 
        {
            SetProcess(false);
            CallDeferred("queue_free");
        }
    }

    public void _enteredTree()
    {
        this.Emitting=true;
        SetProcess(true);
    }
}
