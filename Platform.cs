using Godot;
using System;

public class Platform : StaticBody2D
{
    protected float damage=1f;
    protected float health=1f;
    protected bool falling=false;
    protected Vector2 startPosition;
    
    protected VisibilityNotifier2D visibleNotifier;


    public override void _Ready()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);
        startPosition=Position;

        SetCollisionLayerBit(0,false);
        SetCollisionLayerBit(1,true);     

        visibleNotifier=new VisibilityNotifier2D();
        visibleNotifier.Connect("screen_entered",this,nameof(enteredScreen));
        visibleNotifier.Connect("screen_exited",this,nameof(exitedScreen));
        CallDeferred("add_child",visibleNotifier);
    }

    public float collide(float damage) 
    {
        health-=damage;
        if(health<=0f) QueueFree();
        return this.damage;
    }

    public void enteredScreen() 
    {
        SetProcess(true);
        SetPhysicsProcess(true);
    }

    public void exitedScreen()
    {
        if(!IsQueuedForDeletion())
        {
            SetProcess(false);
            SetPhysicsProcess(false);
            CallDeferred("queue_free");
        }
    }

}
