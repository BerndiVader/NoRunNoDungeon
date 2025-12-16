using Godot;
using System;

public class HiddenDoor : StaticBody2D,ISwitchable
{
    private enum TYPE {
        HITABLE,
        AUTOMATIC,
        SWITCH
    }

    private enum DOOR_STATE {
        CLOSED,
        OPENED
    }

    [Export] private TYPE type=TYPE.HITABLE;
    [Export] private float closedTime=2f;
    [Export] private float openedTime=2f;
    [Export] private bool oneShoot=false;
    [Export] private string switchID="";
    [Export] private DOOR_STATE doorState=DOOR_STATE.CLOSED;

    private bool moving=false;
    private bool used=false;
    private Tween tween;
    private Timer timer;

    public override void _Ready()
    {

        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);

        tween=GetNode<Tween>(nameof(Tween));
        tween.Connect("tween_completed",this,nameof(OnTweenCompleted));

        AddUserSignal(STATE.damage.ToString());
        Connect(STATE.damage.ToString(),this,nameof(OnDamage));

        switch(type)
        {
            case TYPE.AUTOMATIC:
                timer=new Timer();
                timer.OneShot=true;
                timer.Connect("timeout",this,nameof(OnAutomaticTimeout));
                AddChild(timer);
                StartAutomaticMode();
                break;
            case TYPE.SWITCH:
                AddToGroup(GROUPS.SWITCHABLES.ToString());
                break;
        }

        SetProcess(false);
        SetPhysicsProcess(false);

    }

    private void StartAutomaticMode()
    {
        switch(doorState)
        {
            case DOOR_STATE.CLOSED:
                OpenDoor();
                timer.WaitTime=openedTime;
                timer.Start();
                break;
            case DOOR_STATE.OPENED:
                CloseDoor();
                timer.WaitTime=closedTime;
                timer.Start();
                break;
        }
    }
    private void OnAutomaticTimeout()
    {
        StartAutomaticMode();
    }

    private void OnTweenCompleted(Node node,NodePath path)
    {
        moving=false;
    }

    private void OpenDoor()
    {
        if(used&&oneShoot)
        {
            return;
        }
        if(GetNode<CollisionShape2D>(nameof(CollisionShape2D)).Shape is RectangleShape2D shape) {
            used=moving=true;
            Vector2 startPos=Position;
            doorState=DOOR_STATE.OPENED;
            Vector2 endPos=startPos+new Vector2(0,shape.Extents.y*-1.5f);
            tween.InterpolateProperty(this,"position",startPos,endPos,0.5f,Tween.TransitionType.Sine,Tween.EaseType.Out);
            tween.Start();
        }
        else {
            moving=false;
        }
    }
    private void CloseDoor()
    {
        if(used&&oneShoot)
        {
            return;
        }
        if(GetNode<CollisionShape2D>(nameof(CollisionShape2D)).Shape is RectangleShape2D shape)
        {
            Vector2 endPos=Position;
            used=moving=true;
            doorState=DOOR_STATE.CLOSED;
            Vector2 startPos=endPos+new Vector2(0,shape.Extents.y*1.5f);
            tween.InterpolateProperty(this,"position",endPos,startPos,0.5f,Tween.TransitionType.Sine,Tween.EaseType.Out);
            tween.Start();
        }
        else {
            moving=false;
        }
    }

    private void OnDamage(Player player=null, int amount=0)
    {
        if(type==TYPE.HITABLE&&!moving)
        {
            switch(doorState)
            {
                case DOOR_STATE.CLOSED:
                    OpenDoor();
                    break;
                case DOOR_STATE.OPENED:
                    CloseDoor();
                    break;
            }
        }
    }

    public void SwitchCall(string id)
    {
        if(id==switchID&&!moving)
        {
            switch(doorState)
            {
                case DOOR_STATE.CLOSED:
                    OpenDoor();
                    break;
                case DOOR_STATE.OPENED:
                    CloseDoor();
                    break;
            }
        }
    }

}
