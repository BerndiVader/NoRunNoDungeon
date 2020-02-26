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
    Vector2 dir,startPos;

    CollisionShape2D collisionController;

    public override void _Ready()
    {
        base._Ready();
        dir=Direction.Normalized();
        startPos=new Vector2(Position);
        maxLength=Length*16;
    }

    public override void _PhysicsProcess(float delta)
    {
        float currentLength=Position.Length()-startPos.Length();

        if(!Linear)
        {
            if(currentLength>=maxLength)
            {
                dir*=-1;
            }
            float speed=Mathf.Lerp(currentLength,maxLength,LerpFactor);
            
            Position+=dir*(speed*delta);
        } 
        else 
        {
            if(currentLength>=maxLength)
            {
                dir*=-1;
            }
            Position+=dir*(Speed*delta);
        }

        base._PhysicsProcess(delta);

    }

}
