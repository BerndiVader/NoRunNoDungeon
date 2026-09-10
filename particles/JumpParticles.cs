using Godot;
using System;

public class JumpParticles : CPUParticles2D
{
    public override void _Ready()
    {
        SetPhysicsProcess(true);
        SetProcess(false);
        SetProcessInput(false);
    }

    public override void _PhysicsProcess(float delta)
    {
        float flip=Player.instance.AnimationController.FlipH?1f:0f;
        if(AnimOffset!=flip)
        {
            AnimOffset=flip;
        }
    }

    public void Start(bool flipped)
    {
        AnimOffset=flipped?1f:0f;
        if(!Emitting)
        {
            Restart();
        }
    }

    public void Stop()
    {
        Emitting=false;
    }

}
