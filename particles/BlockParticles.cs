using Godot;
using System;

public class BlockParticles : Particles
{

    public override void _Ready()
    {
        base._Ready();
        
        OneShot=true;
        Emitting=true;
    }

    public override void _PhysicsProcess(float delta) 
    {
        if(!Emitting) 
        {
            CallDeferred("queue_free");
            SetPhysicsProcess(false);
        }
    }

}
