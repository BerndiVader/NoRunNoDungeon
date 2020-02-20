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

    VisibilityNotifier2D notifier;
    Tween tween;
    Timer timer;

    Vector2 Movement;

    Vector2 follow=new Vector2(0f,0f);
    bool reverse=false;
    bool running;

    public override void _Ready()
    {

        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);

        notifier=(VisibilityNotifier2D)GetNode("Notifier");
        tween=(Tween)GetNode("Tween");

        notifier.Connect("screen_entered",this,nameof(enteredScreen));
        notifier.Connect("screen_exited",this,nameof(exitedScreen));

        Connect("body_entered",this,nameof(enteredBody));

        tween.Connect("tween_all_completed",this,nameof(finishedTween));

        Movement=MoveDirection*MoveLength;


    }

    public override void _PhysicsProcess(float delta)
    {
        Player player=WorldUtils.world.player;
        Vector2 gamePos=Position+WorldUtils.world.level.Position;
        float distance=player.Position.DistanceTo(gamePos);

        if(distance<ActOnDistance)
        {

            Vector2 direction=(gamePos-player.Position).Normalized();
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
                timer.Connect("timeout",this,nameof(timedOut));
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
            timer.Connect("timeout",this,nameof(timedOut));
            timer.Start(StartDelay);
        }
        else
        {
            SetPhysicsProcess(true);
        }
    }

    void timedOut() 
    {
        tweenIn();
        tween.Start();
        timer.QueueFree();
    }

    void tweening(Vector2 delta)
    {
        Position=delta;
    }

    void finishedTween()
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

    public void enteredScreen()
    {
        if(!StaticElement) initTween();
    }

    public void exitedScreen()
    {
        SetPhysicsProcess(false);
        CallDeferred("queue_free");
    }

    public void enteredBody(Node node)
    {
        if(node.IsInGroup("Players")) 
        {
            WorldUtils.world.CallDeferred("restartGame",true);
        }

    }

}
