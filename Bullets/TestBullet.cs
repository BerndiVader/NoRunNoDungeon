using Godot;
using System;

public class TestBullet : Area2D
{

    public Vector2 direction=new Vector2(0f,0f);
    float speed=300f;

    VisibilityNotifier2D notifier2D;

    public override void _Ready()
    {
        notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",this,"exitedScreen");
        AddChild(notifier2D);

        Connect("body_entered",this,"bodyEntered");
    }

    public override void _PhysicsProcess(float delta)
    {
        Position=Position+direction*(speed*delta);
    }

    void bodyEntered(Node node)
    {
        exitedScreen();
    }

    void exitedScreen()
    {
        CallDeferred("queue_free");
    }

}