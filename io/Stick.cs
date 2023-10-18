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
    private Vector2 hidePosition=new Vector2(-200f,-200f);
    private Vector2 approxPosition=new Vector2(-16f,-16f);
    public bool useAccelerometer;

    public override void _Ready()
    {
        touch=GetParent<Touch>();
        touch.Position=hidePosition;
        touch.oPosition=Position;

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
            Vector2 diffPos=(Vector2.Zero-rad-Position)*touch.Scale;
            Position+=diffPos*returnAccel*delta;
            if(Position.IsEqualApprox(approxPosition)) 
            {
                touch.Position=hidePosition;
                touch.oPosition=hidePosition;
                onGoing--;
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

            if(d!=null)
            {
                position=d.Position;
                index=d.Index;
            }
            else
            {
                position=t.Position;
                index=t.Index;
                
                if(@t.IsPressed()&&position.x<256)
                {
                    touch.GlobalPosition=position;
                    touch.oPosition=touch.Position;
                }
            }
            
            float distCenter=(position-GlobalPosition).Length();

            if(distCenter<=boundary||index==onGoing)
            {
                GlobalPosition=position-rad;
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
