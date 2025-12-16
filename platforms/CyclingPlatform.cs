using Godot;
using System;
using System.Threading;

public class CyclingPlatform : Platform
{

    [Export] private bool RandomCycling=false;
    [Export] private float Delay=10f;
    [Export] private float Speed=5f;
    [Export] private float RotationSteps=1f;
    [Export] private Tween.TransitionType TransType=Tween.TransitionType.Quint;
    [Export] private Tween.EaseType EaseType=Tween.EaseType.InOut;

    private Tween tween;
    private Area2D area;

    private float delayCount=0f;
    private bool overlapping=false;

    public override void _Ready()
    {
        base._Ready();
        tween=GetNode<Tween>("Tween");

        switch(platformState)
        {
            case PLATFORMSTATE.ONPLAYER:
                area=GetNode<Area2D>("Area2D");
                area.Connect("body_entered", this, nameof(OnBodyEntered));
                area.Connect("body_exited", this, nameof(OnBodyExited));
                break;
            case PLATFORMSTATE.SWITCH:
                SetPhysicsProcess(false);
                SetProcess(false);
                AddToGroup(GROUPS.SWITCHABLES.ToString());
                break;
        }

    }

    public override void _PhysicsProcess(float delta)
    {
        if(!tween.IsActive())
        {
            if(RandomCycling)
            {
                if(MathUtils.RandBool())
                {
                    if(platformState==PLATFORMSTATE.ONPLAYER)
                    {
                        if(overlapping)
                        {
                            StartTween();
                        }
                    }
                    else
                    {
                        StartTween();
                    }
                }
            }
            else
            {
                if(delayCount>Delay)
                {
                    if(platformState==PLATFORMSTATE.ONPLAYER)
                    {
                        if(overlapping)
                        {
                            StartTween();
                        }
                    }
                    else
                    {
                        StartTween();
                    }
                    delayCount=0f;
                }
                delayCount++;
            }
        }
    }

    private void StartTween()
    {
        tween.InterpolateProperty(this,
            "rotation",
            Rotation,
            Rotation+Mathf.Pi*RotationSteps,
            Speed,
            TransType,
            EaseType
        );
        tween.Start();
    }

    private void OnBodyEntered(Node body)
    {
        if(body is Player)
        {
            overlapping=true;
        }
    }
    
    private void OnBodyExited(Node body)
    {
        if(body is Player)
        {
            overlapping=false;
        }
    }

    public override void SwitchCall(string id)
    {
        if(switchID==id&&!tween.IsActive())
        {
            StartTween();
        }
    }


}
