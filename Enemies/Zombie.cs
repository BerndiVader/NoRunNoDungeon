using Godot;
using System;

public class Zombie : KinematicMonster
{
    [Export] public float GRAVITY=300f;

    Vector2 velocity=Vector2.Zero;
    int cooldown;

    RayCast2D rayCast2D;
    Vector2 CASTTO;
    Staff weapon;

    public override void _Ready()
    {
        base._Ready();

        weapon=GetNode("Staff") as Staff;

        animationPlayer=GetNode("AnimationPlayer") as AnimationPlayer;
        animationPlayer.Connect("animation_started",this,nameof(animationPlayerStarts));
        animationPlayer.Connect("animation_finished",this,nameof(animationPlayerEnded));

        rayCast2D=GetNode("RayCast2D") as RayCast2D;
        rayCast2D.Enabled=true;
        CASTTO=rayCast2D.CastTo;

        animationController=GetNode("AnimatedSprite") as AnimatedSprite;
        state=STATE.IDLE;

        animationController.Play("default");
        animationController.FlipH=MathUtils.randomRangeInt(0,2)!=0;

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
        else
        {
            Vector2 force=new Vector2(0,GRAVITY);

            velocity+=GetFloorVelocity()*delta;
            velocity+=force*delta;

            KinematicCollision2D collision=MoveAndCollide(velocity*delta);

            if(collision!=null)
            {
                Node2D node=collision.Collider as Node2D;
                velocity=velocity.Bounce(collision.Normal)*0.01f;

                if(node.IsInGroup(GROUPS.PLATFORMS.ToString()))
                {
                    Platform collider=node as Platform;
                    GlobalPosition=new Vector2(collider.GlobalPosition.x,GlobalPosition.y);
                }
            }
        }
        tick(delta);

    }

    public override void idle(float delta)
    {
        if(rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==WorldUtils.world.player.GetInstanceId())
        {
            cooldown=0;
            state=STATE.ATTACK;
        }
        else if(cooldown>250) 
        {
            this.FlipH();
            cooldown=0;
        }
        cooldown++;
    }

    public override void attack(float delta)
    {
        float distance=rayCast2D.GlobalPosition.DistanceTo(WorldUtils.world.player.GlobalPosition);
        if(distance<41)
        {
            Vector2 direction=rayCast2D.GlobalPosition.DirectionTo(WorldUtils.world.player.GlobalPosition);
            FlipH(direction.x<0);

            direction=direction*distance;
            rayCast2D.CastTo=direction;
            if(rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==WorldUtils.world.player.GetInstanceId())
            {
                if(cooldown<0&&!weapon.animationPlayer.IsPlaying())
                {
                    weapon.attack();
                    cooldown=20;
                }
            }
            else {
                rayCast2D.CastTo=animationController.FlipH==true?CASTTO*-1:CASTTO;
                state=STATE.IDLE;
                cooldown=0;
            }
        } else
        {
            rayCast2D.CastTo=animationController.FlipH==true?CASTTO*-1:CASTTO;
            state=STATE.IDLE;
            cooldown=0;
        }
        cooldown--;
    }

    public override void fight(float delta)
    {
        throw new NotImplementedException();
    }

    public override void damage(float delta)
    {
        if(!animationPlayer.IsPlaying())
        {
            if(health<=0)
            {
                EmitSignal(SIGNALS.Die.ToString());
            }
            else
            {
                state=lastState;
            }
        }
    }

    public override void calm(float delta)
    {
        throw new NotImplementedException();
    }    

    public override void passanger(float delta)
    {
        if(!animationPlayer.IsPlaying())
        {
            base.passanger(delta);
        }
    }

    public override void die(float delta)
    {
        base.die(delta);
    }

    public override void onDamage(Player player, int amount)
    {
        if(state!=STATE.DAMAGE&&state!=STATE.DIE)
        {
            base.onDamage(player, amount);
            if(player.GlobalPosition.DirectionTo(GlobalPosition).Normalized().x<0)
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

    void FlipH()
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

    void FlipH(bool flip=false)
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
