using Godot;
using System;

public class Spikes : Area2D
{

    [Export] private bool StaticElement=true;
    [Export] private Vector2 MoveDirection=new Vector2(0,1);
    [Export] private float MoveLength=16f;
    [Export] private float Speed=0.5f;
    [Export] private int InDelay=1;
    [Export] private int OutDelay=1;
    [Export] private float StartDelay=0.1f;
    [Export] private float ActOnDistance=-1f;
    [Export] private float damage=1f;

    private Tween tween;
    private Timer timer;
    private Vector2 Movement;
    private bool reverse=false;

    public override void _Ready()
    {
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",this,nameof(onExitedScreen));
        AddChild(notifier2D);

        tween=new Tween();
        tween.Connect("tween_all_completed",this,nameof(onFinishedTween));
        AddChild(tween);

        Connect("body_entered",this,nameof(onEnteredBody));

        Movement=MoveDirection*MoveLength;

    }

    public override void _PhysicsProcess(float delta)
    {
        Vector2 playerPos=World.instance.player.GlobalPosition;
        Vector2 gamePos=GlobalPosition;
        float distance=playerPos.DistanceTo(gamePos);

        if(distance<ActOnDistance)
        {
            Vector2 direction=(gamePos-playerPos).Normalized();
            float angle=Mathf.Rad2Deg(direction.Angle());

            float dot=direction.Dot(MoveDirection);
            float angleToNode=Mathf.Rad2Deg(Mathf.Acos(dot));
            if(angleToNode<90)
            {
                SetPhysicsProcess(false);
                InDelay=0;
                timer=new Timer();
                timer.OneShot=true;
                AddChild(timer);
                timer.Connect("timeout",this,nameof(onTimedOut));
                timer.Start(StartDelay);
            }
        }
    }

    private void initTween()
    {
        if(ActOnDistance<0f)
        {
            timer=new Timer();
            timer.OneShot=true;
            AddChild(timer);
            timer.Connect("timeout",this,nameof(onTimedOut));
            timer.Start(StartDelay);
        }
        else
        {
            SetPhysicsProcess(true);
        }
    }

    private void onTimedOut() 
    {
        tweenIn();
        tween.Start();
        timer.QueueFree();
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
            tweenIn();
        }

        tween.Start();
    }

    private void tweenIn()
    {
        tween.InterpolateMethod(this,nameof(tweening),Position,Position-Movement,Speed,Tween.TransitionType.Bounce,Tween.EaseType.Out,InDelay);
    }

    private void tweenOut()
    {
        tween.InterpolateMethod(this,nameof(tweening),Position,Position+Movement,Speed,Tween.TransitionType.Linear,Tween.EaseType.Out,OutDelay);
    }

    public override void _EnterTree()
    {
        if(!StaticElement) 
        {
            initTween();
        }
        else
        {
            SetProcess(false);
            SetPhysicsProcess(false);
        }
    }

    private void onEnteredBody(Node node)
    {
        if(node.IsInGroup(GROUPS.PLAYERS.ToString())) 
        {
            node.EmitSignal(SIGNALS.Damage.ToString(),damage,this);
        }

    }

    private void onExitedScreen()
    {
        QueueFree();
    }    

}