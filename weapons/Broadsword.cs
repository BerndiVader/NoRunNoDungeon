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
            case WEAPONSTATE.IDLE:
                if(!animationPlayer.IsPlaying()&&AnimationNames.SETUP+getStringDirection()!=animationPlayer.CurrentAnimation)
                {
                    animationPlayer.Play(AnimationNames.SETUP+getStringDirection());
                }
                break;
            case WEAPONSTATE.ATTACK:
            {
                if(!animationPlayer.IsPlaying())
                {
                    state=WEAPONSTATE.IDLE;
                    hit=false;
                }
                break;
            }
        }        
    }

    public override bool attack()
    {
        if (state == WEAPONSTATE.IDLE)
        {
            playSfx(sfxSwing);
            animationPlayer.Play(AnimationNames.DOUBLE_SWING + getStringDirection());
            state = WEAPONSTATE.ATTACK;
            return true;
        }
        return false;
    }
}
