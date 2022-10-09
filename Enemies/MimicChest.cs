using Godot;
using System;

public class MimicChest : KinematicMonster
{
    [Export] private float GRAVITY=300f;

    private Vector2 velocity=new Vector2(0f,0f);
    private int cooldown;

    private RayCast2D rayCast2D;
    private RectangleShape2D collisionBox;
    private Vector2 CASTTO;

    public override void _Ready()
    {
        base._Ready();

        collisionBox=(RectangleShape2D)collisionController.Shape;
        rayCast2D=GetNode<RayCast2D>("RayCast2D");
        rayCast2D.Enabled=true;
        CASTTO=rayCast2D.CastTo;

        animationController=GetNode<AnimatedSprite>("AnimatedSprite");
        state=STATE.IDLE;

        animationController.Play("idle");
        animationController.FlipH=MathUtils.randomRangeInt(0,2)!=0;

        cooldown=0;

        if(animationController.FlipH)
        {
            rayCast2D.CastTo=rayCast2D.CastTo*-1;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        Vector2 force=new Vector2(0,GRAVITY);

        velocity+=GetFloorVelocity()*delta;
        velocity+=force*delta;

        KinematicCollision2D collision=MoveAndCollide(velocity*delta);

        if(collision!=null)
        {
            Node2D node=(Node2D)collision.Collider;
            velocity=velocity.Bounce(collision.Normal)*0.01f;

            if(node.IsInGroup(GROUPS.PLATFORMS.ToString()))
            {
                Platform collider=(Platform)node;
                GlobalPosition=new Vector2(collider.GlobalPosition.x,GlobalPosition.y);
            }

        }

        tick(delta);
    
    }

    protected override void idle(float delta)
    {
        bool collide=collisionBox.Collide(GetGlobalTransform(),World.instance.player.collisionShape.Shape,World.instance.player.GetGlobalTransform());
        if(collide||(rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==World.instance.player.GetInstanceId()))
        {
            cooldown=0;
            animationController.Play("attack");
            state=STATE.ATTACK;
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
        if(animationController.Frame==2)
        {
            animationController.Play("fight");
            state=STATE.FIGHT;
        }
    }

    protected override void fight(float delta)
    {
        Player player=World.instance.player;
        bool collide=collisionBox.Collide(GetGlobalTransform(),player.collisionShape.Shape,player.GetGlobalTransform());

        if(collide)
        {
            player.EmitSignal(SIGNALS.Damage.ToString(),damageAmount,this);
        }

        float distance=GlobalPosition.DistanceTo(player.GlobalPosition);
        if(distance<101)
        {
            Vector2 direction=new Vector2(GlobalPosition.DirectionTo(player.GlobalPosition));
            direction=direction*distance;
            rayCast2D.CastTo=direction;
            if(rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==player.GetInstanceId())
            {
                if(cooldown<0)
                {
                    cooldown=10;
                }
            }
            else
            {
                rayCast2D.CastTo=CASTTO;
                animationController.Play("calm");
                state=STATE.CALM;
                player=null;
                cooldown=0;
            }
        }
        else
        {
            rayCast2D.CastTo=CASTTO;
            animationController.Play("calm");
            state=STATE.CALM;
            player=null;
            cooldown=0;
        }
        cooldown--;
    }

    protected override void calm(float delta)
    {
        if(animationController.Frame==2)
        {
            state=STATE.IDLE;
            animationController.Play("idle");
        }
    }    

    protected override void die(float delta)
    {
        base.die(delta);
    }

    protected override void onPassanger(Player player)
    {
        if(state!=STATE.FIGHT)
        {
            base.onPassanger(player);
        }
        else
        {
            player.EmitSignal(SIGNALS.Damage.ToString(),damageAmount);
        }
    }

    private void FlipH()
    {
        animationController.FlipH^=true;
        rayCast2D.CastTo*=-1;
    }

}
