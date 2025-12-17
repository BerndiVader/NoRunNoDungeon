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

        animationController.Play("idle");
        OnIdle();
    }

    public override void _PhysicsProcess(float delta)
    {
        if(animationPlayer.IsPlaying())
        {
            Position=startOffset+(ANIMATION_OFFSET*animationDirection);
        }
        goal(delta);
        Navigation(delta);
    }

    protected override void Navigation(float delta)
    {
        velocity+=FORCE*delta;
        velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);

        int slides = GetSlideCount();
        if(slides>0)
        {
            for(int i=0;i<slides;i++)
            {
                if(GetSlideCollision(i).Collider is Platform platform)
                {
                    velocity.x=platform.CurrentSpeed.x;
                }
                else
                {
                    velocity=StopX(velocity,delta);
                }
            }
        } 
        else
        {
            velocity=StopX(velocity,delta);
        }
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

    }

    protected override void Attack(float delta)
    {
        float distance=rayCast2D.GlobalPosition.DistanceTo(victim.GlobalPosition);
        if (distance < 41f)
        {
            Vector2 direction = rayCast2D.GlobalPosition.DirectionTo(victim.GlobalPosition);
            SetFlipH(direction.x < 0f);

            rayCast2D.CastTo = direction * distance;
            if (rayCast2D.IsColliding() && rayCast2D.GetCollider().GetInstanceId() == victim.GetInstanceId())
            {
                if (cooldown < 0 && !weapon.IsPlaying())
                {
                    weapon.Attack();
                    cooldown = 20;
                }
            }
            else
            {
                rayCast2D.CastTo = animationController.FlipH == true ? CASTTO * -1 : CASTTO;
                cooldown = 0;
                OnIdle();
            }
        }
        else
        {
            rayCast2D.CastTo = animationController.FlipH == true ? CASTTO * -1 : CASTTO;
            cooldown = 0;
            OnIdle();
        }
        cooldown--;
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
                EmitSignal(STATE.die.ToString());
            }
            else
            {
                staticBody.GetNode<CollisionShape2D>(nameof(CollisionShape2D)).SetDeferred("disabled", false);
                OnIdle();
            }
        }
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

    protected override void OnDamage(Node2D node=null, int amount=0)
    {
        if(state!=STATE.damage&&state!=STATE.die)
        {
            base.OnDamage(node, amount);
            if(GlobalPosition.x-node.GlobalPosition.x<0)
            {
                animationDirection=-1;
            }
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
        if(animationController.FlipH)
        {
            rayCast2D.CastTo=CASTTO*-1;
        } else
        {
            rayCast2D.CastTo=CASTTO;
        }
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
    }

}
