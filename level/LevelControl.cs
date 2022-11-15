using Godot;
using System;

public class LevelControl : Node2D
{
    VisibilityNotifier2D notifier;
    [Export] private bool EnableSpeed=false;
    [Export] private float Speed=0f;
    [Export] private bool EnableZoom=false;
    [Export] private float Zoom=1f;

    private Settings pSettings;

    public override void _Ready()
    {
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
        float x=(Position+World.level.GlobalPosition).x;
        x-=GetViewportRect().Size.x/2;
        if(x<=0f)
        {
            SetProcess(false);
            pSettings=new Settings(World.level);

            if(EnableSpeed)
            {
                float oSpeed=World.level.Speed;
                Vector2 oZoom=PlayerCamera.instance.Zoom;
                World.level.Speed=Speed;
                PlayerCamera.instance.Zoom=new Vector2(Zoom,Zoom);
                PlayerCamera.instance.GlobalPosition=World.instance.player.GlobalPosition;
                GetTree().CreateTimer(10,false).Connect("timeout",this,nameof(onTimeout));
            }
            else
            {
                CallDeferred("queue_free");
            }
        }

    }

    private void onTimeout()
    {
        pSettings.restore();
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
