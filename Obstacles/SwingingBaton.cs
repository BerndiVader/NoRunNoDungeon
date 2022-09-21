using Godot;
using System;

public class SwingingBaton : Area2D
{
    Placeholder parent;
    VisibilityNotifier2D notifier2D;
    Vector2 rotateTo,rot;

    public override void _Ready()
    {
        rotateTo=new Vector2(90,0);
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
        rot=new Vector2(RotationDegrees,0);
    }

    public void exitedScreen()
    {
        CallDeferred("queue_free");
    }

    public override void _PhysicsProcess(float delta)
    {
        rot=rot.LinearInterpolate(rotateTo,delta*2);
        
        RotationDegrees=rot.x;
        if(RotationDegrees>=89)
        {
            rotateTo=new Vector2(-90,0);
        }
        else if(RotationDegrees<=-89)
        {
            rotateTo=new Vector2(90,0);
        }
    }

}
