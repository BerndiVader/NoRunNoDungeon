using Godot;
using Godot.Collections;
using System;

public class Placeholder : Node2D
{
    public override void _Ready()
    {        
        SetPhysicsProcess(false);
        SetProcessInput(false);
        SetProcess(false);

        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_entered",this,nameof(onEnteredScreen));
        AddChild(notifier2D);
    }

    public void onEnteredScreen()
    {
        Worker.placeholders.Push(new WeakReference(this));
    }

}
