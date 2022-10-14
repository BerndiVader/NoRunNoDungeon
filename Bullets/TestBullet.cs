using Godot;
using System;

public class TestBullet : Area2D
{

    private Vector2 direction=new Vector2(0f,0f);
    private float speed=100f;

    public override void _Ready()
    {
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",this,nameof(onExitedScreen));
        AddChild(notifier2D);

        if(direction.x>0)
        {
            GetNode<Sprite>("Sprite").FlipH=true;
        }

        Connect("body_entered",this,nameof(onBodyEntered));
    }

    public override void _PhysicsProcess(float delta)
    {
        Position=Position+direction*(speed*delta);
    }

    void onBodyEntered(Node node)
    {
        QueueFree();
    }

    void onExitedScreen()
    {
        QueueFree();;
    }

}
