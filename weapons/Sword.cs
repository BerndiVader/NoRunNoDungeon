using Godot;
using System;

public class Sword : Weapon
{
    public override void _Ready()
    {
        base._Ready();
        Connect("body_entered",this,nameof(onHitSomething));
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
                if(!animationPlayer.IsPlaying())
                {
                    state=WEAPONSTATE.IDLE;
                    hit=false;
                }
                break;
        }
    }

    public override void attack()
    {
        if(state==WEAPONSTATE.IDLE)
        {
            playSfx(sfxSwing);
            animationPlayer.Play(AnimationNames.SWING+getStringDirection());
            state=WEAPONSTATE.ATTACK;
        }
    }
}
