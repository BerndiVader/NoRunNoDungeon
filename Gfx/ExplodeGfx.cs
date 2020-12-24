using Godot;
using System;

public class ExplodeGfx : AnimatedSprite
{
    protected int animationLength;
    public override void _Ready()
    {
        ZIndex=4;
        animationLength=this.Frames.GetFrameCount(this.Animation)-1;
        Play();
    }

    public override void _Process(float delta)
    {
        if(Frame==animationLength)
        {
            CallDeferred("queue_free");
        }
    }
}
