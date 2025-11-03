using Godot;
using System;

public class MovingPlatform : Platform
{

    [Export] private Vector2 Direction=Vector2.Up;
    [Export] private float Speed=20f;
    [Export] private float MaxSpeed=20f;
    [Export] private int Length=2;
    [Export] private bool Linear=true;

    private float maxDistance;
    private float speed;

    public override void _Ready()
    {
        base._Ready();

        maxDistance = Length * 16;
        speed = Speed;
    }

    public override void _PhysicsProcess(float delta)
    {
        lastPosition = Position;
        float distance = Position.DistanceTo(startPosition);

        if(((int)distance)>maxDistance)
        {
            Direction*=-1;
        }

        if(!Linear)
        {
            speed=Mathf.Clamp(Mathf.Ease(1-(distance/maxDistance),0.5f)*1000,10,MaxSpeed);
        }

        speed=Mathf.Min(speed,MaxSpeed);
        CurrentSpeed=Direction*speed;
        Translate(CurrentSpeed*delta);
    }

}
