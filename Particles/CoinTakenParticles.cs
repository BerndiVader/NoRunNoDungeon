using Godot;
using System;

public class CoinTakenParticles : Particles
{
    public override void _Ready()
    {
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
