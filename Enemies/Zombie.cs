using Godot;
using System;

public class Zombie : KinematicBody2D
{
    [Export] public float GRAVITY=300f;
    PackedScene BULLET;
    VisibilityNotifier2D notifier2D;

    Vector2 velocity=new Vector2(0f,0f);
    int cooldown;

    Player player;

    enum STATE
    {
        IDLE,
        ATTACK
    }

    RayCast2D rayCast2D;
    Vector2 CASTTO;
    AnimatedSprite animationController;
    STATE sTATE;

    Placeholder parent;

    public override void _Ready()
    {
        if(GetParent().GetType().Name=="Placeholder")
        {
            parent=(Placeholder)GetParent();
            parent.notifier2D.Connect("screen_exited",this,"exitedScreen");
        }
        else
        {
            notifier2D=new VisibilityNotifier2D();
            AddChild(notifier2D);
            notifier2D.Connect("screen_exited",this,"exitedScreen");
        }

        rayCast2D=(RayCast2D)GetNode("RayCast2D");
        rayCast2D.Enabled=true;
        CASTTO=rayCast2D.CastTo;

        animationController=(AnimatedSprite)GetNode("AnimatedSprite");
        sTATE=STATE.IDLE;

        animationController.Play("default");
        animationController.FlipH=MathUtils.randomRangeInt(0,2)!=0;

        cooldown=0;

        if(animationController.FlipH)
        {
            rayCast2D.CastTo=rayCast2D.CastTo*-1;
        }

        BULLET=ResourceUtils.bullets[0];

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

        switch(sTATE)
        {
            case STATE.ATTACK:
            {
                Vector2 d=new Vector2(rayCast2D.GlobalPosition.DirectionTo(player.GlobalPosition));
                d=d*rayCast2D.GlobalPosition.DistanceTo(player.GlobalPosition);
                rayCast2D.CastTo=d;
                if(rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==player.GetInstanceId())
                {
                    if(cooldown>5)
                    {
                        TestBullet bullet=(TestBullet)BULLET.Instance();
                        bullet.Position=getPosition();
                        bullet.direction=GlobalPosition.DirectionTo(player.GlobalPosition);
                        WorldUtils.world.level.AddChild(bullet);
                        cooldown=0;
                    }
                }
                else {
                    rayCast2D.CastTo=CASTTO;
                    sTATE=STATE.IDLE;
                    player=null;
                }
                cooldown++;
                break;
            }
            default:
            {
                if(rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==WorldUtils.world.player.GetInstanceId())
                {
                    player=WorldUtils.world.player;
                    cooldown=0;
                    sTATE=STATE.ATTACK;
                }
                else {
                    rayCast2D.CastTo=rayCast2D.CastTo*-1;
                }
                break;
            }
        }
    
    }
    public Vector2 getPosition()
    {
        return parent!=null?parent.Position+Position:Position;
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
