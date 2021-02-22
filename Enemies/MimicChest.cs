using Godot;
using System;

public class MimicChest : KinematicMonster
{
    [Export] public float GRAVITY=300f;

    Vector2 velocity=new Vector2(0f,0f);
    int cooldown;

    Player player;

    RayCast2D rayCast2D;
    RectangleShape2D collisionBox;
    Vector2 CASTTO;

    public override void _Ready()
    {
        base._Ready();

        collisionBox=(RectangleShape2D)collisionController.Shape;
        rayCast2D=(RayCast2D)GetNode("RayCast2D");
        rayCast2D.Enabled=true;
        CASTTO=rayCast2D.CastTo;

        animationController=(AnimatedSprite)GetNode("AnimatedSprite");
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

            if(node.IsInGroup("Platforms"))
            {
                Platform collider=(Platform)node;
                velocity.x+=collider.CurrentSpeed.x;
            }

        }

        tick(delta);
    
    }

    public override void idle(float delta)
    {
        Player player=WorldUtils.world.player;
        bool collide=collisionBox.Collide(GetGlobalTransform(),player.collisionController.Shape,player.GetGlobalTransform());
        if(collide||(rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==WorldUtils.world.player.GetInstanceId()))
        {
            player=WorldUtils.world.player;
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

    public override void attack(float delta)
    {
        if(animationController.Frame==2)
        {
            animationController.Play("fight");
            state=STATE.FIGHT;
        }
    }

    public override void fight(float delta)
    {
        Player player=WorldUtils.world.player;
        bool collide=collisionBox.Collide(GetGlobalTransform(),player.collisionController.Shape,player.GetGlobalTransform());

        if(collide)
        {
            player.EmitSignal("Damage",1f);
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
            else {
                rayCast2D.CastTo=CASTTO;
                animationController.Play("calm");
                state=STATE.CALM;
                player=null;
                cooldown=0;
            }
        } else
        {
            rayCast2D.CastTo=CASTTO;
            animationController.Play("calm");
            state=STATE.CALM;
            player=null;
            cooldown=0;
        }
        cooldown--;
    }

    public override void calm(float delta)
    {
        if(animationController.Frame==2)
        {
            state=STATE.IDLE;
            animationController.Play("idle");
        }
    }    

    public override void die(float delta)
    {
        base.die(delta);
    }

    public override void onPassanger(Player player)
    {
        if(state!=STATE.FIGHT)
        {
            base.onPassanger(player);
        }
        else
        {
            player.EmitSignal("Damage",1f);
        }
    }

    void FlipH()
    {
        animationController.FlipH^=true;
        rayCast2D.CastTo*=-1;
    }

}
