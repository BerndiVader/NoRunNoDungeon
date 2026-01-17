using Godot;
using System;

public class Particles : CPUParticles2D
{
    public override void _Ready()
    {
        VisibilityNotifier2D notifier=new VisibilityNotifier2D();
        notifier.Connect("screen_exited",this,nameof(OnExitedScreen));
        AddChild(notifier);
    }

    protected void OnExitedScreen() {
        if(!IsQueuedForDeletion())
        {
            SetProcess(false);
            SetPhysicsProcess(false);
            CallDeferred("queue_free");
        }
    }

}
