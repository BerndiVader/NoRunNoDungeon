using Godot;
using System;

public class SfxPlayer : AudioStreamPlayer2D
{
    public override void _Ready()
    {
        Connect("finished",this,nameof(onFinished));
        Play();
        
    }

    private void onFinished()
    {
        QueueFree();
    }

}
