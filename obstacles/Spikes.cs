using Godot;
using System;

public class Spikes : Area2D
{

    [Export] private bool StaticElement=true;
    [Export] private Vector2 MoveDirection=new Vector2(0,1);
    [Export] private float MoveLength=12f;
    [Export] private float Speed=0.5f;
    [Export] private int InDelay=0;
    [Export] private int OutDelay=1;
    [Export] private float ActOnDistance=-1f;
    [Export] private float damage=1f;

    private Tween tween;
    private Vector2 movement,startPosition;
    private bool reverse=false;

    public override void _Ready()
    {
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.onObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);

        if(!StaticElement)
        {
            tween=new Tween();
            tween.Connect("tween_all_completed",this,nameof(onFinishedTween));
            AddChild(tween);
            movement=MoveDirection*MoveLength;
            startPosition=Position;
            init();
        }
        else
        {
            SetProcess(false);
            SetPhysicsProcess(false);
        }

        Connect("body_entered",this,nameof(onEnteredBody));
    }

    public override void _PhysicsProcess(float delta)
    {
        float distance=GlobalPosition.DistanceTo(Player.instance.GlobalPosition);

        if(distance<ActOnDistance)
        {
            SetPhysicsProcess(false);
            tweenIn();
        }
    }

    private void init()
    {
        if(ActOnDistance<0f)
        {
            tweenIn();
        }
        else
        {
            SetPhysicsProcess(true);
        }

    }

    private void tweening(Vector2 delta)
    {
        Position=delta;
    }

    private void onFinishedTween()
    {
        reverse=!reverse;
        if(reverse) 
        {
            tweenOut();
        } 
        else 
        {
            Position=startPosition;
            if(ActOnDistance!=-1)
            {
                SetPhysicsProcess(true);
            }
            else
            {
                tweenIn();
            }
        }
    }

    private void tweenIn()
    {
        tween.InterpolateMethod(this,nameof(tweening),Position,Position-movement,Speed,Tween.TransitionType.Bounce,Tween.EaseType.Out,InDelay);
        tween.Start();
    }

    private void tweenOut()
    {
        tween.InterpolateMethod(this,nameof(tweening),Position,Position+movement,Speed,Tween.TransitionType.Back,Tween.EaseType.In,OutDelay);
        tween.Start();
    }

    private void onEnteredBody(Node node)
    {
        if(node.IsInGroup(GROUPS.PLAYERS.ToString())) 
        {
            node.EmitSignal(STATE.damage.ToString(),damage,this);
        }

    }  

}