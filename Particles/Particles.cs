using Godot;
using System;

public class Particles : CPUParticles2D
{
    VisibilityNotifier2D notifier2D;

    public override void _Ready()
    {
        notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",this,nameof(exitedScreen));
        CallDeferred("add_child",notifier2D);
    }

    public void exitedScreen() {
        if(!IsQueuedForDeletion())
        {
            SetProcess(false);
            SetPhysicsProcess(false);
            CallDeferred("queue_free");
        }
    }

}
