using Godot;
using System;

public class PhysicsBlock : PhysicsObject
{
    [Export] protected float STOP_FORCE=1300f;

    protected Platform riding;
    protected int ridingFrames;
    protected readonly Vector2 SNAP=new Vector2(0f,16f);
    protected readonly Vector2 UP_DIRECTION=new Vector2(8f,8f);

    protected CollisionShape2D collision;

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

    protected virtual void Navigation(float delta)
    {
        velocity+=GRAVITY*delta;
        velocity=MoveAndSlideWithSnap(velocity,SNAP,UP_DIRECTION,true,4,0f,true);
        
        int slides=GetSlideCount();
        bool contact=false;

        for(int i=0;i<slides;i++)
        {
            var collision=GetSlideCollision(i);
            if(collision.Collider is Platform platform&&collision.Normal.y<-0.9f)
            {
                riding=platform;
                ridingFrames=0;
                contact=true;
            }
        }
        if(!contact&&riding!=null&&++ridingFrames>3)
        {
            riding=null;
        }
        if(riding!=null)
        {
            velocity.x=riding.Velocity.x;
        }
        else
        {
            StopX(delta);
        }

    }

    protected virtual void StopX(float delta)
    {
        velocity.x=Mathf.Max(Mathf.Abs(velocity.x)-STOP_FORCE*delta,0f)*Mathf.Sign(velocity.x);
    }

}
