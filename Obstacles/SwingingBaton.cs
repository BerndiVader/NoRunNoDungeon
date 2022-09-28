using Godot;
using System;

public class SwingingBaton : Area2D
{
    Placeholder parent;
    VisibilityNotifier2D notifier2D;
    Vector2 rotateTo,rot;

    [Export] public float minSpeed=0.01f;
    [Export] public float maxSpeed=0.09f;
    [Export] public int maxRotation=90;


    public override void _Ready()
    {
        AddToGroup(GROUPS.OBSTACLES.ToString(),true);

        notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",this,"exitedScreen");
        AddChild(notifier2D);

        rotateTo=new Vector2(Mathf.Deg2Rad(90),0);
        rot=new Vector2(Rotation,0);
        
        Connect("body_entered",this,nameof(onBodyEntered));
    }

    public void onBodyEntered(Node2D body)
    {
        if(body.IsInGroup(GROUPS.PLAYERS.ToString()))
        {
            body.EmitSignal(SIGNALS.Damage.ToString(),1f,this);
        }
    }

    public void exitedScreen()
    {
        CallDeferred("queue_free");
    }

    public override void _PhysicsProcess(float delta)
    {
        float speed=rot.x;
        rot=rot.LinearInterpolate(rotateTo,delta*3);
        speed=rot.x-speed;
        speed=MathUtils.minMax(minSpeed,maxSpeed,Math.Abs(speed))*Math.Sign(speed);
        Rotate(speed);

        if(RotationDegrees>maxRotation-1)
        {
            rotateTo=new Vector2(Mathf.Deg2Rad(maxRotation*-1),0);
        }
        else if(RotationDegrees<(maxRotation-1)*-1)
        {
            rotateTo=new Vector2(Mathf.Deg2Rad(maxRotation),0);
        }

    }

}
