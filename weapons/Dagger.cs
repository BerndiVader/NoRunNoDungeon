using Godot;
using System;

public class Dagger : Weapon
{
    private PackedScene shootParticles=ResourceUtils.particles[(int)PARTICLES.DAGGERSHOOT];
    public override void _Ready()
    {
        base._Ready();
        Connect("body_entered",this,nameof(OnHitSomething));
        Connect("area_entered", this, nameof(OnHitSomething));
        cooldown=5;
    }

    public override void _PhysicsProcess(float delta)
    {
        switch(state)
        {
            case WEAPONSTATE.ATTACK:
            {
                if(!animationPlayer.IsPlaying())
                {
                    if(!hit)
                    {
                        ThrowDagger();
                    }

                    hit=false;
                    cooldown=0;
                    state=WEAPONSTATE.IDLE;
                }
                break;
            }
            case WEAPONSTATE.IDLE:
            {
                if(!animationPlayer.IsPlaying()&&AnimationNames.SETUP+GetStringDirection()!=animationPlayer.CurrentAnimation)
                {
                    animationPlayer.Play(AnimationNames.SETUP+GetStringDirection());
                }
                if(cooldown<5) 
                {
                    cooldown++;
                }
                break;
            }
        }        
    }

    public override bool Attack()
    {
        if (state == WEAPONSTATE.IDLE && cooldown == 5)
        {
            PlaySfx(sfxSwing);
            animationPlayer.Play(AnimationNames.SWING + GetStringDirection());
            state = WEAPONSTATE.ATTACK;
            return true;
        }
        return false;

    }
    private void ThrowDagger()
    {
        DaggerShoot shoot=shootParticles.Instance<DaggerShoot>();
        shoot.Position=World.level.ToLocal(GetNode<Position2D>(nameof(Position2D)).GlobalPosition);
        shoot.Emitting=true;
        World.level.AddChild(shoot);
        DaggerBullet bullet=ResourceUtils.bullets[(int)BULLETS.DAGGERBULLET].Instance<DaggerBullet>();
        bullet.Position=World.instance.renderer.ToLocal(GlobalPosition);
        World.instance.renderer.AddChild(bullet);
    }

}
