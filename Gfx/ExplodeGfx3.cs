using Godot;
using System;

public class ExplodeGfx3 : ExplodeGfx
{
    public override void _Ready()
    {
        ZIndex=4;
        animationLength=this.Frames.GetFrameCount(this.Animation)-1;
        Play();
    }
        
}
