using Godot;
using Godot.Collections;
using System;

public class Placeholder : Node2D
{

    [Signal]
    public delegate void Create(InstancePlaceholder iHolder);

    public bool isDisposed=false;
    public override void _Ready()
    {
        Visible=false;
        SetPhysicsProcess(false);
        SetProcessInput(false);
        SetProcess(false);

        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_entered",this,nameof(OnEnteredScreen));
        AddChild(notifier2D);

        Connect(nameof(Create),this,nameof(OnCreate));
    }

    public void OnDeferredCreate(InstancePlaceholder iHolder)
    {
        RemoveChild(iHolder);
        iHolder.Set("position",Position);
        World.level.AddChild(iHolder);
        iHolder.CreateInstance(false);
        iHolder.QueueFree();
        QueueFree();
    }

    private void OnCreate(InstancePlaceholder iHolder,bool deferred)
    {
        if(deferred)
        {
            CallDeferred("OnDeferredCreate",iHolder);
        }
        else
        {
            RemoveChild(iHolder);
            iHolder.Set("position",Position);
            World.level.CallDeferred("add_child",iHolder);
            iHolder.CallDeferred("create_instance",false);
            iHolder.CallDeferred("queue_free");
            CallDeferred("queue_free");
        }

    }

    public void OnEnteredScreen()
    {
        Worker.placeholders.Push(new WeakReference(this));
    }

    protected override void Dispose(bool disposing)
    {
        isDisposed=true;
        base.Dispose(disposing);
    }

}
