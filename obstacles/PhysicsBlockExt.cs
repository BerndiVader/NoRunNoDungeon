using Godot;
using System;

public class PhysicsBlockExt : PhysicsObject
{
    [Export] protected float STOP_FORCE=1300f;

    private readonly Vector2 UP_DIRECTION=new Vector2(8f,8f);

    private CollisionShape2D collision;

    public override void _Ready()
    {
        base._Ready();
        collision=GetNode<CollisionShape2D>(nameof(CollisionShape2D));

        Vector2 size=((RectangleShape2D)collision.Shape).Extents*2f;
        Vector2 start=collision.Position-(size*0.5f);

        notifier2D.Rect=new Rect2(start,size);
        notifier2D.Scale=Scale;
        notifier2D.Rotation=Rotation;
    }

    public override void _PhysicsProcess(float delta)
    {
        Navigation(delta);
    }

    private void Navigation(float delta)
    {
        velocity+=GRAVITY*delta;
        velocity=MoveAndSlideWithSnap(velocity,Vector2.Zero,UP_DIRECTION,true,4,0f,true);
        
        int slides=GetSlideCount();
        if(slides>0)
        {
            for(int i=0;i<slides;i++)
            {
                var collision=GetSlideCollision(i);
                if(collision.Collider is Platform platform&&collision.Normal==Vector2.Up)
                {
                    velocity.x=platform.Velocity.x;
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
