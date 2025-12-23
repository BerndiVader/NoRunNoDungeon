using Godot;
using System;

public class Stick : TouchScreenButton
{
    private Touch touch;
    private Vector2 rad=new Vector2(16f,16f);
    private float boundary=32f;
    private int onGoing=-1;
    private float returnAccel=60f;
    private float threshold=10f;
    private Vector2 hidePosition=new Vector2(-100f,-100f);
    private bool visible;
    private Vector2 approxPosition=new Vector2(-16f,-16f);
    public bool useAccelerometer;

    public override void _Ready()
    {
        SetProcess(false);

        touch=GetParent<Touch>();
        touch.Position=hidePosition;
        visible=false;
        touch.oPosition=Position;

        useAccelerometer=false;

        if(useAccelerometer)
        {
            SetPhysicsProcess(false);
            SetProcessInput(false);
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if(onGoing==-1)
        {
            Vector2 diffPos=Vector2.Zero-rad-Position;
            Position+=diffPos*returnAccel*delta;
            if(visible&&Position.IsEqualApprox(approxPosition)) 
            {
                touch.Position=hidePosition;
                touch.oPosition=hidePosition;
                visible=false;
                onGoing--;
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        using(@event)
        {
            if(@event is InputEventScreenDrag || (@event is InputEventScreenTouch && @event.IsPressed()))
            {
                InputEventScreenDrag d=@event as InputEventScreenDrag;
                InputEventScreenTouch t=@event as InputEventScreenTouch;

                int index;
                Vector2 position;

                if(d!=null)
                {
                    position=d.Position;
                    index=d.Index;
                }
                else
                {
                    position=t.Position;
                    index=t.Index;
                    
                    if(@t.IsPressed()&&position.x<256f)
                    {
                        touch.Position=position;
                        touch.oPosition=position;
                        visible=true;
                    }
                }

                float distCenter=(position-touch.Position).Length();

                if(distCenter<=boundary||index==onGoing)
                {
                    Position=position-touch.Position-rad;
                    if(GetButtonPosition().Length()>boundary)
                    {
                        Position=GetButtonPosition().Normalized()*boundary-rad;
                    }
                    onGoing=index;
                }

            }

            if(@event is InputEventScreenTouch && !@event.IsPressed())
            {
                InputEventScreenTouch eventScreenTouch=@event as InputEventScreenTouch;
                if(eventScreenTouch.Index==onGoing)
                {
                    onGoing=-1;
                }
            }
        }
    }

    private Vector2 GetButtonPosition()
    {
        return Position+rad;
    }

    public Vector2 GetValue()
    {
        if(!useAccelerometer)
        {
            if(GetButtonPosition().Length()>threshold)
            {
                return GetButtonPosition().Normalized();
            }
            else
            {
                return Vector2.Zero;
            }
        }
        else
        {
            Vector3 acc=Input.GetAccelerometer();
            Vector2 accel=new Vector2(acc.x,acc.y);
            return accel;
        }
    }



}
