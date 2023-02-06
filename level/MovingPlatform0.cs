using Godot;
using System;

public class MovingPlatform0 : Platform
{

    [Export] private Vector2 Direction=new Vector2(0f,-1f);
    [Export] private float Speed=20f;
    [Export] private float MaxSpeed=20f;
    [Export] private int Length=10;
    [Export] private float LerpFactor=4f;
    [Export] private bool Linear=true;
    [Export] private float Shrink=0f;

    private int maxDistance;
    private float speed;
    private Vector2 maxPosition;

    public override void _Ready()
    {
        base._Ready();
        maxDistance=Length*16;
        speed=Speed;
    }

    public override void _PhysicsProcess(float delta)
    {
        lastPosition=Position;
        float distance=Position.DistanceTo(startPosition);

        if(distance>=maxDistance)
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
