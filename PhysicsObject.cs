using Godot;
using System;

public class PhysicsObject : KinematicBody2D
{
    protected float GRAVITY=300f;
    protected float friction=0.7f;

    protected Vector2 velocity=new Vector2(0f,0f);
    protected VisibilityNotifier2D notifier2D;

    public override void _Ready()
    {
        notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",this,nameof(onExitedScreen));
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
            Node node=(Node)collision.Collider;
            if(node.IsInGroup(GROUPS.PLATFORMS.ToString()))
            {
                Platform collider=(Platform)node;
                velocity.x+=collider.CurrentSpeed.x*0.5f;
            }
            velocity=velocity.Bounce(collision.Normal)*friction;
        }
    }

    void onExitedScreen()
    {
        QueueFree();
    }

}
