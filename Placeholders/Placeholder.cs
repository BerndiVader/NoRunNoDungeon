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
        notifier2D.Connect("screen_entered",this,nameof(enteredScreen));
        AddChild(notifier2D);
    }

    public override void _Process(float delta)
    {
        if(instantiated)
        {
            SetProcess(false);
            RemoveChild(placeholder);
            WorldUtils.world.level.AddChild(placeholder);
            placeholder.Set("position",WorldUtils.world.level.ToLocal(GlobalPosition));
            placeholder.CreateInstance(false);
            CallDeferred("queue_free");
        }
    }

    public void enteredScreen()
    {
        if(placeholder==null)
        {
            placeholder=GetChild(0) as InstancePlaceholder;
        }
        ResourceUtils.worker.placeholderQueue.Enqueue(this);
    }

}
