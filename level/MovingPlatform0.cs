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
        maxLength=Length;
        speed=Speed;
    }

    public override void _PhysicsProcess(float delta)
    {
        float currentLength=Mathf.Abs(Position.Length()-startPosition.Length());
        if(currentLength>maxLength)
        {
            Direction*=-1;
        }

        if(!Linear)
        {
            float speed=Mathf.Lerp(currentLength,maxLength,LerpFactor);
        } 
        Position+=Direction*(speed*delta);
    }

}
