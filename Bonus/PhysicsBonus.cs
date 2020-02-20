using Godot;
using System;

public class PhysicsBonus : KinematicBody2D
{
    protected float GRAVITY=300f;
    protected VisibilityEnabler2D notifier;

    protected Vector2 velocity=new Vector2(0f,0f);

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        SetProcess(false);
        SetProcessInput(false);

        notifier=new VisibilityEnabler2D();
        CallDeferred("add_child",notifier);
        notifier.Connect("screen_entered",this,nameof(_onScreenEntered));
        notifier.Connect("screen_exited",this,nameof(_onScreenExited));
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
            velocity=velocity.Bounce(collision.Normal)*0.5f;

            if(node.IsInGroup("Platforms"))
            {
                Platform collider=(Platform)node;
                velocity.x+=collider.CurrentSpeed.x*0.5f;
            }

        }
    }

    void _onScreenEntered() 
    {
        SetPhysicsProcess(true);
    }

    void _onScreenExited() 
    {
        SetPhysicsProcess(false);
        CallDeferred("queue_free");
    }    

}
