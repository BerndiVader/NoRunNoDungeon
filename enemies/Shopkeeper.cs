using Godot;
using System;

public class Shopkeeper : KinematicMonster
{
    private AudioStreamPlayer player;

    public override void _Ready()
    {
        player=GetNode<AudioStreamPlayer>(nameof(AudioStreamPlayer));
        player.Connect("finished",this,nameof(OnMusicStop));
        player.Play();

        base._Ready();

		SetSpawnFacing();
        OnIdle();
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
		goal(delta);

        if(!notifier2D.IsOnScreen())
        {
            QueueFree();
        }

    }

    protected override void Idle(float delta)
    {
        Navigation(delta);
    }

    protected override void OnDamage(Node2D node=null,float amount=0f)
    {
        if(state!=STATE.damage&&state!=STATE.die)
        {
            return;
        }

    }

    private void OnMusicStop()
    {
        player.Play();
    }

}
