using Godot;
using System;

public class Platform : StaticBody2D
{
    protected float damage=1f;
    protected float health=1f;
    protected Vector2 startPosition,lastPosition;
    protected VisibilityNotifier2D notifier2D;

    public Vector2 CurrentSpeed;


    public override void _Ready()
    {
        notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",this,nameof(exitedScreen));
        AddChild(notifier2D);
        
        startPosition=Position;
        lastPosition=startPosition;
        CurrentSpeed=new Vector2(0f,0f);
        
        AddToGroup(GROUPS.PLATFORMS.ToString());
    }

    public float collide(float damage) 
    {
        health-=damage;
        if(health<=0f)
        {
            CallDeferred("queue_free");
        }
        return this.damage;
    }

    void exitedScreen()
    {
        CallDeferred("queue_free");
    }

}
