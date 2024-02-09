using Godot;
using System;

public abstract class PhysicsObject : KinematicBody2D
{
    protected float friction=0.7f;
    protected Vector2 velocity=new Vector2(0f,0f);
    protected Vector2 GRAVITY=new Vector2(0,300f);

    public override void _Ready()
    {
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.onObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);
    }

    public override void _PhysicsProcess(float delta)
    {
        velocity+=GRAVITY*delta;

        KinematicCollision2D collision=MoveAndCollide(velocity*delta);

        if(collision!=null) 
        {
            velocity=velocity.Bounce(collision.Normal)*friction;

            Node node=(Node)collision.Collider;
            if(node.IsInGroup(GROUPS.PLATFORMS.ToString()))
            {
                Platform collider=(Platform)node;
                velocity.x+=collider.CurrentSpeed.x*0.35f;
            }

        }
    }

    public abstract void apply(Player player);

}
