using Godot;
using System;

public class MimicChest : KinematicMonster
{
    [Export] public float GRAVITY=300f;
    VisibilityNotifier2D notifier2D;

    Vector2 velocity=new Vector2(0f,0f);
    int cooldown;

    Player player;

    RayCast2D rayCast2D;
    Vector2 CASTTO;

    public override void _Ready()
    {
        base._Ready();
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
        if(rayCast2D.IsColliding()&&rayCast2D.GetCollider().GetInstanceId()==WorldUtils.world.player.GetInstanceId())
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

    void FlipH()
    {
        animationController.FlipH^=true;
        rayCast2D.CastTo*=-1;
    }

    void exitedScreen()
    {
        CallDeferred("queue_free");
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    public override string ToString()
    {
        return base.ToString();
    }

    public override object _Get(string property)
    {
        return base._Get(property);
    }

    public override Godot.Collections.Array _GetPropertyList()
    {
        return base._GetPropertyList();
    }

    public override void _Notification(int what)
    {
        base._Notification(what);
    }

    public override bool _Set(string property, object value)
    {
        return base._Set(property, value);
    }

    public override void _EnterTree()
    {
        base._EnterTree();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
    }

    public override string _GetConfigurationWarning()
    {
        return base._GetConfigurationWarning();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
    }

    public override void _UnhandledKeyInput(InputEventKey @event)
    {
        base._UnhandledKeyInput(@event);
    }

    public override void _Draw()
    {
        base._Draw();
    }

    public override void _InputEvent(Godot.Object viewport, InputEvent @event, int shapeIdx)
    {
        base._InputEvent(viewport, @event, shapeIdx);
    }

}
