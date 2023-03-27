using Godot;
using System;

public class MimicChest : KinematicMonster
{
    private int cooldown;
    private float shake;
    private float ShakeMax=0.6f;
    private RayCast2D rayCast2D;
    private Shape2D collisionBox;
    private Vector2 CASTTO;

    public override void _Ready()
    {
        base._Ready();

        collisionBox=collisionController.Shape;
        rayCast2D=GetNode<RayCast2D>("RayCast2D");
        rayCast2D.Enabled=true;
        CASTTO=rayCast2D.CastTo;

        animationController=GetNode<AnimatedSprite>("AnimatedSprite");

        animationController.Play("idle");
        animationController.FlipH=MathUtils.randomRangeInt(0,1)!=0;
        EmitSignal(STATE.idle.ToString());

        cooldown=0;

        if(animationController.FlipH)
        {
            rayCast2D.CastTo=rayCast2D.CastTo*-1;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
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

            if(shake!=0f)
            {
                applyShake();
            }

        }
        goal(delta);
    
    }

    protected override void idle(float delta)
    {
        float distance=GlobalPosition.DistanceTo(Player.instance.GlobalPosition);
        if(distance<12f||(rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==Player.instance.GetInstanceId()))
        {
            cooldown=0;
            animationController.Play("attack");
            onAttack(Player.instance);
            return;
        }
        else if(cooldown>99) 
        {
            this.FlipH();
            cooldown=0;
        }
        cooldown++;
    }

    protected override void attack(float delta)
    {
        if(animationController.Frame>1)
        {
            animationController.Play("fight");
            onFight(victim);
        }
    }

    protected override void fight(float delta)
    {
        float distance=GlobalPosition.DistanceTo(victim.GlobalPosition);

        if(distance<12f)
        {
            victim.EmitSignal(STATE.damage.ToString(),damageAmount,this);
        }
        else if(distance<100f)
        {
            Vector2 direction=new Vector2(GlobalPosition.DirectionTo(victim.GlobalPosition));
            rayCast2D.CastTo=direction*distance;
            if(rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==victim.GetInstanceId())
            {
                shake=0.3f;
            }
            else
            {
                onCalm();
            }
        }
        else
        {
            onCalm();
        }
    }

    protected override void calm(float delta)
    {
        if(animationController.Frame==2)
        {
            onIdle();
        }
    }    

    public override void onPassanger(Player player)
    {
        if(state!=STATE.fight)
        {
            base.onPassanger(player);
        }
        else
        {
            player.EmitSignal(STATE.damage.ToString(),damageAmount,this);
        }
    }

    protected override void onCalm()
    {
        if(state!=STATE.calm)
        {
            base.onCalm();
            rayCast2D.CastTo=CASTTO;
            animationController.Play("calm");
            victim=null;
            cooldown=0;            
        }
    }

    private void FlipH()
    {
        animationController.FlipH^=true;
        rayCast2D.CastTo*=-1;
    }

    private void applyShake()
    {
        shake=Math.Min(shake,ShakeMax);
        if(shake>=0.02f)
        {
            float offset=(float)MathUtils.randomRange(-shake,shake);
            Rotation=offset;
            shake*=0.9f;
        } 
        else if(shake>0f)
        {
            shake=0f;
            Rotation=0;
        }
    }


}
