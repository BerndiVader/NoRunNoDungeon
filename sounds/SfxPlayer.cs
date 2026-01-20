using Godot;
using System;

public class SfxPlayer : AudioStreamPlayer2D
{
    public override void _Ready()
    {
        MaxDistance=500f;
        Bus="Sfx";
        Connect("finished",this,nameof(OnFinished));
        Play(); 
    }

    private void OnFinished()
    {
        CallDeferred("queue_free");
    }

}
