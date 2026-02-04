using Godot;
using System;

public class Dust : AnimatedSprite
{
    public enum TYPE
    {
        FALL,
        JUMP,
        RUN,
    }

    public TYPE type;

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        SetProcess(false);
        SetProcessInput(false);
        
        Connect("animation_finished",this,nameof(OnFinish));
        Play(type.ToString());
    }

    private void OnFinish()
    {
        CallDeferred("queue_free");
    }

}
