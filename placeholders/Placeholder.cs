using Godot;
using Godot.Collections;
using System;

public class Placeholder : Node2D
{
    public Boolean isDisposed=false;
    public override void _Ready()
    {        
        Visible=false;
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

    protected override void Dispose(bool disposing)
    {
        isDisposed=true;
        base.Dispose(disposing);
    }

}
