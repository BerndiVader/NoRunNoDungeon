using Godot;
using System;

public class Zombie : KinematicMonster
{
    [Export] public float GRAVITY=300f;
    PackedScene BULLET;
    VisibilityNotifier2D notifier2D;

    Vector2 velocity=new Vector2(0f,0f);
    int cooldown;

    Player player;

    RayCast2D rayCast2D;
    Vector2 CASTTO;
    AnimatedSprite animationController;
    Placeholder parent;

    public override void _Ready()
    {
        notifier2D=new VisibilityNotifier2D();
        if(GetParent().GetType().Name=="Placeholder")
        {
            parent=(Placeholder)GetParent();
            notifier2D.Connect("screen_exited",parent,"exitedScreen");
        }
        else 
        {
            notifier2D.Connect("screen_exited",this,"exitedScreen");
        }
        AddChild(notifier2D);

        rayCast2D=(RayCast2D)GetNode("RayCast2D");
        rayCast2D.Enabled=true;
        CASTTO=rayCast2D.CastTo;

        animationController=(AnimatedSprite)GetNode("AnimatedSprite");
        state=STATE.IDLE;

        animationController.Play("default");
        animationController.FlipH=MathUtils.randomRangeInt(0,2)!=0;

        cooldown=0;

        if(animationController.FlipH)
        {
            rayCast2D.CastTo=rayCast2D.CastTo*-1;
        }

        BULLET=ResourceUtils.bullets[(int)BULLETS.TESTBULLET];

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
        if(rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==WorldUtils.world.player.GetInstanceId())
        {
            player=WorldUtils.world.player;
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
        float distance=rayCast2D.GlobalPosition.DistanceTo(player.GlobalPosition);
        if(distance<101)
        {
            Vector2 direction=new Vector2(rayCast2D.GlobalPosition.DirectionTo(player.GlobalPosition));
            direction=direction*distance;
            rayCast2D.CastTo=direction;
            if(rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==player.GetInstanceId())
            {
                if(cooldown<0)
                {
                    TestBullet bullet=(TestBullet)BULLET.Instance();
                    bullet.Position=getPosition();
                    bullet.direction=GlobalPosition.DirectionTo(player.GlobalPosition);
                    WorldUtils.world.level.AddChild(bullet);
                    cooldown=500;
                }
            }
            else {
                rayCast2D.CastTo=CASTTO;
                state=STATE.IDLE;
                player=null;
                cooldown=0;
            }
        } else
        {
            rayCast2D.CastTo=CASTTO;
            state=STATE.IDLE;
            player=null;
            cooldown=0;
        }
        cooldown--;
    }

    public override void fight(float delta)
    {
        throw new NotImplementedException();
    }

    public override void die(float delta)
    {
        throw new NotImplementedException();
    }

    public Vector2 getPosition()
    {
        return parent!=null?parent.Position+Position:Position;
    }


    void FlipH()
    {
        animationController.FlipH^=true;
        rayCast2D.CastTo*=-1;
    }

    public void _Free()
    {
        if(parent!=null)
        {
            parent.CallDeferred("queue_free");
        }
        else
        {
            CallDeferred("queue_free");
        }
    }

    void exitedScreen()
    {
        CallDeferred("queue_free");
    }
}
