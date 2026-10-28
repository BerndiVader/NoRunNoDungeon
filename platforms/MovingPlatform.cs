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
    private bool moving=true;

    public override void _Ready()
    {
        base._Ready();

        maxDistance=Mathf.Pow(Length*16f,2f);
        speed=Speed;

        if(platformState==PLATFORMSTATE.SWITCH)
        {
            AddToGroup(GROUPS.SWITCHABLES.ToString());
        }

    }

    public override void _PhysicsProcess(float delta)
    {
        lastPosition=Position;
        float distance=Position.DistanceSquaredTo(startPosition);

        if(platformState==PLATFORMSTATE.ONPLAYER&&!playerOn)
        {
            if(((int)distance)>maxDistance&&moving)
            {
                moving=false;
                SetPhysicsProcess(moving);
            }
        }

        if(((int)distance)>maxDistance)
        {
            Position=startPosition+Direction*(Length*16f);
            Direction*=-1;
            if(platformState==PLATFORMSTATE.ONETIME||platformState==PLATFORMSTATE.SWITCH)
            {
                moving=false;
                SetPhysicsProcess(moving);
            }
        }

        if(!Linear)
        {
            speed=Mathf.Clamp(Mathf.Ease(1-(distance/maxDistance),0.5f)*1000f,10f,MaxSpeed);
        }

        speed=Mathf.Min(speed,MaxSpeed);
        Velocity=Direction*speed;
        if(!moving)
        {
            Velocity=Vector2.Zero;
        }
        Translate(Velocity*delta);
    }

    public override void SwitchCall(string id)
    {
        if(switchID==id&&!moving)
        {
            moving=true;
            SetPhysicsProcess(moving);
        }
    }

    protected override void BumpAreaEntered(Node node)
    {
        base.BumpAreaEntered(node);
        if(platformState==PLATFORMSTATE.ONPLAYER&&playerOn&&!moving)
        {
            moving=true;
            SetPhysicsProcess(moving);
        }
    }


}
