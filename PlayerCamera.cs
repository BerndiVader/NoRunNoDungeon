using Godot;
using System;

public class PlayerCamera : Camera2D
{

    [Export] private float SMOOTHING_SPEED=2f;

    private const float ROT=0.001f;
    private const float MIN_ZOOM=0.5f;
    private const float ZOOM_PADDING=1.30f;
    private const float MAX_FRAMING_DISTANCE=256f;
    private const float FRAMING_SMOOTHING_SPEED=4f;
    public static PlayerCamera instance;
    private static Rect2 DEFAULT_LIMIT=new Rect2(Vector2.Zero,World.RESOLUTION);

    public int direction;
    private Node2D framedTarget;
    private Vector2 prevZoom,prevPosition;
    private Vector2 framingZoom,framingPosition;
    private bool framingOrigin,framing;

    public PlayerCamera() : base()
    {
        instance=this;
        direction=0;
    }

    public override void _Ready()
    {
        FixedCamMode();
    }

    public override void _PhysicsProcess(float delta)
    {
        if(direction!=0f)
        {
            float target=direction*0.02f;
            Rotation=Mathf.MoveToward(Rotation,target,ROT);

        }
        else if(Rotation!=0f)
        {
            Rotation=Mathf.MoveToward(Rotation,0f,ROT);
        }

        if(framing)
        {
            float weight=1f-Mathf.Exp(-FRAMING_SMOOTHING_SPEED*delta);
            Zoom=Zoom.LinearInterpolate(framingZoom,weight);
            Position=Position.LinearInterpolate(framingPosition,weight);

            if(Zoom.DistanceSquaredTo(framingZoom)<0.001f&&Position.DistanceSquaredTo(framingPosition)<0.1f)
            {
                Zoom=framingZoom;
                Position=framingPosition;
                framing=false;
                if(framedTarget==null)
                {
                    framingOrigin=false;
                }
            }
        }
    }

    public void ResetSmoothingSpeed()
    {
        SmoothingSpeed=SMOOTHING_SPEED;
    }

    public void FreeCamMode(Rect2 mapMeasures)
    {
        LimitLeft=(int)mapMeasures.Position.x*16;
        LimitTop=(int)mapMeasures.Position.y*16;
        LimitRight=(int)mapMeasures.Size.x*16;
        LimitBottom=(int)mapMeasures.Size.y*16;
    }

    public void FixedCamMode()
    {
        LimitLeft=(int)DEFAULT_LIMIT.Position.x;
        LimitTop=(int)DEFAULT_LIMIT.Position.y;
        LimitRight=(int)DEFAULT_LIMIT.Size.x;
        LimitBottom=(int)DEFAULT_LIMIT.Size.y;
    }

    public void Framing(Node2D target)
    {
        if(World.state!=Gamestate.RUNNING_FREECAM)
        {
            StopFraming(target);
            return;
        }

        Vector2 viewportSize=World.RESOLUTION;

        Vector2 playerPosition=Player.instance.GlobalPosition;
        Vector2 targetPosition=target.GlobalPosition;
        if(playerPosition.DistanceTo(targetPosition)>MAX_FRAMING_DISTANCE)
        {
            StopFraming(target);
            return;
        }

        if(framedTarget!=target)
        {
            if(!framingOrigin)
            {
                prevZoom=Zoom;
                prevPosition=Position;
                framingOrigin=true;
            }
            framedTarget=target;
        }

        Vector2 separation=(targetPosition-playerPosition).Abs();
        float requiredZoom=Mathf.Max
        (
            separation.x/viewportSize.x,
            separation.y/viewportSize.y
        )*ZOOM_PADDING;
        float zoom=Mathf.Max(MIN_ZOOM,requiredZoom);

        framingZoom=new Vector2(zoom,zoom);
        framingPosition=Player.instance.ToLocal((playerPosition+targetPosition)*0.5f);
        framing=true;
    }

    public void StopFraming(Node2D target)
    {
        if(!framingOrigin||framedTarget!=target)
        {
            return;
        }

        framedTarget=null;
        framingZoom=prevZoom;
        framingPosition=prevPosition;
        framing=true;
    }

}
