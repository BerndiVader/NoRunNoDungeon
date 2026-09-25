using Godot;
using System;

public class Zombie : KinematicMonster
{
    [Export] protected float ATTACK_RANGE=41f;
    [Export] protected float ALERT_TIMING=0.05f;
    
    protected int cooldown;
    protected float alertTimer;
    protected float attackRangeSqrd;
    protected RayCast2D rayCast2D;
    protected Vector2 castTo;
    protected CPUParticles2D aura;
    protected MonsterWeapon weapon;
    
    public override void _Ready()
    {
        base._Ready();

        attackRangeSqrd=ATTACK_RANGE*ATTACK_RANGE;

        weapon=GetNode<MonsterWeapon>("Mace");
        aura=GetNode<CPUParticles2D>("Aura");
        aura.OneShot=true;
        aura.Emitting=false;

        animationPlayer=GetNode<AnimationPlayer>(nameof(AnimationPlayer));
        animationPlayer.Connect("animation_started",this,nameof(OnAnimationPlayerStarts));
        animationPlayer.Connect("animation_finished",this,nameof(OnAnimationPlayerEnded));

        rayCast2D=GetNode<RayCast2D>(nameof(RayCast2D));
        rayCast2D.Enabled=true;
        castTo=rayCast2D.CastTo;

        if(animationController.FlipH)
        {
            rayCast2D.CastTo=castTo*-1;
        } else
        {
            rayCast2D.CastTo=castTo;
        }

        if(weapon!=null)
        {
            weapon._Init();
        }
        cooldown=0;

        SetSpawnFacing();      
        OnIdle();
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        goal(delta);
    }

    protected override void Idle(float delta)
    {
        if(rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==Player.instance.GetInstanceId())
        {
            cooldown=0;
            OnAlert();
        }
        else if(cooldown>250)
        {
            FlipH();
            cooldown=0;
        }
        cooldown++;
        Navigation(delta);
    }

    protected override void Alert(float delta)
    {
        alertTimer+=delta;
        if(alertTimer>ALERT_TIMING)
        {
            alertTimer=0f;
            OnAttack(Player.instance);
        }

        Navigation(delta);
    }

    protected override void Attack(float delta)
    {
        float distance=rayCast2D.GlobalPosition.DistanceSquaredTo(victim.GlobalPosition);
        if(distance<attackRangeSqrd)
        {
            Vector2 direction=rayCast2D.GlobalPosition.DirectionTo(victim.GlobalPosition);
            SetFlipH(direction.x<0f);

            rayCast2D.CastTo=direction*Mathf.Sqrt(distance);
            if(rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==victim.GetInstanceId())
            {
                if(cooldown<0&&!weapon.IsPlaying())
                {
                    weapon.Attack();
                    cooldown=20;
                }
            }
            else
            {
                rayCast2D.CastTo=animationController.FlipH==true?castTo*-1:castTo;
                cooldown=0;
                OnIdle();
            }
        }
        else
        {
            rayCast2D.CastTo=animationController.FlipH==true?castTo*-1:castTo;
            cooldown=0;
            OnIdle();
        }
        cooldown--;
        Navigation(delta);
    }

    protected override void Fight(float delta)
    {
        throw new NotImplementedException();
    }

    protected override void Damage(float delta)
    {
        if(!animationPlayer.IsPlaying())
        {
            if(health<=0)
            {
                OnDie();
            }
            else
            {
                if(facing.x!=MathUtils.SignVector(GlobalPosition.DirectionTo(Player.instance.GlobalPosition)).x)
                {
                    FlipH();
                    cooldown=0;
                }
                OnIdle();

            }
        }
        Navigation(delta);
    }

    protected override void Calm(float delta)
    {
        throw new NotImplementedException();
    }    

    protected override void Passanger(float delta)
    {
        base.Passanger(delta);
    }

    protected override void Die(float delta)
    {
        base.Die(delta);
    }

    protected override void OnAlert()
    {
        onDelay=false;
        if(state!=STATE.alert&&state!=STATE.die)
        {
            velocity.y+=-50f;
            noSnap=true;

            lastState=state;
            state=STATE.alert;
            goal=Alert;
            alertTimer=0f;
            aura.Restart();
        }
    }

    protected override void OnDamage(Node2D node=null,float amount=0f,bool overrideDestroyable=false)
    {
        if(state!=STATE.damage&&state!=STATE.die)
        {
            base.OnDamage(node,amount);
            animationPlayer.Play("HIT");
        }
    }

    protected override void OnPassanger(Player player=null)
    {
        if(state!=STATE.passanger)
        {
            base.OnPassanger(player);
            animationPlayer.Play("PASSANGER");
        }
    }

    protected override void FlipH()
    {
        animationController.FlipH^=true;
        rayCast2D.CastTo=FlipX(rayCast2D.CastTo);
        facing=Facing();
    }

    protected virtual void SetFlipH(bool flip=false)
    {
        animationController.FlipH=flip;
        if(flip)
        {
            rayCast2D.CastTo=castTo*-1;
        } else
        {
            rayCast2D.CastTo=castTo;
        }
        facing=Facing();
    }

}
