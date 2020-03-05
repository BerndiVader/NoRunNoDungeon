using Godot;
using System;

public class PhysicsObject : KinematicBody2D
{
    protected float GRAVITY=300f;
    protected VisibilityNotifier2D notifier2D;

    protected Vector2 velocity=new Vector2(0f,0f);

    protected Placeholder parent;

    public override void _Ready()
    {
        parent=(Placeholder)GetParent();

        notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",parent,"exitedScreen");
        AddChild(notifier2D);
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
            float friction=node.IsInGroup("Platforms")?0.5f:0.7f;
            velocity=velocity.Bounce(collision.Normal)*friction;

            if(node.IsInGroup("Platforms"))
            {
                Platform collider=(Platform)node;
                velocity.x+=collider.CurrentSpeed.x*0.5f;
            }

        }
    }

}