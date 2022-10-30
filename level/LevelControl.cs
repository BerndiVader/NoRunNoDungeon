using Godot;
using System;

public class LevelControl : Node2D
{
    VisibilityNotifier2D notifier;
    [Export] private bool EnableSpeed=false;
    [Export] private float Speed=0f;
    [Export] private bool EnableZoom=false;
    [Export] private float Zoom=0f;

    public override void _Ready()
    {
        Visible=false;
        notifier=new VisibilityNotifier2D();
        notifier.Connect("screen_entered",this,nameof(onScreenEntered));
        notifier.Connect("screen_exited",this,nameof(onScreenExited));
        AddChild(notifier);

        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);
    }

    public override void _Process(float delta)
    {
        float x=(Position+World.instance.level.GlobalPosition).x;
        x-=GetViewportRect().Size.x/2;
        if(x<=0f)
        {
            SetProcess(false);
            Level level=World.instance.level;

            if(EnableSpeed)
            {
                float oSpeed=level.Speed;
                level.Speed=Speed;
                GetTree().CreateTimer(10,false).Connect("timeout",this,nameof(onTimeout),new Godot.Collections.Array(oSpeed));
            }
            else
            {
                CallDeferred("queue_free");
            }
        }

    }

    private void onTimeout(float speed)
    {
        World.instance.level.Speed=speed;
        CallDeferred("queue_free");
    }

    private void onScreenEntered()
    {
        SetProcess(true);
    }
    private void onScreenExited()
    {
        SetProcess(false);
        CallDeferred("queue_free");
    }

}
