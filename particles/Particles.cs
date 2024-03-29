using Godot;
using System;

public class Particles : CPUParticles2D
{
    public override void _Ready()
    {
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",this,nameof(onExitedScreen));
        AddChild(notifier2D);
    }

    private void onExitedScreen() {
        if(!IsQueuedForDeletion())
        {
            SetProcess(false);
            SetPhysicsProcess(false);
            QueueFree();
        }
    }

}
