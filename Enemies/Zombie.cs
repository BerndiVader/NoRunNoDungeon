using Godot;
using System;

public class Zombie : KinematicMonster
{

    private Vector2 velocity=Vector2.Zero;
    private int cooldown;

    private RayCast2D rayCast2D;
    private Vector2 CASTTO;
    private Staff weapon;

    public override void _Ready()
    {
        base._Ready();

        weapon=GetNode<Staff>("Staff");

        animationPlayer=GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.Connect("animation_started",this,nameof(onAnimationPlayerStarts));
        animationPlayer.Connect("animation_finished",this,nameof(onAnimationPlayerEnded));

        rayCast2D=GetNode<RayCast2D>("RayCast2D");
        rayCast2D.Enabled=true;
        CASTTO=rayCast2D.CastTo;

        animationController=GetNode<AnimatedSprite>("AnimatedSprite");
        state=STATE.idle;

        animationController.Play("idle");
        animationController.FlipH=MathUtils.randomRangeInt(1,2)!=1;

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

        velocity+=force*delta;
        KinematicCollision2D collision=MoveAndCollide(velocity*delta);
        if(collision!=null)
        {
            velocity=velocity.Bounce(collision.Normal)*friction;

            Node2D node=(Node2D)collision.Collider;
            if(node.IsInGroup(GROUPS.PLATFORMS.ToString()))
            {
                Platform collider=(Platform)node;
                velocity.x+=collider.CurrentSpeed.x*1.8f;
            }
        }

        tick(delta);
    }

    protected override void idle(float delta)
    {
        if(rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==World.instance.player.GetInstanceId())
        {
            cooldown=0;
            onAttack(World.instance.player);
        }
        else if(cooldown>250)
        {
            this.FlipH();
            cooldown=0;
        }
        cooldown++;
    }

    protected override void attack(float delta)
    {
        float distance=rayCast2D.GlobalPosition.DistanceTo(victim.GlobalPosition);
        if(distance<41)
        {
            Vector2 direction=rayCast2D.GlobalPosition.DirectionTo(victim.GlobalPosition);
            FlipH(direction.x<0);

            rayCast2D.CastTo=direction*distance;
            if(rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==victim.GetInstanceId())
            {
                if(cooldown<0&&!weapon.isPlaying())
                {
                    weapon.attack();
                    cooldown=20;
                }
            }
            else {
                rayCast2D.CastTo=animationController.FlipH==true?CASTTO*-1:CASTTO;
                state=STATE.idle;
                cooldown=0;
            }
        } else
        {
            rayCast2D.CastTo=animationController.FlipH==true?CASTTO*-1:CASTTO;
            onIdle();
            cooldown=0;
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
                state=lastState;
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

    protected override void onDamage(Player player, int amount)
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

    public override void onPassanger(Player player)
    {
        base.onPassanger(player);
        animationPlayer.Play("PASSANGER");
    }

    private void FlipH()
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

    private void FlipH(bool flip=false)
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
