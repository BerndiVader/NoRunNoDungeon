using System;
using Godot;

public class CheckpointMarker : AnimatedSprite
{

    private static readonly AudioStream sfx=ResourceLoader.Load<AudioStream>("res://sounds/ingame/PowerUp/Retro PowerUP 09.wav");

    private enum STATE
    {
        DOWN,
        FLYING,
        RAISE
    }

    private STATE state=STATE.DOWN;
    private STATE oldstate;

    private VisibilityNotifier2D notifier;

    public override void _Ready()
    {
        notifier=GetNode<VisibilityNotifier2D>(nameof(VisibilityNotifier2D));
        notifier.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));

        SetProcess(false);
        SetProcessInput(false);

        oldstate=state;
        Play(state.ToString());
    }

    public override void _PhysicsProcess(float delta)
    {
        if(Player.instance.GlobalPosition.x>GlobalPosition.x)
        {
            SetPhysicsProcess(false);
            state=STATE.RAISE;
            Connect("animation_finished",this,nameof(OnFlagRaised));
            Play(state.ToString());
            Renderer.instance.PlaySfx(sfx,Renderer.instance.ToLocal(GlobalPosition));
        }
    }

    private void OnFlagRaised()
    {
        state=STATE.FLYING;
        Disconnect("animation_finished",this,nameof(OnFlagRaised));
        Play(state.ToString());
    }
}
