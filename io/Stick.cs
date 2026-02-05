using Godot;
using System;

public class Stick : TouchScreenButton
{
    private readonly Vector2 RAD=new Vector2(16f,16f);
    private readonly Vector2 HIDE_POSITION=new Vector2(-100f,-100f);
    private readonly Vector2 APPROX_POSITION=new Vector2(-16f,-16f);

    private const float BOUNDARY=32f;
    private const float RETURN_ACCEL=60f;
    private const float THRESHOLD=10f;

    private int onGoing=-1;
    private bool visible;
    public bool useAccelerometer;

    private Touch touch;

    public override void _Ready()
    {
        SetProcess(false);

        touch=GetParent<Touch>();
        touch.Position=HIDE_POSITION;
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
            Vector2 diffPos=Vector2.Zero-RAD-Position;
            Position+=diffPos*RETURN_ACCEL*delta;
            if(visible&&Position.IsEqualApprox(APPROX_POSITION)) 
            {
                touch.Position=HIDE_POSITION;
                touch.oPosition=HIDE_POSITION;
                visible=false;
                onGoing--;
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        using(@event)
        {
            if((@event.IsPressed()&&@event is InputEventScreenTouch)||@event is InputEventScreenDrag)
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

                if(distCenter<=BOUNDARY||index==onGoing)
                {
                    Position=position-touch.Position-RAD;
                    if(GetButtonPosition().Length()>BOUNDARY)
                    {
                        Position=GetButtonPosition().Normalized()*BOUNDARY-RAD;
                    }
                    onGoing=index;
                }
            }

            if(!@event.IsPressed()&&@event is InputEventScreenTouch)
            {
                InputEventScreenTouch e=@event as InputEventScreenTouch;
                if(e.Index==onGoing)
                {
                    onGoing=-1;
                }
            }
            
        }
    }

    private Vector2 GetButtonPosition()
    {
        return Position+RAD;
    }

    public Vector2 GetValue()
    {
        if(!useAccelerometer)
        {
            if(GetButtonPosition().Length()>THRESHOLD)
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
