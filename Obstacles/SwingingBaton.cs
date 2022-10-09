using Godot;
using System;

public class SwingingBaton : Area2D
{
    [Export] private float minSpeed=0.01f;
    [Export] private float maxSpeed=0.09f;
    [Export] private int maxRotation=90;
    
    private Vector2 rotateTo,rot;

    public override void _Ready()
    {
        AddToGroup(GROUPS.OBSTACLES.ToString(),true);

        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",this,nameof(onExitedScreen));
        AddChild(notifier2D);

        rotateTo=new Vector2(Mathf.Deg2Rad(90),0);
        rot=new Vector2(Rotation,0);
        
        Connect("body_entered",this,nameof(onBodyEntered));
    }

    private void onBodyEntered(Node2D body)
    {
        if(body.IsInGroup(GROUPS.PLAYERS.ToString()))
        {
            body.EmitSignal(SIGNALS.Damage.ToString(),1f,this);
        }
    }

    private void onExitedScreen()
    {
        QueueFree();
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
