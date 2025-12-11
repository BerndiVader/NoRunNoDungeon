using Godot;
using System;

public abstract class Bonus : Area2D
{
    public override void _Ready()
    {
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);
        
        GetNode<AnimatedSprite>(nameof(AnimatedSprite)).Play("default");
        Connect("body_entered",this,nameof(OnEnteredBody));
    }

    protected abstract void OnEnteredBody(Node body);

    public abstract void Apply(Player player);

}
