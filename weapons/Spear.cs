using Godot;
using System;

public class Spear : Weapon
{

    private int facing;

    public override void _Ready()
    {
        base._Ready();
        Connect("body_entered",this,nameof(OnHitSomething));
        Connect("area_entered", this, nameof(OnHitSomething));
    }

    public override void _PhysicsProcess(float delta)
    {
        switch(state)
        {

            case WEAPONSTATE.IDLE:
                if(!animationPlayer.IsPlaying()&&AnimationNames.SETUP+GetStringDirection()!=animationPlayer.CurrentAnimation)
                {
                    animationPlayer.Play(AnimationNames.SETUP+GetStringDirection());
                }
                break;
            case WEAPONSTATE.ATTACK:
                if(!animationPlayer.IsPlaying())
                {
                    if(!hit)
                    {
                        ThrowDagger();
                    }
                    state=WEAPONSTATE.IDLE;
                    hit=false;
                }
                break;        }        
    }

    public override bool Attack()
    {
        if(state==WEAPONSTATE.IDLE&&CooldownReady())
        {

            facing=Player.instance.AnimationController.FlipH?-1:1;
            cooldownTimer.WaitTime=COOLDOWN;
            cooldownTimer.Start();
            animationPlayer.Play(AnimationNames.SWING+GetStringDirection());
            state=WEAPONSTATE.ATTACK;
            return true;
        }
        return false;
    }

    private void ThrowDagger()
    {
        DaggerShoot shoot=ResourceUtils.particles[(int)PARTICLES.DAGGERSHOOT].Instance<DaggerShoot>();
        shoot.Position=World.level.ToLocal(GetNode<Position2D>(nameof(Position2D)).GlobalPosition);
        shoot.Emitting=true;
        World.level.AddChild(shoot);
        
        SpearBullet bullet=SpearBullet.Create(facing);
        bullet.Position=World.level.ToLocal(GlobalPosition);
        World.level.AddChild(bullet);
    }

}
