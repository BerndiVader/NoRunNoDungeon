using Godot;
using System;

public class Zombie : KinematicMonster
{
    private int cooldown;
    private RayCast2D rayCast2D;
    private Vector2 CASTTO;
    private MonsterWeapon weapon;
    
    public override void _Ready()
    {
        base._Ready();

        weapon=GetNode<MonsterWeapon>("Mace");

        animationPlayer=GetNode<AnimationPlayer>(nameof(AnimationPlayer));
        animationPlayer.Connect("animation_started",this,nameof(OnAnimationPlayerStarts));
        animationPlayer.Connect("animation_finished",this,nameof(OnAnimationPlayerEnded));

        rayCast2D=GetNode<RayCast2D>(nameof(RayCast2D));
        rayCast2D.Enabled=true;
        CASTTO=rayCast2D.CastTo;

        if(animationController.FlipH)
        {
            rayCast2D.CastTo=CASTTO*-1;
        } else
        {
            rayCast2D.CastTo=CASTTO;
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
            OnAttack(Player.instance);
        }
        else if(cooldown>250)
        {
            FlipH();
            cooldown=0;
        }
        cooldown++;
        Navigation(delta);
    }

    protected override void Attack(float delta)
    {
        float distance=rayCast2D.GlobalPosition.DistanceTo(victim.GlobalPosition);
        if (distance<41f)
        {
            Vector2 direction=rayCast2D.GlobalPosition.DirectionTo(victim.GlobalPosition);
            SetFlipH(direction.x<0f);

            rayCast2D.CastTo=direction*distance;
            if (rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==victim.GetInstanceId())
            {
                if(cooldown<0&&!weapon.IsPlaying())
                {
                    weapon.Attack();
                    cooldown=20;
                }
            }
            else
            {
                rayCast2D.CastTo=animationController.FlipH==true?CASTTO*-1:CASTTO;
                cooldown=0;
                OnIdle();
            }
        }
        else
        {
            rayCast2D.CastTo=animationController.FlipH==true?CASTTO*-1:CASTTO;
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
                staticBody.GetNode<CollisionShape2D>(nameof(CollisionShape2D)).SetDeferred("disabled", false);
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
        if(!animationPlayer.IsPlaying())
        {
            base.Passanger(delta);
        }
    }

    protected override void Die(float delta)
    {
        base.Die(delta);
    }

    protected override void OnDamage(Node2D node=null,float amount=0f)
    {
        if(state!=STATE.damage&&state!=STATE.die)
        {
            base.OnDamage(node,amount);
            animationPlayer.Play("HIT");
        }
    }

    public override void OnPassanger(Player player=null)
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

    private void SetFlipH(bool flip=false)
    {
        animationController.FlipH=flip;
        if(flip)
        {
            rayCast2D.CastTo=CASTTO*-1;
        } else
        {
            rayCast2D.CastTo=CASTTO;
        }
        facing=Facing();
    }

}
