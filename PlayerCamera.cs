using Godot;
using System;

public class PlayerCamera : Camera2D
{
    public static PlayerCamera instance;
    private float rot=0.001f;
    public int direction;
    private Vector2 size;

    public PlayerCamera() : base()
    {
        instance=this;
        direction=0;
    }

    public override void _Ready()
    {
        size=GetViewportRect().Size;
    }

    public override void _PhysicsProcess(float delta)
    {
        if(direction!=0)
        {
            if((direction==-1&&Rotation>-0.02f)||(direction==1&&Rotation<0.02f))
            {
                Rotation+=rot*Mathf.Sign(direction);
            }
        }
        else if(Rotation!=0)
        {
            Rotation-=rot*Mathf.Sign(Rotation);
        }
    }

    public void resetSize()
    {
        size=GetViewportRect().Size;
    }

    public Vector2 relativePosition()
    {
        return GetCameraScreenCenter()-(GetViewportRect().Size*Zoom*0.5f);
    }

}
