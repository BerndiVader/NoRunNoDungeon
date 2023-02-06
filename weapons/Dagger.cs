using Godot;
using System;

public class Dagger : Weapon
{
    private int cooldown;
    private PackedScene shootParticles=ResourceUtils.particles[(int)PARTICLES.DAGGERSHOOT];
    public override void _Ready()
    {
        base._Ready();
        Connect("body_entered",this,nameof(onHitSomething));
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

                    animationPlayer.Play("SETUP");
                    state=WEAPONSTATE.IDLE;
                    hit=false;
                    cooldown=0;
                }
                break;
            }
            case WEAPONSTATE.IDLE:
            {
                if(cooldown<5) 
                {
                    cooldown++;
                }
                break;
            }
        }        
    }

    public override void attack()
    {
        if(state==WEAPONSTATE.IDLE&&cooldown==5)
        {
            animationPlayer.Play("SWING");
            state=WEAPONSTATE.ATTACK;
        }
    }
    private void throwDagger()
    {
        DaggerShoot shoot=shootParticles.Instance<DaggerShoot>();
        shoot.Position=World.level.ToLocal(GetNode<Position2D>(nameof(Position2D)).GlobalPosition);
        shoot.Emitting=true;
        World.level.AddChild(shoot);
        DaggerBullet bullet=(DaggerBullet)((PackedScene)ResourceUtils.bullets[(int)BULLETS.DAGGERBULLET]).Instance();
        bullet.Position=World.instance.renderer.ToLocal(GlobalPosition);
        World.instance.renderer.AddChild(bullet);
    }

}
