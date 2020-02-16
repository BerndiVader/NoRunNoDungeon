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

    VisibilityNotifier2D notifier;
    Tween tween;

    Vector2 follow=new Vector2(0f,0f);
    bool reverse=false;


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

        MoveDirection=MoveDirection*MoveLength;

        if(!StaticElement) initTween();

    }

    public override void _PhysicsProcess(float delta)
    {
    }


    void initTween()
    {
        tweenIn();
        tween.Start();
    }

    public void tweenOut(Vector2 delta)
    {
        Position=delta;
    }

    public void finishedTween()
    {
        reverse=!reverse;
        if(reverse) {
            tweenOut();
        } else {
            tweenIn();
        }
        tween.Start();
    }

    void tweenIn()
    {
        tween.InterpolateMethod(this,"tweenOut",Position,Position-MoveDirection,Speed,Tween.TransitionType.Bounce,Tween.EaseType.Out,InDelay);
    }

    void tweenOut()
    {
        tween.InterpolateMethod(this,"tweenOut",Position,Position+MoveDirection,Speed,Tween.TransitionType.Linear,Tween.EaseType.Out,OutDelay);
    }


    public void enteredScreen()
    {
        SetPhysicsProcess(true);
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
            WorldUtils.world.CallDeferred("restartGame");
        }

    }

}
