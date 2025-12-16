using Godot;
using System;

public class Spikes : Area2D,ISwitchable
{
    private enum SPIKESTATE
    {
        DOWN,
        UP
    }

    [Export] private bool StaticElement=true;
    [Export] private bool Switchable=false;
    [Export] private string SwitchId="";
    [Export] private SPIKESTATE SpikeState=SPIKESTATE.DOWN;
    [Export] private bool DamageMonster=false;
    [Export] private float MoveLength=12f;
    [Export] private float Speed=0.5f;
    [Export] private int InDelay=0;
    [Export] private int OutDelay=1;
    [Export] private float ActOnDistance=-1f;
    [Export] private float Damage=1f;

    private Tween tween;
    private Vector2 movement,startPosition;
    private Vector2 MoveDirection;
    private bool moving=false;

    public override void _Ready()
    {
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);

        switch(RotationDegrees)
        {
            case 0:
                MoveDirection=Vector2.Down;
                break;
            case 90:
                MoveDirection=Vector2.Left;
                break;
            case 180:
                MoveDirection=Vector2.Up;
                break;
            case 270:
                MoveDirection=Vector2.Right;
                break;
        }

        if(DamageMonster)
        {
            CollisionMask|=1<<5;
        }

        if(Switchable)
        {
            startPosition=Position;
            movement=MoveDirection*MoveLength;
            tween=new Tween();
            tween.Connect("tween_all_completed",this,nameof(OnTweenCompleted));
            AddChild(tween);
            AddToGroup(GROUPS.SWITCHABLES.ToString());
        }
        else if(!StaticElement)
        {
            startPosition=Position;
            movement=MoveDirection*MoveLength;
            tween=new Tween();
            tween.Connect("tween_all_completed",this,nameof(OnFinishedTween));
            AddChild(tween);
            Init();
        }
        else
        {
            SetProcess(false);
            SetPhysicsProcess(false);
        }

        Connect("body_entered",this,nameof(OnEnteredBody));
    }

    public override void _PhysicsProcess(float delta)
    {
        float distance=GlobalPosition.DistanceTo(Player.instance.GlobalPosition);

        if(distance<ActOnDistance)
        {
            SetPhysicsProcess(false);
            TweenIn();
        }
    }

    private void Init()
    {
        if(ActOnDistance==-1f)
        {
            TweenIn();
        }
        else
        {
            SetPhysicsProcess(true);
        }

    }

    private void Start()
    {
        switch(SpikeState)
        {
            case SPIKESTATE.UP:
                TweenOut();
                break;

            case SPIKESTATE.DOWN:
                Position=startPosition;
                Init();
                break;
        }
    }

    private void Tweening(Vector2 delta)
    {
        Position=delta;
    }

    private void OnFinishedTween()
    {
        moving=false;
        Start();
    }

    private void TweenIn()
    {
        moving=true;
        SpikeState=SPIKESTATE.UP;
        tween.InterpolateMethod(this,nameof(Tweening),Position,Position-movement,Speed,Tween.TransitionType.Bounce,Tween.EaseType.Out,InDelay);
        tween.Start();
    }

    private void TweenOut()
    {
        moving=true;
        SpikeState=SPIKESTATE.DOWN;
        tween.InterpolateMethod(this,nameof(Tweening),Position,Position+movement,Speed,Tween.TransitionType.Back,Tween.EaseType.In,OutDelay);
        tween.Start();
    }

    private void OnEnteredBody(Node node)
    {
        if(node.IsInGroup(GROUPS.PLAYERS.ToString())) 
        {
            node.EmitSignal(STATE.damage.ToString(),Damage,this);
        }
        else if(node.IsInGroup(GROUPS.ENEMIES.ToString()))
        {
            node.EmitSignal(STATE.damage.ToString(),this,Damage);
        }

    }

    private void OnTweenCompleted()
    {
        moving=false;
    }

    public void SwitchCall(string id)
    {
        if(id==SwitchId&&!moving)
        {
            switch(SpikeState)
            {
                case SPIKESTATE.DOWN:
                    TweenIn();
                    break;
                case SPIKESTATE.UP:
                    TweenOut();
                    break;
            }
        }
    }
}