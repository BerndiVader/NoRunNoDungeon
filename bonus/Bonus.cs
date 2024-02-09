using Godot;
using System;

public abstract class Bonus : Area2D
{
    public override void _Ready()
    {
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.onObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);
        
        GetNode<AnimatedSprite>("AnimatedSprite").Play("default");
        Connect("body_entered",this,nameof(onEnteredBody));
    }

    protected abstract void onEnteredBody(Node body);

    public abstract void apply(Player player);

}
