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
    private bool moving=false;

    public override void _Ready()
    {
        base._Ready();

        maxDistance=Length*16;
        speed=Speed;

        if(platformState==PLATFORMSTATE.SWITCH)
        {
            AddToGroup(GROUPS.SWITCHABLES.ToString());
        }

    }

    public override void _PhysicsProcess(float delta)
    {
        lastPosition=Position;
        float distance=Position.DistanceTo(startPosition);

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
            Direction*=-1;
            if(platformState==PLATFORMSTATE.ONETIME||platformState==PLATFORMSTATE.SWITCH)
            {
                moving=false;
                SetPhysicsProcess(moving);
            }
        }

        if(!Linear)
        {
            speed=Mathf.Clamp(Mathf.Ease(1-(distance/maxDistance),0.5f)*1000,10,MaxSpeed);
        }

        speed=Mathf.Min(speed,MaxSpeed);
        CurrentSpeed=Direction*speed;
        Translate(CurrentSpeed*delta);
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
