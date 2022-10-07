using Godot;
using System;

public class Spikes : Area2D
{

    [Export] bool StaticElement=true;
    [Export] Vector2 MoveDirection=new Vector2(0,1);
    [Export] float MoveLength=16f;
    [Export] float Speed=0.5f;
    [Export] int InDelay=1;
    [Export] int OutDelay=1;
    [Export] float StartDelay=0.1f;
    [Export] float ActOnDistance=-1f;
    [Export] float damage=1f;

    VisibilityNotifier2D notifier2D;

    Tween tween;
    Timer timer;

    Vector2 Movement;

    Vector2 follow=new Vector2(0f,0f);
    bool reverse=false;
    bool running;

    public override void _Ready()
    {
        notifier2D=new VisibilityNotifier2D();
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

    void initTween()
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

    void onTimedOut() 
    {
        tweenIn();
        tween.Start();
        timer.QueueFree();
    }

    void tweening(Vector2 delta)
    {
        Position=delta;
    }

    void onFinishedTween()
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

    void tweenIn()
    {
        tween.InterpolateMethod(this,"tweening",Position,Position-Movement,Speed,Tween.TransitionType.Bounce,Tween.EaseType.Out,InDelay);
    }

    void tweenOut()
    {
        tween.InterpolateMethod(this,"tweening",Position,Position+Movement,Speed,Tween.TransitionType.Linear,Tween.EaseType.Out,OutDelay);
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

    public void onEnteredBody(Node node)
    {
        if(node.IsInGroup(GROUPS.PLAYERS.ToString())) 
        {
            node.EmitSignal(SIGNALS.Damage.ToString(),damage,this);
        }

    }

    void onExitedScreen()
    {
        QueueFree();
    }    

}