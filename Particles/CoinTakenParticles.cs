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
            CallDeferred("queue_free");
        }

        Vector2 position=Position;
        position.y-=0.5f;
        Position=position;
    }

    public void _enteredTree()
    {
        this.Emitting=true;
        SetProcess(true);
    }

}
