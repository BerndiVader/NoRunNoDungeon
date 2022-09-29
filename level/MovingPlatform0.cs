using Godot;
using System;

public class MovingPlatform0 : Platform
{

    [Export] public Vector2 Direction=new Vector2(0f,-1f);
    [Export] public float Speed=20f;
    [Export] public float MaxSpeed=20f;
    [Export] public int Length=10;
    [Export] public float LerpFactor=4f;
    [Export] public bool Linear=true;
    [Export] public float Shrink=0f;

    int maxLength;
    float speed;

    public override void _Ready()
    {
        base._Ready();
        maxLength=Length*16;
        speed=Speed;
    }

    public override void _PhysicsProcess(float delta)
    {
        lastPosition=Position;
        float currentLength=Mathf.Abs(Position.DistanceTo(startPosition));
        if(currentLength>maxLength)
        {
            Direction*=-1;
        }

        if(!Linear)
        {
            float speed=Mathf.Lerp(currentLength,maxLength,LerpFactor);
        } 
        CurrentSpeed=Direction*speed;
        Position+=CurrentSpeed*delta;
    }

}
