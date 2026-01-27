using Godot;
using System;

public class PlayerCamera : Camera2D
{

    [Export] private float SMOOTHING_SPEED=2f;

    public static PlayerCamera instance;
    private const float ROT=0.001f;
    public int direction;

    public PlayerCamera() : base()
    {
        instance=this;
        direction=0;
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

}
