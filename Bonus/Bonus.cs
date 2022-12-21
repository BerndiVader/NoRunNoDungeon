using Godot;
using System;

public class Bonus : Area2D
{
    public override void _Ready()
    {
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.onObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);
        
        GetNode<AnimatedSprite>("AnimatedSprite").Play("default");
    }

}
