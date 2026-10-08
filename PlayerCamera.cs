using Godot;
using System;

public class PlayerCamera : Camera2D
{

    [Export] private float SMOOTHING_SPEED=2f;

    private const float ROT=0.001f;
    public static PlayerCamera instance;
    private static Rect2 DEFAULT_LIMIT=new Rect2(Vector2.Zero,World.RESOLUTION);
    public int direction;

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

}
