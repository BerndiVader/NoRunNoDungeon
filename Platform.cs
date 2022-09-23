using Godot;
using System;

public class Platform : StaticBody2D
{
    protected float damage=1f;
    protected float health=1f;
    protected bool falling=false;
    protected Vector2 startPosition,lastPosition;
    protected VisibilityNotifier2D notifier2D;
    protected Placeholder parent;

    public Vector2 CurrentSpeed;


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
        
        startPosition=Position;
        AddToGroup(GROUPS.PLATFORMS.ToString());
        lastPosition=Position;
    }

    public float collide(float damage) 
    {
        health-=damage;
        if(health<=0f) QueueFree();
        return this.damage;
    }

    public override void _PhysicsProcess(float delta)
    {
        CurrentSpeed=(Position-lastPosition)/delta;
        lastPosition=Position;
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
