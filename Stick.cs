using Godot;
using System;

public class Stick : TouchScreenButton
{
    Touch touch;
    Vector2 rad=new Vector2(16f,16f);
    float boundary=32f;
    int ongoing=-1;
    float returnAccel=20f;
    float threshold=10f;

    public override void _Ready()
    {
        touch=GetParent<Touch>();
        touch.Position=new Vector2(-100,-100);

    }

    public override void _Process(float delta)
    {
        if(ongoing==-1)
        {
            Vector2 diffPos=Vector2.Zero-rad-Position;
            Position+=diffPos*returnAccel*delta;
            if((Position.IsEqualApprox(new Vector2(-16f,-16f)))) touch.Position=new Vector2(-100,-100);
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

            if(distCenter<=boundary*GlobalScale.x||index==ongoing)
            {
                GlobalPosition=position-rad*GlobalScale;
                if(getButtonPosition().Length()>boundary)
                {
                    Position=getButtonPosition().Normalized()*boundary-rad;
                }
                ongoing=index;
            }

        }

        if(@event is InputEventScreenTouch && !@event.IsPressed())
        {
            InputEventScreenTouch eventScreenTouch=@event as InputEventScreenTouch;
            if(eventScreenTouch.Index==ongoing)
            {
                ongoing=-1;
            }
        }
    }

    Vector2 getButtonPosition()
    {
        return Position+rad;
    }

    public Vector2 getValue()
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



}
