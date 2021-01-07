using Godot;
using Godot.Collections;
using System;

public class Placeholder : Node2D
{
    public VisibilityNotifier2D notifier2D;
    public InstancePlaceholder placeholder;
    public bool instantiated;

    public override void _Ready()
    {
        instantiated=false;

        SetPhysicsProcess(false);
        SetProcessInput(false);
        SetProcess(true);

        notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_entered",this,nameof(enteredScreen));
        AddChild(notifier2D);

        placeholder=(InstancePlaceholder)GetChild(0);
    }

    public override void _Process(float delta)
    {
        if(instantiated)
        {
            placeholder.CreateInstance(true);
            SetProcess(false);
        }
    }

    public void enteredScreen()
    {
        if(placeholder==null)
        {
            placeholder=(InstancePlaceholder)GetChild(0);
        }
        ResourceUtils.worker.placeholderQueue.Enqueue(this);
    }

    void exitedScreen()
    {
        CallDeferred("queue_free");
    }
}
