using Godot;
using System;

public class Dagger : Weapon
{
    private PackedScene shootParticles=ResourceUtils.particles[(int)PARTICLES.DAGGERSHOOT];
    public override void _Ready()
    {
        base._Ready();
        Connect("body_entered",this,nameof(onHitSomething));
        Connect("area_entered", this, nameof(onHitSomething));
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
                        throwDagger();
                    }

                    hit=false;
                    cooldown=0;
                    state=WEAPONSTATE.IDLE;
                }
                break;
            }
            case WEAPONSTATE.IDLE:
            {
                if(!animationPlayer.IsPlaying()&&AnimationNames.SETUP+getStringDirection()!=animationPlayer.CurrentAnimation)
                {
                    animationPlayer.Play(AnimationNames.SETUP+getStringDirection());
                }
                if(cooldown<5) 
                {
                    cooldown++;
                }
                break;
            }
        }        
    }

    public override bool attack()
    {
        if (state == WEAPONSTATE.IDLE && cooldown == 5)
        {
            playSfx(sfxSwing);
            animationPlayer.Play(AnimationNames.SWING + getStringDirection());
            state = WEAPONSTATE.ATTACK;
            return true;
        }
        return false;

    }
    private void throwDagger()
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
