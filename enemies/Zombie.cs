using Godot;
using System;
using System.Xml.Serialization;

public class Zombie : KinematicMonster
{

    private int cooldown;

    private RayCast2D rayCast2D;
    private Vector2 CASTTO;
    private MonsterWeapon weapon;
    private Vector2 snap = new Vector2(0f, 8f);

    public override void _Ready()
    {
        base._Ready();

        weapon=GetNode<MonsterWeapon>("Mace");

        animationPlayer=GetNode<AnimationPlayer>(nameof(AnimationPlayer));
        animationPlayer.Connect("animation_started",this,nameof(onAnimationPlayerStarts));
        animationPlayer.Connect("animation_finished",this,nameof(onAnimationPlayerEnded));

        rayCast2D=GetNode<RayCast2D>(nameof(RayCast2D));
        rayCast2D.Enabled=true;
        CASTTO=rayCast2D.CastTo;

        animationController.Play("idle");
        animationController.FlipH=MathUtils.randomRangeInt(1,3)==2;
        onIdle();

        cooldown=0;

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

    }

    public override void _PhysicsProcess(float delta)
    {
        if(animationPlayer.IsPlaying())
        {
            Position=startOffset+(ANIMATION_OFFSET*animationDirection);
        }
        goal(delta);
        navigation(delta);
    }

    protected override void navigation(float delta)
    {
        velocity+=FORCE*delta;
        velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);

        int slides = GetSlideCount();
        for(int i=0;i<slides;i++)
        {
            if(GetSlideCollision(i).Collider is Platform platform)
            {
                velocity.x=platform.CurrentSpeed.x;
            }
        }
    }

    protected override void idle(float delta)
    {
        if(rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==Player.instance.GetInstanceId())
        {
            cooldown=0;
            onAttack(Player.instance);
        }
        else if(cooldown>250)
        {
            FlipH();
            cooldown=0;
        }
        cooldown++;

    }

    protected override void attack(float delta)
    {
        float distance=rayCast2D.GlobalPosition.DistanceTo(victim.GlobalPosition);
        if (distance < 41f)
        {
            Vector2 direction = rayCast2D.GlobalPosition.DirectionTo(victim.GlobalPosition);
            SetFlipH(direction.x < 0f);

            rayCast2D.CastTo = direction * distance;
            if (rayCast2D.IsColliding() && rayCast2D.GetCollider().GetInstanceId() == victim.GetInstanceId())
            {
                if (cooldown < 0 && !weapon.isPlaying())
                {
                    weapon.attack();
                    cooldown = 20;
                }
            }
            else
            {
                rayCast2D.CastTo = animationController.FlipH == true ? CASTTO * -1 : CASTTO;
                cooldown = 0;
                onIdle();
            }
        }
        else
        {
            rayCast2D.CastTo = animationController.FlipH == true ? CASTTO * -1 : CASTTO;
            cooldown = 0;
            onIdle();
        }
        cooldown--;
    }

    protected override void fight(float delta)
    {
        throw new NotImplementedException();
    }

    protected override void damage(float delta)
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
                onIdle();
            }
        }
    }

    protected override void calm(float delta)
    {
        throw new NotImplementedException();
    }    

    protected override void passanger(float delta)
    {
        if(!animationPlayer.IsPlaying())
        {
            base.passanger(delta);
        }
    }

    protected override void die(float delta)
    {
        base.die(delta);
    }

    protected override void onDamage(Player player=null, int amount=0)
    {
        if(state!=STATE.damage&&state!=STATE.die)
        {
            base.onDamage(player, amount);
            if(GlobalPosition.x-player.GlobalPosition.x<0)
            {
                animationDirection=-1;
            }
            animationPlayer.Play("HIT");
        }
    }

    public override void onPassanger(Player player=null)
    {
        if(state!=STATE.passanger)
        {
            base.onPassanger(player);
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
