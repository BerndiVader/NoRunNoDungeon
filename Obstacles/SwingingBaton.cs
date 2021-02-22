using Godot;
using System;

public class SwingingBaton : Area2D
{
    Placeholder parent;
    VisibilityNotifier2D notifier2D;

    public override void _Ready()
    {
        notifier2D=new VisibilityNotifier2D();
    }

}
