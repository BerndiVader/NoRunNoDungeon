using Godot;
using System;

public class Broadsword : Weapon
{
    CPUParticles2D swingParticles;

    public override void _Ready()
    {
        base._Ready();
        Connect("body_entered",this,nameof(onHitSomething));

        swingParticles=GetNode<CPUParticles2D>(nameof(CPUParticles2D));
        swingParticles.Emitting=false;
    }

    public override void _PhysicsProcess(float delta)
    {
        switch(state)
        {
            case WEAPONSTATE.ATTACK:
            {
                if(!animationPlayer.IsPlaying())
                {
                    animationPlayer.Play("SETUP");
                    state=WEAPONSTATE.IDLE;
                    hit=false;
                }
                break;
            }
        }        
    }

    public override void attack()
    {
        if(state==WEAPONSTATE.IDLE)
        {
            animationPlayer.Play("DOUBLE_SWING");
            state=WEAPONSTATE.ATTACK;
        }
    }
}
