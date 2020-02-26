using Godot;
using System;

public class Placeholder : Node2D
{
    public VisibilityNotifier2D notifier2D;
    InstancePlaceholder placeholder;

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_entered",this,nameof(enteredScreen));
        AddChild(notifier2D);

        placeholder=(InstancePlaceholder)GetChild(0);
    }

    public void enteredScreen()
    {
        if(placeholder==null)
        {
            placeholder=(InstancePlaceholder)GetChild(0);
        }
        placeholder.ReplaceByInstance();
    }

    public void exitedScreen()
    {
        CallDeferred("queue_free");
    }

}
