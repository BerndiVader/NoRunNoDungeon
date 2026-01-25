using Godot;
using System;

public class PhysicsBlock : PhysicsObject
{
    [Export] protected float STOP_FORCE=1300f;

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _PhysicsProcess(float delta)
    {
        Navigation(delta);
    }

    private void Navigation(float delta)
    {
        velocity+=GRAVITY*delta;
        velocity=MoveAndSlideWithSnap(velocity,Vector2.Zero,new Vector2(4f,4f),true,4,0f,true);
        
        int slides=GetSlideCount();
        if(slides>0)
        {
            for(int i=0;i<slides;i++)
            {
                var collision=GetSlideCollision(i);
                if(collision.Collider is Platform platform&&collision.Normal==Vector2.Up)
                {
                    velocity.x=platform.CurrentSpeed.x;
                }
                else
                {
                    velocity=StopX(velocity,delta);
                }
            }    
        }
        else
        {
            velocity=StopX(velocity,delta);
        }

    }

    private Vector2 StopX(Vector2 velocity,float delta)
    {
        float xLength=Mathf.Abs(velocity.x)-(STOP_FORCE*delta);
        if(xLength<0f) {
            xLength=0f;
        }
        velocity.x=xLength*Mathf.Sign(velocity.x);
        return velocity;
    }
}
