using Godot;
using System;

public class SfxPlayer : AudioStreamPlayer2D
{
    public override void _Ready()
    {
        Bus="Sfx";
        Connect("finished",this,nameof(OnFinished));
        Play(); 
    }

    private void OnFinished()
    {
        CallDeferred("queue_free");
    }

}
