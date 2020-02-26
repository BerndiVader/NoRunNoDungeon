using Godot;
using System;

public class Platform : StaticBody2D
{
    protected float damage=1f;
    protected float health=1f;
    protected bool falling=false;
    protected Vector2 startPosition,lastPosition;
    protected Placeholder parent;
    protected VisibilityNotifier2D notifier2D;

    public Vector2 CurrentSpeed;


    public override void _Ready()
    {
        parent=(Placeholder)GetParent();
        
        startPosition=Position;

        SetCollisionLayerBit(0,false);
        SetCollisionLayerBit(1,true);     

        notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",parent,"exitedScreen");
        AddChild(notifier2D);

        AddToGroup("Platforms");

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

}
