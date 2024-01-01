using Godot;
using System;

public class TestBullet : Area2D
{
    public Vector2 direction=Vector2.Zero;
    private float speed=100f,liveSpan=50f;

    public override void _Ready()
    {
        if(direction==Vector2.Right)
        {
            GetNode<Sprite>("Sprite").FlipH=true;
        }
        Connect("body_entered",this,nameof(onBodyEntered));
    }

    public override void _PhysicsProcess(float delta)
    {
        liveSpan-=1f;
        Position+=direction*(speed*delta);
        if(liveSpan<0f)
        {
            GD.Print("eol");
            QueueFree();
        }
    }

    public void onBodyEntered(Node node)
    {
        QueueFree();
    }   

}
