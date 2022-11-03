using Godot;
using System;

public class Stick : TouchScreenButton
{
    private Touch touch;
    private Vector2 rad=new Vector2(16f,16f);
    private float boundary=32f;
    private int onGoing=-1;
    private float returnAccel=20f;
    private float threshold=10f;
    public bool useAccelerometer;

    private Label label;

    public override void _Ready()
    {
        touch=GetParent<Touch>();
        touch.Position=new Vector2(-100,-100);

        useAccelerometer=false;

        if(useAccelerometer)
        {
            SetProcess(false);
            SetProcessInput(false);
        }
    }

    public override void _Process(float delta)
    {
        if(onGoing==-1)
        {
            Vector2 diffPos=Vector2.Zero-rad-Position;
            Position+=diffPos*returnAccel*delta;
            if((Position.IsEqualApprox(new Vector2(-16f,-16f)))) 
            {
                touch.Position=new Vector2(-100,-100);
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventScreenDrag || (@event is InputEventScreenTouch && @event.IsPressed()))
        {
            InputEventScreenDrag d=@event as InputEventScreenDrag;
            InputEventScreenTouch t=@event as InputEventScreenTouch;

            int index=-1;
            Vector2 position=Vector2.Zero;
            Vector2 speed=Vector2.Zero;

            if(d!=null)
            {
                position=d.Position;
                index=d.Index;
                speed=d.Speed;
            }
            else
            {
                position=t.Position;
                index=t.Index;
            }
            
            if(@event.IsPressed()&&position.x<512*0.5) 
            {
                touch.GlobalPosition=position;
            }
            float distCenter=(position-touch.GlobalPosition).Length();

            if(distCenter<=boundary*GlobalScale.x||index==onGoing)
            {
                GlobalPosition=position-rad*GlobalScale;
                if(getButtonPosition().Length()>boundary)
                {
                    Position=getButtonPosition().Normalized()*boundary-rad;
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

    private Vector2 getButtonPosition()
    {
        return Position+rad;
    }

    public Vector2 getValue()
    {
        if(!useAccelerometer)
        {
            if(getButtonPosition().Length()>threshold)
            {
                return getButtonPosition().Normalized();
            }
            else
            {
                return Vector2.Zero;
            }
        }
        else
        {
            Vector3 acc=Input.GetAccelerometer();
            Vector2 accel=new Vector2((int)acc.x,(int)acc.y);
            return accel;
        }
    }



}
