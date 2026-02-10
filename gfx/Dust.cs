using Godot;
using System;

public class Dust : AnimatedSprite
{
    public enum TYPE
    {
        FALL,
        JUMP,
        RUN,
        APPEAR,
        DISAPEAR
    }

    public TYPE type;

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        SetProcess(false);
        SetProcessInput(false);

        if((int)type>2)
        {
            Offset=Vector2.Zero;
        }
        
        Connect("animation_finished",this,nameof(OnFinish));
        Play(type.ToString());
    }

    private void OnFinish()
    {
        CallDeferred("queue_free");
    }

}
