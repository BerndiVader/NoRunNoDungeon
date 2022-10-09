using Godot;
using System;

public class Platform : StaticBody2D
{
    protected float damage=1f;
    protected float health=1f;
    protected Vector2 startPosition,lastPosition;
    public Vector2 CurrentSpeed;

    public override void _Ready()
    {
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",this,nameof(onExitedScreen));
        AddChild(notifier2D);
        
        startPosition=Position;
        lastPosition=startPosition;
        CurrentSpeed=new Vector2(0f,0f);
        
        AddToGroup(GROUPS.PLATFORMS.ToString());
    }

    private float collide(float damage) 
    {
        health-=damage;
        if(health<=0f)
        {
            QueueFree();
        }
        return this.damage;
    }

    private void onExitedScreen()
    {
        QueueFree();
    }

}
