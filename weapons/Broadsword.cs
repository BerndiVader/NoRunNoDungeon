using Godot;
using System;

public class Broadsword : Weapon
{
    CPUParticles2D swingParticles;

    public override void _Ready()
    {
        base._Ready();

        swingParticles=GetNode<CPUParticles2D>(nameof(CPUParticles2D));
        swingParticles.Emitting=false;        

        Connect("body_entered",this,nameof(OnHitSomething));
        Connect("area_entered",this,nameof(OnHitSomething));
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        
        switch(state)
        {
            case WEAPONSTATE.IDLE:
                if(!animationPlayer.IsPlaying()&&AnimationNames.SETUP+GetStringDirection()!=animationPlayer.CurrentAnimation)
                {
                    animationPlayer.Play(AnimationNames.SETUP+GetStringDirection());
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

    public override bool Attack()
    {
        if(state==WEAPONSTATE.IDLE)
        {
            PlaySfx(sfxSwing);
            animationPlayer.Play(AnimationNames.DOUBLE_SWING+GetStringDirection());
            state=WEAPONSTATE.ATTACK;
            return true;
        }
        return false;
    }
}
