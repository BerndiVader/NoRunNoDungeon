using Godot;
using Godot.Collections;
using System;

public class Placeholder : Node2D
{
    private VisibilityNotifier2D notifier2D;
    public InstancePlaceholder placeholder;
    public bool instantiated;

    public override void _Ready()
    {
        instantiated=false;

        SetPhysicsProcess(false);
        SetProcessInput(false);
        SetProcess(true);

        notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_entered",this,nameof(onEnteredScreen));
        AddChild(notifier2D);
    }

    public override void _Process(float delta)
    {
        if(instantiated)
        {
            SetProcess(false);
            RemoveChild(placeholder);
            World.instance.level.AddChild(placeholder);
            placeholder.Set("position",World.instance.level.ToLocal(GlobalPosition));
            placeholder.CreateInstance(false);
            QueueFree();
        }
    }

    public void onEnteredScreen()
    {
        if(placeholder==null)
        {
            placeholder=GetChild<InstancePlaceholder>(0);
        }
        ResourceUtils.worker.placeholderQueue.Enqueue(this);
    }

}
