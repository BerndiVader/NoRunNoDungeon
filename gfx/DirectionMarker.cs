using Godot;
using System;

public class DirectionMarker : AnimatedSprite
{
    private VisibilityNotifier2D notifier;

    public override void _Ready()
    {
        notifier=GetNode<VisibilityNotifier2D>(nameof(VisibilityNotifier2D));
        notifier.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));
        
        Play("default");
    }

}
