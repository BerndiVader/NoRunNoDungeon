using Godot;
using System;

public class CoinTakenParticles : CPUParticles2D
{
    static Vector2 offset=new Vector2(0f,0.5f);

    public override void _Ready()
    {
        Emitting=true;
    }

    public override void _PhysicsProcess(float delta) 
    {
        if(!Emitting) 
        {
            QueueFree();
        }

        Position-=offset;
    }

}
