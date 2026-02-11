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
    private Area2D area;
    private CPUParticles2D confetti;

    public override void _Ready()
    {
        notifier=GetNode<VisibilityNotifier2D>(nameof(VisibilityNotifier2D));
        notifier.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));

        area=GetNode<Area2D>(nameof(Area2D));
        area.Connect("body_entered",this,nameof(OnBodyEntered));

        confetti=GetNode<CPUParticles2D>("Confetti");

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

    private void OnBodyEntered(Node body)
    {
        if(state==STATE.RAISE&&Frame<3)
        {
            confetti.Emitting=true;
        }
    }

    private void OnFlagRaised()
    {
        state=STATE.FLYING;
        Disconnect("animation_finished",this,nameof(OnFlagRaised));
        Play(state.ToString());
    }
}
